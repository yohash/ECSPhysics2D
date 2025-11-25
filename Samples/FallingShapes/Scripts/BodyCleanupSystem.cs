using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Cleans up old bodies when count exceeds MaxBodies limit.
  /// Runs after physics export to avoid destroying bodies mid-simulation.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct BodyCleanupSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<ShapeSpawnerConfig>(out var config))
        return;

      // Count dynamic bodies
      var dynamicQuery = SystemAPI.QueryBuilder()
          .WithAll<PhysicsDynamicTag, PhysicsBodyComponent>()
          .Build();

      var bodyCount = dynamicQuery.CalculateEntityCount();

      if (bodyCount <= config.MaxBodies)
        return;

      // Destroy oldest bodies (excess count)
      var excessCount = bodyCount - config.MaxBodies;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      var entities = dynamicQuery.ToEntityArray(Allocator.Temp);

      // Destroy first N entities (oldest)
      for (int i = 0; i < excessCount && i < entities.Length; i++) {
        ecb.DestroyEntity(entities[i]);
      }

      entities.Dispose();
      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}