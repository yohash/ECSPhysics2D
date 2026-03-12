using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Initializes static bodies that exist in multiple physics worlds.
  /// Creates independent body handles for each world specified in WorldMask.
  /// 
  /// Runs once per entity during initialization.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(BuildPhysicsWorldSystem))]
  //[BurstCompile]
  public partial struct MultiWorldStaticBodyInitializationSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<MultiWorldDemoConfig>();
      state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Process uninitialized multi-world static bodies
      foreach (var (transform, multiBody, shape, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRW<MultiWorldStaticBodyComponent>, RefRO<PhysicsShapeBox>>()
          .WithNone<MultiWorldStaticBodyInitialized>()
          .WithEntityAccess()) {
        var shapeValue = shape.ValueRO;
        InitializeStaticBody(ref state, singleton, transform, multiBody, entity, ecb);
      }

      // Circle shapes
      foreach (var (transform, multiBody, shape, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRW<MultiWorldStaticBodyComponent>, RefRO<PhysicsShapeCircle>>()
          .WithNone<MultiWorldStaticBodyInitialized>()
          .WithEntityAccess()) {
        InitializeStaticBody(ref state, singleton, transform, multiBody, entity, ecb);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void InitializeStaticBody(
      ref SystemState state,
      PhysicsWorldSingleton singleton,
      RefRO<LocalTransform> transform,
      RefRW<MultiWorldStaticBodyComponent> multiBody,
      Entity entity,
      EntityCommandBuffer ecb)
    {
      var pos = transform.ValueRO.Position.xy;
      var rot = PhysicsUtility.GetRotationZ(transform.ValueRO.Rotation);

      // Create body in each flagged world
      for (int i = 0; i < singleton.WorldCount; i++) {
        if (!multiBody.ValueRO.ExistsInWorld(i))
          continue;

        var world = singleton.GetWorld(i);
        if (!world.isValid)
          continue;

        // Transform
        //ecb.AddComponent(entity, LocalTransform.FromPosition(pos.x, pos.y, 0));

        // Physics body (uninitialized - BuildPhysicsWorldSystem will create it)
        ecb.AddComponent(entity, new PhysicsBodyComponent
        {
          WorldIndex = i,
          GravityScale = 1f
        });

        // Material
        ecb.AddComponent(entity, new PhysicsMaterial
        {
          Friction = 0.4f,
          Bounciness = 0.2f,
          Density = 1f,
          RollingResistance = 0.05f
        });

        // Collision filter - debris layer
        ecb.AddComponent(
          entity,
          CollisionFilter.CreateFromMask(
            MultiWorldDemoLayers.TerrainLayer,
            MultiWorldDemoLayers.TerrainCollidesWith)
        );

        //multiBody.ValueRW.SetBodyForWorld(i, body);
      }

      ecb.AddComponent<MultiWorldStaticBodyInitialized>(entity);
    }
  }
}
