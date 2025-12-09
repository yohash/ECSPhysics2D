using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D.Samples.FallingCascade
{
  /// <summary>
  /// Responds to collision events by modifying circle collision filters.
  /// After a circle hits a terrain layer once, it no longer collides with that layer.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
  [BurstCompile]
  public partial struct CascadeCollisionResponseSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<CascadeCircleSpawner>();
      state.RequireForUpdate<PhysicsEventsSingleton>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      ref var buffers = ref eventsSingleton.Buffers;

      // Process all collision events
      for (int i = 0; i < buffers.Collisions.Count; i++) {
        var evt = buffers.Collisions[i];

        // Only process collision begin events (initial impact)
        if (evt.EventType != CollisionEventType.Begin)
          continue;

        // First, we'll check for two-circle/no-circle escape cases
        bool aIsCircle = SystemAPI.HasComponent<CascadeCircleTag>(evt.EntityA);
        bool bIsCircle = SystemAPI.HasComponent<CascadeCircleTag>(evt.EntityB);

        if (aIsCircle && bIsCircle) {
          // Both entities are cascade circles, skip
          continue;
        }

        if (!aIsCircle && !bIsCircle) {
          // Neither entity is a cascade circle, skip
          continue;
        }

        // At this point, we know one entity is a circle and the other is not
        bool aIsTerrain = SystemAPI.HasComponent<TerrainTag>(evt.EntityA);
        bool bIsTerrain = SystemAPI.HasComponent<TerrainTag>(evt.EntityB);

        // Determine which entity is the terrain and which is the circle
        Entity circleEntity = aIsCircle ? evt.EntityA : evt.EntityB;
        Entity terrainEntity = aIsTerrain ? evt.EntityA : evt.EntityB;

        // Get terrain's collision layer
        if (!SystemAPI.HasComponent<CollisionFilter>(terrainEntity))
          continue;

        var terrainFilter = SystemAPI.GetComponent<CollisionFilter>(terrainEntity);
        int terrainLayer = GetLayerFromCategoryBits(terrainFilter.CategoryBits);

        // Check if circle already hit this layer
        var layersHit = SystemAPI.GetBuffer<TerrainLayersHit>(circleEntity);
        bool alreadyHit = false;
        for (int j = 0; j < layersHit.Length; j++) {
          if (layersHit[j].LayerIndex == terrainLayer) {
            alreadyHit = true;
            break;
          }
        }

        if (alreadyHit)
          continue;

        // Record this hit
        layersHit.Add(new TerrainLayersHit { LayerIndex = terrainLayer });

        // Update circle's collision filter to exclude this layer
        var circleFilter = SystemAPI.GetComponentRW<CollisionFilter>(circleEntity);
        uint layerBit = (uint)(1 << terrainLayer);
        circleFilter.ValueRW.MaskBits &= ~layerBit; // Remove this layer from mask

        // Fetch the buffer of shape references for this circle and update their filters
        var shapes = SystemAPI.GetBuffer<PhysicsShapeReference>(circleEntity);
        var contactFilter = new PhysicsShape.ContactFilter
        {
          categories = circleFilter.ValueRO.Categories(),
          contacts = circleFilter.ValueRO.Mask(),
          groupIndex = circleFilter.ValueRO.GroupIndex
        };

        for (int k = 0; k < shapes.Length; k++) {
          if (shapes[k].Shape.isValid) {
            shapes[k].Shape.contactFilter = contactFilter;
          }
        }
      }
    }

    private int GetLayerFromCategoryBits(uint categoryBits)
    {
      // Find which bit is set (assumes single bit set)
      for (int i = 0; i < 32; i++) {
        if ((categoryBits & (1u << i)) != 0) {
          return i;
        }
      }
      return -1;
    }
  }
}