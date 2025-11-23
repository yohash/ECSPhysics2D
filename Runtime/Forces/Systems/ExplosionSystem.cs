using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Optimized explosion system using native Box2D Explode() method.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(BatchForceApplicationSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ExplosionSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var physicsWorld = physicsWorldSingleton.World;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (explosion, entity) in
          SystemAPI.Query<RefRO<PhysicsExplosion>>()
          .WithEntityAccess()) {

        // Use native Box2D explosion method - highly optimized
        var definition = new PhysicsWorld.ExplosionDefinition
        {
          position = explosion.ValueRO.Center,
          radius = explosion.ValueRO.Radius,
          hitCategories = explosion.ValueRO.AffectedLayers,
          impulsePerLength = explosion.ValueRO.Force / explosion.ValueRO.Radius, // TODO - what is this
          // this is the distance OUTSIDE radius that force falls off to zero
          // 0 should mean immeidate. 
          // TODO - expose this in the PhysicsExplosion component?
          falloff = 0
        };

        // Note: Box2D's Explode() doesn't support custom falloff or layer filtering
        // If we need those, we'd have to implement custom explosion logic:

        if (explosion.ValueRO.Falloff != PhysicsExplosion.ExplosionFalloff.Linear ||
            explosion.ValueRO.AffectedLayers != ~0u) {
          // Custom explosion implementation for non-default settings
          ApplyCustomExplosion(physicsWorld, explosion.ValueRO);
        } else {
          physicsWorld.Explode(definition);
        }

        // Remove explosion component (one-shot)
        ecb.RemoveComponent<PhysicsExplosion>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void ApplyCustomExplosion(PhysicsWorld physicsWorld, PhysicsExplosion explosion)
    {
      // Query all bodies in explosion radius
      var aabb = new PhysicsAABB
      {
        lowerBound = explosion.Center - new float2(explosion.Radius, explosion.Radius),
        upperBound = explosion.Center + new float2(explosion.Radius, explosion.Radius)
      };

      var filter = new PhysicsQuery.QueryFilter
      {
        categories = explosion.AffectedLayers,
        hitCategories = explosion.AffectedLayers,
      };

      var overlaps = physicsWorld.OverlapAABB(aabb, filter);

      // Apply impulse to each body based on distance and falloff
      for (int i = 0; i < overlaps.Length; i++) {

        var body = overlaps[i].shape.body;

        if (!body.isValid || body.type != PhysicsBody.BodyType.Dynamic)
          continue;

        var bodyPos = body.position;
        var impulse = ForceUtility.CalculateExplosionImpulse(
            bodyPos,
            explosion.Center,
            explosion.Force,
            explosion.Radius,
            explosion.Falloff
        );

        if (math.lengthsq(impulse) > 0.001f) {
          body.ApplyLinearImpulse(impulse, bodyPos);
        }
      }

      overlaps.Dispose();
    }
  }
}