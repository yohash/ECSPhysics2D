using Unity.Collections;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Handles runtime shape updates - material changes, resizing, etc.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(CompoundShapeBuilderSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  public partial struct ShapeUpdateSystem : ISystem
  {
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (body, updateRequest, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRO<ShapeUpdateRequest>>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        switch (updateRequest.ValueRO.Type) {
          case ShapeUpdateRequest.UpdateType.Recreate:
            // Destroy all existing shapes
            // TBD - do we even want to offer this feature?
            // TODO - this method:
            //body.ValueRO.Body.DestroyAllShapes();

            // Remove created tag to trigger recreation
            ecb.RemoveComponent<ShapesCreatedTag>(entity);
            break;

          case ShapeUpdateRequest.UpdateType.ModifyMaterial:
            // Update material properties on existing shapes
            // This would require storing shape handles, which we'll add in a future phase
            break;
        }

        // Remove the update request
        ecb.RemoveComponent<ShapeUpdateRequest>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}