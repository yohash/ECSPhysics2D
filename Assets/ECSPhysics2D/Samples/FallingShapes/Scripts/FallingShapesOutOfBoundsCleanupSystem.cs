using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Destroys physics bodies that fall below the world bounds.
  /// Prevents infinite accumulation of out-of-bounds entities.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct FallingShapesOutOfBoundsCleanupSystem : ISystem
  {
    private const float DestroyThreshold = -15f;  // Y position below which entities are destroyed

    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<FallingShapesSampleConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<FallingShapesSampleConfig>(out var config))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Check all dynamic bodies
      foreach (var (transform, physicsBody, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsDynamicTag>()
          .WithEntityAccess()) {
        // Destroy if fallen too far below ground
        if (transform.ValueRO.Position.y < DestroyThreshold) {
          // Destroy physics body FIRST
          if (physicsBody.ValueRO.IsValid) {
            physicsBody.ValueRO.Body.Destroy();
          }

          // Then destroy entity
          ecb.DestroyEntity(entity);
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}