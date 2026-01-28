using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Destroys debris entities that have exceeded their lifetime.
  /// Also cleans up debris that falls too far below the scene.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct DebrisLifetimeSystem : ISystem
  {
    private const float CleanupThresholdY = -20f;

    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<MultiWorldDemoConfig>();
      state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      float currentTime = (float)SystemAPI.Time.ElapsedTime;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Check each debris entity
      foreach (var (debris, bodyComponent, entity) in
          SystemAPI.Query<RefRO<DebrisTag>, RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsBodyInitialized>()
          .WithEntityAccess()) {
        bool shouldDestroy = false;

        // Check lifetime expiration
        if (debris.ValueRO.IsExpired(currentTime)) {
          shouldDestroy = true;
        }

        // Check if fallen out of bounds
        if (bodyComponent.ValueRO.IsValid) {
          var body = bodyComponent.ValueRO.Body;
          if (body.position.y < CleanupThresholdY) {
            shouldDestroy = true;
          }
        }

        if (shouldDestroy) {
          // Destroy physics body
          if (bodyComponent.ValueRO.IsValid) {
            var world = singleton.GetWorld(bodyComponent.ValueRO.WorldIndex);
            if (world.isValid) {
              bodyComponent.ValueRO.Body.Destroy();
            }
          }

          // Destroy entity
          ecb.DestroyEntity(entity);
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
