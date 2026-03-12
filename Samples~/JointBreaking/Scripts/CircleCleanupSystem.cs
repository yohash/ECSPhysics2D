using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Removes circles that fall below the world or when max count exceeded.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateAfter(typeof(CircleSpawnerSystem))]
  [BurstCompile]
  public partial struct CircleCleanupSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Remove circles below Y = -15
      const float minY = -15f;

      foreach (var (transform, entity) in
          SystemAPI.Query<RefRO<LocalTransform>>()
          .WithAll<SpawnedCircleTag>()
          .WithEntityAccess()) {
        if (transform.ValueRO.Position.y < minY) {
          ecb.DestroyEntity(entity);
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
