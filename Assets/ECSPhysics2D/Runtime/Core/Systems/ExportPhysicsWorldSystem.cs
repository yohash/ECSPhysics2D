using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D
{
  /// <summary>
  /// Exports physics simulation results back to ECS components.
  /// Runs after simulation to sync physics state TO ECS.
  ///
  /// Each entity's transform is read from its assigned physics world
  /// (determined by PhysicsBodyComponent.WorldIndex).
  ///
  /// When a physics entity has a Parent component, the world-space position
  /// returned by Box2D is converted to local space before writing to LocalTransform.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ExportPhysicsWorldSystem : ISystem
  {
    private ComponentLookup<Parent> _parentLookup;
    private ComponentLookup<LocalToWorld> _localToWorldLookup;

    public void OnCreate(ref SystemState state)
    {
      _parentLookup = state.GetComponentLookup<Parent>(isReadOnly: true);
      _localToWorldLookup = state.GetComponentLookup<LocalToWorld>(isReadOnly: true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      _parentLookup.Update(ref state);
      _localToWorldLookup.Update(ref state);

      // Step 1: Sync Dynamic body transforms FROM physics
      SyncDynamicTransforms(ref state);

      // Step 2: Handle destroyed bodies
      CleanupDestroyedBodies(ref state);
    }

    [BurstCompile]
    private void SyncDynamicTransforms(ref SystemState state)
    {
      // Dynamic bodies: Physics drives ECS transform
      foreach (var (transform, bodyComponent, preservation, entity) in
          SystemAPI.Query<RefRW<LocalTransform>, RefRO<PhysicsBodyComponent>, RefRO<PhysicsTransformPreservation>>()
          .WithAll<PhysicsDynamicTag, PhysicsBodyInitialized>()
          .WithEntityAccess()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        var body = bodyComponent.ValueRO.Body;

        // Reconstruct world-space position (physics operates in XY; Z is preserved from creation)
        var worldPos = new float3(body.position.x, body.position.y, preservation.ValueRO.ZPosition);
        var worldRot = quaternion.RotateZ(body.rotation.angle);

        if (_parentLookup.TryGetComponent(entity, out var parent)) {
          // Entity has a parent: convert world-space physics result to local space
          var parentLTW = _localToWorldLookup[parent.Value];
          var invParent = math.inverse(parentLTW.Value);
          transform.ValueRW.Position = math.transform(invParent, worldPos);
          transform.ValueRW.Rotation = math.mul(math.inverse(math.rotation(parentLTW.Value)), worldRot);
        } else {
          // Root entity: local space == world space
          transform.ValueRW.Position = worldPos;
          transform.ValueRW.Rotation = worldRot;
        }

        // Scale is preserved from original value (physics doesn't affect scale)
        transform.ValueRW.Scale = preservation.ValueRO.Scale.x;
      }
    }

    [BurstCompile]
    private void CleanupDestroyedBodies(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Check for invalid bodies and clean them up
      foreach (var (bodyComponent, entity) in
        SystemAPI.Query<RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsBodyInitialized>()
          .WithEntityAccess()) {
        if (!bodyComponent.ValueRO.IsValid) {
          // Body was destroyed, remove physics components
          ecb.RemoveComponent<PhysicsBodyComponent>(entity);
          ecb.RemoveComponent<PhysicsBodyInitialized>(entity);
          ecb.RemoveComponent<PhysicsTransformPreservation>(entity);
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
