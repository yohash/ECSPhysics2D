using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.FallingCascade
{
  /// <summary>
  /// Destroys circles that fall below the world.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct FallingCascadeCascadeCleanupSystem : ISystem
  {
    private const float DestroyThreshold = -5f;

    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<CascadeCircleSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<CascadeCircleSpawner>(out var config))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (transform, physicsBody, entity) in
        SystemAPI.Query<RefRO<LocalTransform>, RefRO<PhysicsBodyComponent>>()
          .WithAll<CascadeCircleTag>()
          .WithEntityAccess()) {

        if (transform.ValueRO.Position.y < DestroyThreshold) {
          // Destroy physics body first
          if (physicsBody.ValueRO.IsValid) {
            physicsBody.ValueRO.Body.Destroy();
          }

          ecb.DestroyEntity(entity);
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}