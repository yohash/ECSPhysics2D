using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Creates windmill entities from config at runtime.
  /// Runs once per config entity, then removes the config.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(BuildPhysicsWorldSystem))]
  public partial struct WindmillInitializationSystem : ISystem
  {
    public void OnUpdate(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (config, configEntity) in
          SystemAPI.Query<RefRO<WindmillConfig>>()
          .WithEntityAccess()) {
        CreateWindmill(ref state, ecb, config.ValueRO);

        // Remove config after initialization (one-time creation)
        ecb.DestroyEntity(configEntity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void CreateWindmill(
        ref SystemState state,
        EntityCommandBuffer ecb,
        WindmillConfig config)
    {
      var position = config.SpawnPosition;

      // ===== Create Static World Anchor (invisible) =====
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

      // Small circle shape (just for physics existence, won't collide with much)
      ecb.AddComponent(anchorEntity, new PhysicsShapeCircle
      {
        Center = float2.zero,
        Radius = 0.1f
      });

      ecb.AddComponent(anchorEntity, new PhysicsMaterial
      {
        Friction = 0f,
        Bounciness = 0f,
        Density = 1f
      });

      // Don't collide with spokes/circles
      ecb.AddComponent(anchorEntity, CollisionFilter.Create(2, 0).WithCollisionEvents());

      // ===== Create Central Rotating Circle =====
      var centralEntity = ecb.CreateEntity();

      ecb.AddComponent(centralEntity, new LocalTransform
      {
        Position = position,
        Rotation = quaternion.identity,
        Scale = 1f
      });

      ecb.AddComponent(centralEntity, new PhysicsBodyComponent
      {
        Body = default,
        WorldIndex = 0
      });
      ecb.AddComponent<PhysicsDynamicTag>(centralEntity);

      ecb.AddComponent(centralEntity, new PhysicsShapeCircle
      {
        Center = float2.zero,
        Radius = config.CentralRadius
      });

      ecb.AddComponent(centralEntity, new PhysicsMaterial
      {
        Friction = config.Friction,
        Bounciness = config.Bounciness,
        Density = config.Density
      });

      ecb.AddComponent(centralEntity, CollisionFilter.Create(1, 0, 1).WithCollisionEvents());

      ecb.AddComponent(centralEntity, new PhysicsVelocity
      {
        Linear = float2.zero,
        Angular = config.RotationSpeed  // Start with initial spin
      });

      ecb.AddComponent(centralEntity, new PhysicsDamping
      {
        Linear = 0f,
        Angular = 0f  // No damping - motor maintains speed
      });

      ecb.AddComponent(centralEntity, new PhysicsGravityModifier { Scale = 0f });

      // WindmillMotor component for velocity control
      ecb.AddComponent(centralEntity, WindmillMotor.Create(config.RotationSpeed, config.MaxTorque));

      // ===== Create Motor Joint (Hinge with motor) =====
      var motorJointEntity = ecb.CreateEntity();

      ecb.AddComponent(motorJointEntity, new PhysicsJointComponent
      {
        Joint = default,
        BodyA = anchorEntity,
        BodyB = centralEntity,
        Type = PhysicsJointComponent.JointType.Revolute,
        CollideConnected = false
      });

      ecb.AddComponent(motorJointEntity, new HingeJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = false,  // Free rotation
        EnableMotor = true,
        MotorSpeed = config.RotationSpeed,
        MaxMotorTorque = config.MaxTorque,
        EnableSpring = false
      });

      // ===== Create 4 Spokes with Weld Joints =====
      float[] angles = { 0f, 90f, 180f, 270f };

      for (int i = 0; i < 4; i++) {
        float angleRad = math.radians(angles[i]);
        float2 direction = new float2(math.cos(angleRad), math.sin(angleRad));

        // Spoke position: starts at edge of central circle
        float spokeOffset = config.CentralRadius + config.SpokeLength * 0.5f;
        float2 spokeLocalPos = direction * spokeOffset;

        // Create spoke entity
        var spokeEntity = ecb.CreateEntity();

        ecb.AddComponent(spokeEntity, new LocalTransform
        {
          Position = new float3(position.x + spokeLocalPos.x, position.y + spokeLocalPos.y, 0f),
          Rotation = quaternion.RotateZ(angleRad),
          Scale = 1f
        });

        ecb.AddComponent(spokeEntity, new PhysicsBodyComponent
        {
          Body = default,
          WorldIndex = 0
        });
        ecb.AddComponent<PhysicsDynamicTag>(spokeEntity);

        ecb.AddComponent(spokeEntity, new PhysicsShapeBox
        {
          Center = float2.zero,
          Size = new float2(config.SpokeLength, config.SpokeWidth),
          Rotation = 0f  // Rotation handled by LocalTransform
        });

        ecb.AddComponent(spokeEntity, new PhysicsMaterial
        {
          Friction = config.Friction,
          Bounciness = config.Bounciness,
          Density = config.Density
        });

        ecb.AddComponent(spokeEntity, CollisionFilter.Create(1, 0, 1).WithCollisionEvents());

        ecb.AddComponent(spokeEntity, new PhysicsVelocity
        {
          Linear = float2.zero,
          Angular = 0f
        });

        ecb.AddComponent(spokeEntity, new PhysicsDamping
        {
          Linear = 0f,
          Angular = 0f
        });

        ecb.AddComponent(spokeEntity, new PhysicsGravityModifier { Scale = 1f });

        // ===== Create Weld Joint connecting spoke to central circle =====
        var weldJointEntity = ecb.CreateEntity();

        // Anchor at base of spoke (inside central circle)
        float2 spokeAnchorOnCircle = direction * config.CentralRadius;
        float2 spokeAnchorLocal = new float2(-config.SpokeLength * 0.5f, 0f);

        ecb.AddComponent(weldJointEntity, new PhysicsJointComponent
        {
          Joint = default,
          BodyA = centralEntity,
          BodyB = spokeEntity,
          Type = PhysicsJointComponent.JointType.Weld,
          CollideConnected = false
        });

        ecb.AddComponent(weldJointEntity, new WeldJoint
        {
          LocalAnchorA = spokeAnchorOnCircle,
          LocalAnchorB = spokeAnchorLocal,
          ReferenceAngle = 0f,  // Maintain rigid angle
          LinearHertz = 0f, // Rigid (no spring)
          LinearDampingRatio = 1f,
          AngularHertz = 0,
          AngularDampingRatio = 1f
        });

        ecb.AddComponent(weldJointEntity, JointDamage.Create(config.SpokeBreakThreshold));
      }
    }
  }
}
