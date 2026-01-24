using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Creates spring platform entities from config at runtime.
  /// Runs once per config entity, then removes the config.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(BuildPhysicsWorldSystem))]
  public partial struct SpringPlatformInitializationSystem : ISystem
  {
    public void OnUpdate(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (config, configEntity) in
          SystemAPI.Query<RefRO<SpringPlatformConfig>>()
          .WithEntityAccess()) {
        CreatePlatform(ref state, ecb, config.ValueRO);

        // Remove config after initialization (one-time creation)
        ecb.DestroyEntity(configEntity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void CreatePlatform(
        ref SystemState state,
        EntityCommandBuffer ecb,
        SpringPlatformConfig config)
    {
      var position = config.SpawnPosition;

      // ===== Create Static Anchor Box =====
      var anchorEntity = ecb.CreateEntity();

      ecb.AddComponent(anchorEntity, new LocalTransform
      {
        Position = position,
        Rotation = quaternion.identity,
        Scale = 1f
      });

      ecb.AddComponent(anchorEntity, new PhysicsBodyComponent
      {
        Body = default,
        WorldIndex = 0
      });
      ecb.AddComponent<PhysicsStaticTag>(anchorEntity);

      ecb.AddComponent(anchorEntity, new PhysicsShapeBox
      {
        Center = float2.zero,
        Size = new float2(config.AnchorSize, config.AnchorSize),
        Rotation = 0f
      });

      ecb.AddComponent(anchorEntity, new PhysicsMaterial
      {
        Friction = config.Friction,
        Bounciness = config.Bounciness,
        Density = config.Density
      });

      ecb.AddComponent(anchorEntity, CollisionFilter.Create(1, 0, 1).WithCollisionEvents());

      // ===== Create Dynamic Platform Box =====
      var platformEntity = ecb.CreateEntity();

      // Calculate platform position (to the right of anchor)
      float platformOffset = (config.AnchorSize + config.PlatformWidth) * 0.5f;
      float3 platformPosition;

      if (config.Flip) {
        // Platform to the LEFT of anchor
        platformPosition = new float3(position.x - platformOffset, position.y, 0f);
      } else {
        // Platform to the RIGHT of anchor (default)
        platformPosition = new float3(position.x + platformOffset, position.y, 0f);
      }

      ecb.AddComponent(platformEntity, new LocalTransform
      {
        Position = platformPosition,
        Rotation = quaternion.identity,
        Scale = 1f
      });

      ecb.AddComponent(platformEntity, new PhysicsBodyComponent
      {
        Body = default,
        WorldIndex = 0
      });
      ecb.AddComponent<PhysicsDynamicTag>(platformEntity);

      ecb.AddComponent(platformEntity, new PhysicsShapeBox
      {
        Center = float2.zero,
        Size = new float2(config.PlatformWidth, config.PlatformHeight),
        Rotation = 0f
      });

      ecb.AddComponent(platformEntity, new PhysicsMaterial
      {
        Friction = config.Friction,
        Bounciness = config.Bounciness,
        Density = config.Density
      });

      ecb.AddComponent(platformEntity, CollisionFilter.Create(1, 0, 1).WithCollisionEvents());

      ecb.AddComponent(platformEntity, new PhysicsVelocity
      {
        Linear = float2.zero,
        Angular = 0f
      });

      ecb.AddComponent(platformEntity, new PhysicsDamping
      {
        Linear = 0.1f,
        Angular = 0.1f
      });

      ecb.AddComponent(platformEntity, new PhysicsGravityModifier { Scale = 1f });

      // ===== Create Hinge Joint =====
      var jointEntity = ecb.CreateEntity();

      // Calculate anchor points based on flip
      float2 anchorPoint;
      float2 platformAnchor;

      if (config.Flip) {
        // Anchor on LEFT edge of anchor box, RIGHT edge of platform
        anchorPoint = new float2(-config.AnchorSize * 0.5f, 0f);
        platformAnchor = new float2(config.PlatformWidth * 0.5f, 0f);
      } else {
        // Anchor on RIGHT edge of anchor box, LEFT edge of platform (default)
        anchorPoint = new float2(config.AnchorSize * 0.5f, 0f);
        platformAnchor = new float2(-config.PlatformWidth * 0.5f, 0f);
      }

      ecb.AddComponent(jointEntity, new PhysicsJointComponent
      {
        Joint = default,
        BodyA = anchorEntity,
        BodyB = platformEntity,
        Type = PhysicsJointComponent.JointType.Revolute,
        CollideConnected = false
      });

      ecb.AddComponent(jointEntity, new HingeJoint
      {
        LocalAnchorA = anchorPoint,
        LocalAnchorB = platformAnchor,
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = config.LowerAngleLimit,
        UpperAngle = config.UpperAngleLimit,
        EnableMotor = false,
        EnableSpring = true,
        SpringHertz = config.SpringFrequency,
        SpringDampingRatio = config.SpringDamping
      });

      ecb.AddComponent(jointEntity, JointDamage.Create(config.BreakThreshold));
    }
  }
}
