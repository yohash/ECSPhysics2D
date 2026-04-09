using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Builds/Updates the physics world from ECS components.
  /// Runs before simulation to sync ECS state TO physics.
  /// 
  /// Supports multi-world: each body is created in the world specified by
  /// PhysicsBodyComponent.WorldIndex.
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
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      // Step 1: Create new physics bodies (routed to correct world by WorldIndex)
      CreateNewPhysicsBodies(ref state, singleton);

      // Step 2: Sync Kinematic/Static transforms TO physics
      SyncKinematicTransforms(ref state);
      SyncStaticTransforms(ref state);
    }

    [BurstCompile]
    private void CreateNewPhysicsBodies(ref SystemState state, PhysicsWorldSingleton singleton)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Create Dynamic bodies
      foreach (var (transform, bodyComponent, entity) in
          SystemAPI.Query<RefRO<LocalTransform>, RefRW<PhysicsBodyComponent>>()
            .WithAll<PhysicsDynamicTag>()
            .WithNone<PhysicsBodyInitialized>()
            .WithEntityAccess()) {
        var worldIndex = bodyComponent.ValueRO.WorldIndex;

        if (!singleton.IsValidWorldIndex(worldIndex)) {
          // Fall back to default world
          worldIndex = 0;
          bodyComponent.ValueRW.WorldIndex = 0;
        }

        var physicsWorld = singleton.GetWorld(worldIndex);

        var bodyDef = new PhysicsBodyDefinition
        {
          type = PhysicsBody.BodyType.Dynamic,
          position = transform.ValueRO.Position.xy,
          rotation = PhysicsUtility.GetRotationZ(transform.ValueRO.Rotation),
          fastCollisionsAllowed = bodyComponent.ValueRO.EnableCCD,
          linearVelocity = bodyComponent.ValueRO.InitialLinearVelocity,
          angularVelocity = bodyComponent.ValueRO.InitialAngularVelocity,
          linearDamping = bodyComponent.ValueRO.LinearDamping,
          angularDamping = bodyComponent.ValueRO.AngularDamping,
          gravityScale = bodyComponent.ValueRO.GravityScale,
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
        var worldIndex = bodyComponent.ValueRO.WorldIndex;

        if (!singleton.IsValidWorldIndex(worldIndex)) {
          worldIndex = 0;
          bodyComponent.ValueRW.WorldIndex = 0;
        }

        var physicsWorld = singleton.GetWorld(worldIndex);

        var bodyDef = new PhysicsBodyDefinition
        {
          type = PhysicsBody.BodyType.Kinematic,
          position = transform.ValueRO.Position.xy,
          rotation = PhysicsUtility.GetRotationZ(transform.ValueRO.Rotation),
          fastCollisionsAllowed = bodyComponent.ValueRO.EnableCCD,
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
        var worldIndex = bodyComponent.ValueRO.WorldIndex;

        if (!singleton.IsValidWorldIndex(worldIndex)) {
          worldIndex = 0;
          bodyComponent.ValueRW.WorldIndex = 0;
        }

        var physicsWorld = singleton.GetWorld(worldIndex);

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
    private void SyncKinematicTransforms(ref SystemState state)
    {
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
    private void SyncStaticTransforms(ref SystemState state)
    {
      // Static bodies typically don't move after creation.
      // No sync needed - body already has correct position from creation.
      // If runtime repositioning is needed in the future, implement explicit
      // user-triggered sync via a request component.
    }
  }
}
