using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that creates multiple physics bodies in a single batch operation.
  /// Much more efficient than creating bodies one at a time.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(BuildPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct BatchBodyCreationSystem : ISystem
  {
    public struct BatchBodyRequest : IComponentData
    {
      public BlobAssetReference<BatchBodyData> BatchData;
      public float2 SpawnCenter;
      public float SpawnRadius;
      public bool RandomizePositions;
      public bool RandomizeRotations;
      public uint RandomSeed;
    }

    public struct BatchBodyData
    {
      public int Count;
      public PhysicsBody.BodyType BodyType;
      public EntityArchetype BodyArchetype;
      public float2 BaseVelocity;
      public float VelocityRandomness;
      public float MinSize;
      public float MaxSize;
      public PhysicsMaterial Material;
      public CollisionFilter Filter;
      public BlobArray<float2> Positions;  // Optional predefined positions
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var physicsWorld = physicsWorldSingleton.World;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (request, entity) in
          SystemAPI.Query<RefRO<BatchBodyRequest>>()
          .WithEntityAccess()) {

        if (!request.ValueRO.BatchData.IsCreated)
          continue;

        ref var batchData = ref request.ValueRO.BatchData.Value;
        var random = new Unity.Mathematics.Random(request.ValueRO.RandomSeed);

        // Prepare body definitions
        var bodyDefinitions = new NativeArray<PhysicsBodyDefinition>(batchData.Count, Allocator.Temp);

        for (int i = 0; i < batchData.Count; i++) {
          float2 position;

          if (batchData.Positions.Length > i) {
            position = batchData.Positions[i] + request.ValueRO.SpawnCenter;
          } else if (request.ValueRO.RandomizePositions) {
            var angle = random.NextFloat(0, math.PI * 2f);
            var radius = random.NextFloat(0, request.ValueRO.SpawnRadius);
            position = request.ValueRO.SpawnCenter + new float2(
                math.cos(angle) * radius,
                math.sin(angle) * radius
            );
          } else {
            position = request.ValueRO.SpawnCenter;
          }

          var rotation = request.ValueRO.RandomizeRotations ?
              random.NextFloat(0, math.PI * 2f) : 0f;

          var velocity = batchData.BaseVelocity;
          if (batchData.VelocityRandomness > 0) {
            velocity += random.NextFloat2(-1f, 1f) * batchData.VelocityRandomness;
          }

          bodyDefinitions[i] = new PhysicsBodyDefinition
          {
            type = batchData.BodyType,
            position = position,
            rotation = PhysicsUtility.GetRotationZ(rotation),
            linearVelocity = velocity,
            angularVelocity = 0f,
            linearDamping = 0f,
            angularDamping = 0f,
            gravityScale = 1f,
            sleepingAllowed = true,
            awake = true,
            enabled = true
          };
        }

        // Create all bodies in one batch call
        var bodies = physicsWorld.CreateBodyBatch(bodyDefinitions);

        // Create entities for these bodies
        var archetype = batchData.BodyArchetype;
        var entities = state.EntityManager.CreateEntity(archetype, batchData.Count, Allocator.Temp);

        // Set up entity components
        for (int i = 0; i < batchData.Count; i++) {
          var bodyEntity = entities[i];

          // Core components
          ecb.SetComponent(bodyEntity, new PhysicsBodyComponent
          {
            Body = bodies[i],
            WorldIndex = 0
          });

          ecb.SetComponent(bodyEntity, LocalTransform.FromPositionRotation(
              new float3(bodyDefinitions[i].position.x, bodyDefinitions[i].position.y, 0),
              quaternion.RotateZ(bodyDefinitions[i].rotation.angle)
          ));

          ecb.SetComponent(bodyEntity, new PhysicsVelocity
          {
            Linear = bodyDefinitions[i].linearVelocity,
            Angular = 0f
          });

          ecb.SetComponent(bodyEntity, batchData.Material);
          ecb.SetComponent(bodyEntity, batchData.Filter);

          // Random shape size
          var size = random.NextFloat(batchData.MinSize, batchData.MaxSize);
          ecb.SetComponent(bodyEntity, new PhysicsShapeCircle
          {
            Radius = size,
            Center = float2.zero
          });
        }

        // Store entity reference in body userData
        for (int i = 0; i < bodies.Length; i++) {
          // Copy out the struct to modify it
          var body = bodies[i];
          body.SetEntityUserData(entities[i]);
          // Write entire modified struct back
          bodies[i] = body;
        }

        bodyDefinitions.Dispose();
        entities.Dispose();

        // Remove the request
        ecb.RemoveComponent<BatchBodyRequest>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}