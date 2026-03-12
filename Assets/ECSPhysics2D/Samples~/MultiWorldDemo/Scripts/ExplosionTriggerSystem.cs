using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Triggers explosion when circles fall below the threshold Y position.
  /// Creates a DebrisSpawnRequest and destroys the circle.
  /// 
  /// Uses simple Y-position check instead of collision events for sample simplicity.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct ExplosionTriggerSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<MultiWorldDemoConfig>();
      state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<MultiWorldDemoConfig>(out var configRef))
        return;

      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      ref var config = ref configRef.ValueRW;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Check each falling circle
      foreach (var (transform, bodyComponent, explosion, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRO<PhysicsBodyComponent>, RefRO<ExplosionOnContact>>()
          .WithAll<FallingCircleTag, PhysicsBodyInitialized>()
          .WithEntityAccess()) {
        // Check if below trigger threshold
        if (transform.ValueRO.Position.y > config.ExplosionTriggerY)
          continue;

        // Create debris spawn request
        var requestEntity = ecb.CreateEntity();
        ecb.AddComponent(requestEntity, new DebrisSpawnRequest
        {
          Position = transform.ValueRO.Position.xy,
          Count = explosion.ValueRO.DebrisCount,
          Radius = explosion.ValueRO.DebrisRadius,
          SpreadSpeed = explosion.ValueRO.SpreadSpeed,
          Lifetime = explosion.ValueRO.DebrisLifetime,
          TargetWorldIndex = 1  // Debris goes to World 1
        });

        // Destroy the physics body
        if (bodyComponent.ValueRO.Body.isValid) {
          var world = singleton.GetWorld(bodyComponent.ValueRO.WorldIndex);
          if (world.isValid) {
            bodyComponent.ValueRO.Body.Destroy();
          }
        }

        // Destroy the entity
        ecb.DestroyEntity(entity);
        config.CurrentCircleCount--;
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
