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
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ExportPhysicsWorldSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      // ===== Step 1: Sync Dynamic body transforms FROM physics =====
      SyncDynamicTransforms(ref state);

      // ===== Step 2: Update velocity components =====
      UpdateVelocityComponents(ref state);

      // ===== Step 3: Handle destroyed bodies =====
      CleanupDestroyedBodies(ref state);
    }

    [BurstCompile]
    private void SyncDynamicTransforms(ref SystemState state)
    {
      // Dynamic bodies: Physics drives ECS transform
      foreach (var (transform, bodyComponent, preservation) in
          SystemAPI.Query<RefRW<LocalTransform>, RefRO<PhysicsBodyComponent>, RefRO<PhysicsTransformPreservation>>()
          .WithAll<PhysicsDynamicTag, PhysicsBodyInitialized>()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        var body = bodyComponent.ValueRO.Body;

        // Update position (preserving Z)
        transform.ValueRW.Position = new float3(
            body.position.x,
            body.position.y,
            preservation.ValueRO.ZPosition
        );

        // Update rotation (2D rotation around Z axis)
        transform.ValueRW.Rotation = PhysicsUtility.CreateRotationZ(body.rotation);

        // Scale is preserved from original value (physics doesn't affect scale)
        transform.ValueRW.Scale = preservation.ValueRO.Scale.x;
      }
    }

    [BurstCompile]
    private void UpdateVelocityComponents(ref SystemState state)
    {
      // Update velocity components for all dynamic bodies
      foreach (var (velocity, bodyComponent) in
        SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsDynamicTag, PhysicsBodyInitialized>()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        var body = bodyComponent.ValueRO.Body;
        velocity.ValueRW.Linear = body.linearVelocity;
        velocity.ValueRW.Angular = body.angularVelocity;
      }

      // Also update velocity for kinematic bodies (they might have changed)
      foreach (var (velocity, bodyComponent) in
        SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsKinematicTag, PhysicsBodyInitialized>()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        var body = bodyComponent.ValueRO.Body;
        velocity.ValueRW.Linear = body.linearVelocity;
        velocity.ValueRW.Angular = body.angularVelocity;
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