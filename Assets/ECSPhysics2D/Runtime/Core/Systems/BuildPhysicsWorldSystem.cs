using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Builds/Updates the physics world from ECS components.
  /// Runs before simulation to sync ECS state TO physics.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct BuildPhysicsWorldSystem : ISystem
  {
    private EntityQuery uninitializedBodies;

    public void OnCreate(ref SystemState state)
    {
      // Query for entities that need physics body creation
      uninitializedBodies = state.GetEntityQuery(
          ComponentType.ReadOnly<LocalTransform>(),
          ComponentType.ReadWrite<PhysicsBodyComponent>(),
          ComponentType.Exclude<PhysicsBodyInitialized>()
      );
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var physicsWorld = physicsWorldSingleton.World;

      // ===== Step 1: Create new physics bodies =====
      CreateNewPhysicsBodies(ref state, physicsWorld);

      // ===== Step 2: Sync Kinematic/Static transforms TO physics =====
      // (Dynamic bodies are driven BY physics, so we don't sync them here)
      SyncKinematicTransforms(ref state, physicsWorld);
      SyncStaticTransforms(ref state, physicsWorld);

      // ===== Step 3: Apply velocities to kinematic bodies =====
      ApplyKinematicVelocities(ref state);
    }

    [BurstCompile]
    private void CreateNewPhysicsBodies(ref SystemState state, PhysicsWorld physicsWorld)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Create Dynamic bodies
      foreach (var (transform, bodyComponent, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRW<PhysicsBodyComponent>>()
            .WithAll<PhysicsDynamicTag>()
            .WithNone<PhysicsBodyInitialized>()
            .WithEntityAccess()) {
        var bodyDef = new PhysicsBodyDefinition
        {
          type = PhysicsBody.BodyType.Dynamic,
          position = transform.ValueRO.Position.xy,
          rotation = PhysicsUtility.GetRotationZ(transform.ValueRO.Rotation),
          linearVelocity = float2.zero,
          angularVelocity = 0f,
          linearDamping = 0f,
          angularDamping = 0f,
          gravityScale = 1f,
          sleepingAllowed = true,
          awake = true,
          enabled = true
        };

        bodyComponent.ValueRW.Body = physicsWorld.CreateBody(bodyDef);

        // Store entity reference in userData for callbacks
        bodyComponent.ValueRW.Body.SetEntityUserData(entity);

        // Mark as initialized and preserve Z position
        ecb.AddComponent<PhysicsBodyInitialized>(entity);
        ecb.AddComponent(entity, new PhysicsTransformPreservation
        {
          ZPosition = transform.ValueRO.Position.z,
          Scale = transform.ValueRO.Scale
        });
      }

      // Create Kinematic bodies
      foreach (var (transform, bodyComponent, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRW<PhysicsBodyComponent>>()
          .WithAll<PhysicsKinematicTag>()
          .WithNone<PhysicsBodyInitialized>()
          .WithEntityAccess()) {
        var bodyDef = new PhysicsBodyDefinition
        {
          type = PhysicsBody.BodyType.Kinematic,
          position = transform.ValueRO.Position.xy,
          rotation = PhysicsUtility.GetRotationZ(transform.ValueRO.Rotation),
          enabled = true
        };

        bodyComponent.ValueRW.Body = physicsWorld.CreateBody(bodyDef);
        bodyComponent.ValueRW.Body.SetEntityUserData(entity);

        ecb.AddComponent<PhysicsBodyInitialized>(entity);
        ecb.AddComponent(entity, new PhysicsTransformPreservation
        {
          ZPosition = transform.ValueRO.Position.z,
          Scale = transform.ValueRO.Scale
        });
      }

      // Create Static bodies
      foreach (var (transform, bodyComponent, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRW<PhysicsBodyComponent>>()
          .WithAll<PhysicsStaticTag>()
          .WithNone<PhysicsBodyInitialized>()
          .WithEntityAccess()) {
        var bodyDef = new PhysicsBodyDefinition
        {
          type = PhysicsBody.BodyType.Static,
          position = transform.ValueRO.Position.xy,
          rotation = PhysicsUtility.GetRotationZ(transform.ValueRO.Rotation),
          enabled = true
        };

        bodyComponent.ValueRW.Body = physicsWorld.CreateBody(bodyDef);
        bodyComponent.ValueRW.Body.SetEntityUserData(entity);

        ecb.AddComponent<PhysicsBodyInitialized>(entity);
        ecb.AddComponent(entity, new PhysicsTransformPreservation
        {
          ZPosition = transform.ValueRO.Position.z,
          Scale = transform.ValueRO.Scale
        });
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    [BurstCompile]
    private void SyncKinematicTransforms(ref SystemState state, PhysicsWorld physicsWorld)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Kinematic bodies: ECS drives physics transform
      foreach (var (transform, bodyComponent) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsKinematicTag, PhysicsBodyInitialized>()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        var body = bodyComponent.ValueRO.Body;
        body.position = transform.ValueRO.Position.xy;
        body.rotation = PhysicsUtility.GetRotationZ(transform.ValueRO.Rotation);
      }
    }

    [BurstCompile]
    private void SyncStaticTransforms(ref SystemState state, PhysicsWorld physicsWorld)
    {
      // Static bodies typically don't move, but support runtime repositioning
      // Only sync if marked dirty (not implemented in Phase 1)
      foreach (var (transform, bodyComponent) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsStaticTag, PhysicsBodyInitialized>()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        // In Phase 1, we don't move static bodies after creation
        // Phase 6 will add dirty flag optimization
      }
    }

    [BurstCompile]
    private void ApplyKinematicVelocities(ref SystemState state)
    {
      // Apply velocities to kinematic bodies if specified
      foreach (var (velocity, bodyComponent) in
          SystemAPI.Query<RefRO<PhysicsVelocity>, RefRO<PhysicsBodyComponent>>()
          .WithAll<PhysicsKinematicTag, PhysicsBodyInitialized>()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        var body = bodyComponent.ValueRO.Body;
        body.linearVelocity = velocity.ValueRO.Linear;
        body.angularVelocity = velocity.ValueRO.Angular;
      }
    }
  }
}