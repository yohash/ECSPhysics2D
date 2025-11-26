using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
          impulsePerLength = explosion.ValueRO.Force / explosion.ValueRO.Radius, // TODO - is this correct?
          falloff = explosion.ValueRO.Falloff
        };

        // Note: Box2D's Explode() doesn't support custom falloff or layer filtering
        // If we need those, we'd have to implement custom explosion logic
        physicsWorld.Explode(definition);

        // Destroy entity and explosion component (one-shot)
        ecb.DestroyEntity(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}