using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Optimized explosion system using native Box2D Explode() method.
  /// 
  /// Each explosion specifies which physics world to affect via WorldIndex.
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
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (explosion, entity) in
        SystemAPI.Query<RefRO<PhysicsExplosion>>()
          .WithEntityAccess()) {
        // Get the correct physics world for this explosion
        var worldIndex = explosion.ValueRO.WorldIndex;
        if (!singleton.IsValidWorldIndex(worldIndex)) {
          worldIndex = 0; // Fall back to default world
        }

        var physicsWorld = singleton.GetWorld(worldIndex);

        // Use native Box2D explosion method - highly optimized
        var definition = new PhysicsWorld.ExplosionDefinition
        {
          position = explosion.ValueRO.Center,
          radius = explosion.ValueRO.Radius,
          hitCategories = explosion.ValueRO.AffectedLayers,
          impulsePerLength = explosion.ValueRO.Force / explosion.ValueRO.Radius,
          falloff = explosion.ValueRO.Falloff
        };

        physicsWorld.Explode(definition);

        // Destroy entity and explosion component (one-shot)
        ecb.DestroyEntity(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
