using Unity.Collections;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Cleans up shapes when bodies are destroyed.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExportPhysicsWorldSystem))]
  public partial struct ShapeDestructionSystem : ISystem
  {
    public void OnUpdate(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Clean up shapes for destroyed bodies
      foreach (var (body, shapesCreated, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRO<ShapesCreatedTag>>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid) {
          // Body was destroyed, clean up shape components
          ecb.RemoveComponent<ShapesCreatedTag>(entity);
        }
      }

      // Clean up chain blobs for destroyed bodies
      foreach (var (body, chainShape, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRO<PhysicsShapeChain>>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid && chainShape.ValueRO.ChainBlob.IsCreated) {
          chainShape.ValueRO.ChainBlob.Dispose();
          ecb.RemoveComponent<PhysicsShapeChain>(entity);
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}