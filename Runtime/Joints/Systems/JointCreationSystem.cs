using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that creates joints between physics bodies.
  /// Joints are separate entities that reference two body entities.
  /// 
  /// Important: Both bodies connected by a joint must be in the same physics world.
  /// The system validates WorldIndex match before creating joints.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ShapeCreationSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  public partial struct JointCreationSystem : ISystem
  {
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      CreateDistanceJoints(ref state, singleton, ecb);
      CreateHingeJoints(ref state, singleton, ecb);
      CreateSliderJoints(ref state, singleton, ecb);
      CreateWheelJoints(ref state, singleton, ecb);
      CreateFixedJoints(ref state, singleton, ecb);
      CreateRelativeJoints(ref state, singleton, ecb);
      CreateMouseJoints(ref state, singleton, ecb);

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    /// <summary>
    /// Validates that both bodies are in the same physics world and returns the world.
    /// Returns false if validation fails.
    /// </summary>
    private bool ValidateJointBodies(
        ref SystemState state,
        Entity bodyEntityA,
        Entity bodyEntityB,
        out PhysicsBody bodyA,
        out PhysicsBody bodyB,
        out PhysicsWorld world)
    {
      bodyA = default;
      bodyB = default;
      world = default;

      if (!SystemAPI.HasComponent<PhysicsBodyComponent>(bodyEntityA) ||
          !SystemAPI.HasComponent<PhysicsBodyComponent>(bodyEntityB))
        return false;

      var bodyCompA = SystemAPI.GetComponent<PhysicsBodyComponent>(bodyEntityA);
      var bodyCompB = SystemAPI.GetComponent<PhysicsBodyComponent>(bodyEntityB);

      if (!bodyCompA.IsValid || !bodyCompB.IsValid)
        return false;

      // Validate both bodies are in the same world
      if (bodyCompA.WorldIndex != bodyCompB.WorldIndex) {
        Debug.LogError($"Joint creation failed: Bodies are in different physics worlds " +
                       $"(BodyA: World {bodyCompA.WorldIndex}, BodyB: World {bodyCompB.WorldIndex}). " +
                       $"Joints cannot span multiple worlds.");
        return false;
      }

      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return false;

      bodyA = bodyCompA.Body;
      bodyB = bodyCompB.Body;
      world = singleton.GetWorld(bodyCompA.WorldIndex);

      return true;
    }

    private void CreateDistanceJoints(ref SystemState state, PhysicsWorldSingleton singleton, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, distanceJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<DistanceJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!ValidateJointBodies(
          ref state,
          jointComponent.ValueRO.BodyA,
          jointComponent.ValueRO.BodyB,
          out var bodyA,
          out var bodyB,
          out var physicsWorld)
        ) {
          continue;
        }

        var jointDef = new PhysicsDistanceJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(distanceJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(distanceJoint.ValueRO.LocalAnchorB),
          distance = distanceJoint.ValueRO.Length,
          minDistanceLimit = distanceJoint.ValueRO.MinLength,
          maxDistanceLimit = distanceJoint.ValueRO.MaxLength,
          springFrequency = distanceJoint.ValueRO.SpringHertz,
          springDamping = distanceJoint.ValueRO.SpringDamping,
          enableMotor = distanceJoint.ValueRO.EnableMotor,
          motorSpeed = distanceJoint.ValueRO.MotorSpeed,
          maxMotorForce = distanceJoint.ValueRO.MaxMotorForce,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };

        jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
        jointComponent.ValueRW.Joint.SetEntityUserData(entity);

        ecb.AddComponent<JointCreatedTag>(entity);
      }
    }

    private void CreateHingeJoints(ref SystemState state, PhysicsWorldSingleton singleton, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, hingeJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<HingeJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!ValidateJointBodies(
          ref state,
          jointComponent.ValueRO.BodyA,
          jointComponent.ValueRO.BodyB,
          out var bodyA,
          out var bodyB,
          out var physicsWorld)
        ) {
          continue;
        }

        var jointDef = new PhysicsHingeJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(hingeJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(hingeJoint.ValueRO.LocalAnchorB),
          springTargetAngle = hingeJoint.ValueRO.TargetAngle,
          enableLimit = hingeJoint.ValueRO.EnableLimit,
          lowerAngleLimit = hingeJoint.ValueRO.LowerAngle,
          upperAngleLimit = hingeJoint.ValueRO.UpperAngle,
          enableMotor = hingeJoint.ValueRO.EnableMotor,
          motorSpeed = hingeJoint.ValueRO.MotorSpeed,
          maxMotorTorque = hingeJoint.ValueRO.MaxMotorTorque,
          enableSpring = hingeJoint.ValueRO.EnableSpring,
          springFrequency = hingeJoint.ValueRO.SpringHertz,
          springDamping = hingeJoint.ValueRO.SpringDampingRatio,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };

        jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
        jointComponent.ValueRW.Joint.SetEntityUserData(entity);

        ecb.AddComponent<JointCreatedTag>(entity);
      }
    }

    private void CreateSliderJoints(ref SystemState state, PhysicsWorldSingleton singleton, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, sliderJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<SliderJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!ValidateJointBodies(
          ref state,
          jointComponent.ValueRO.BodyA,
          jointComponent.ValueRO.BodyB,
          out var bodyA,
          out var bodyB,
          out var physicsWorld)
        ) {
          continue;
        }

        var jointDef = new PhysicsSliderJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(sliderJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(sliderJoint.ValueRO.LocalAnchorB),
          springTargetTranslation = sliderJoint.ValueRO.TargetAngle,
          enableLimit = sliderJoint.ValueRO.EnableLimit,
          lowerTranslationLimit = sliderJoint.ValueRO.LowerTranslation,
          upperTranslationLimit = sliderJoint.ValueRO.UpperTranslation,
          enableMotor = sliderJoint.ValueRO.EnableMotor,
          motorSpeed = sliderJoint.ValueRO.MotorSpeed,
          maxMotorForce = sliderJoint.ValueRO.MaxMotorForce,
          enableSpring = sliderJoint.ValueRO.EnableSpring,
          springFrequency = sliderJoint.ValueRO.SpringHertz,
          springDamping = sliderJoint.ValueRO.SpringDampingRatio,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };

        jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
        jointComponent.ValueRW.Joint.SetEntityUserData(entity);

        ecb.AddComponent<JointCreatedTag>(entity);
      }
    }

    private void CreateWheelJoints(ref SystemState state, PhysicsWorldSingleton singleton, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, wheelJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<WheelJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!ValidateJointBodies(
          ref state,
          jointComponent.ValueRO.BodyA,
          jointComponent.ValueRO.BodyB,
          out var bodyA,
          out var bodyB,
          out var physicsWorld)
        ) {
          continue;
        }

        var jointDef = new PhysicsWheelJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(wheelJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(wheelJoint.ValueRO.LocalAnchorB),
          enableSpring = wheelJoint.ValueRO.EnableSpring,
          springFrequency = wheelJoint.ValueRO.SpringHertz,
          springDamping = wheelJoint.ValueRO.SpringDampingRatio,
          enableLimit = wheelJoint.ValueRO.EnableLimit,
          lowerTranslationLimit = wheelJoint.ValueRO.LowerTranslation,
          upperTranslationLimit = wheelJoint.ValueRO.UpperTranslation,
          enableMotor = wheelJoint.ValueRO.EnableMotor,
          motorSpeed = wheelJoint.ValueRO.MotorSpeed,
          maxMotorTorque = wheelJoint.ValueRO.MaxMotorTorque,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };

        jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
        jointComponent.ValueRW.Joint.SetEntityUserData(entity);

        ecb.AddComponent<JointCreatedTag>(entity);
      }
    }

    private void CreateFixedJoints(ref SystemState state, PhysicsWorldSingleton singleton, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, fixedJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<WeldJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!ValidateJointBodies(
          ref state,
          jointComponent.ValueRO.BodyA,
          jointComponent.ValueRO.BodyB,
          out var bodyA,
          out var bodyB,
          out var physicsWorld)
        ) {
          continue;
        }

        var jointDef = new PhysicsFixedJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(fixedJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(fixedJoint.ValueRO.LocalAnchorB),
          tuningFrequency = fixedJoint.ValueRO.LinearHertz,
          tuningDamping = fixedJoint.ValueRO.LinearDampingRatio,
          angularFrequency = fixedJoint.ValueRO.AngularHertz,
          angularDamping = fixedJoint.ValueRO.AngularDampingRatio,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };

        jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
        jointComponent.ValueRW.Joint.SetEntityUserData(entity);

        ecb.AddComponent<JointCreatedTag>(entity);
      }
    }

    private void CreateRelativeJoints(ref SystemState state, PhysicsWorldSingleton singleton, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, relativeJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<RelativeJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!ValidateJointBodies(
          ref state,
          jointComponent.ValueRO.BodyA,
          jointComponent.ValueRO.BodyB,
          out var bodyA,
          out var bodyB,
          out var physicsWorld)
        ) {
          continue;
        }

        var jointDef = new PhysicsRelativeJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(relativeJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(relativeJoint.ValueRO.LocalAnchorB),
          linearVelocity = relativeJoint.ValueRO.LinearVelocity,
          angularVelocity = relativeJoint.ValueRO.AngularVelocity,
          maxForce = relativeJoint.ValueRO.MaxForce,
          maxTorque = relativeJoint.ValueRO.MaxTorque,
          springLinearFrequency = relativeJoint.ValueRO.SpringLinearFrequency,
          springLinearDamping = relativeJoint.ValueRO.SpringLinearDamping,
          springAngularFrequency = relativeJoint.ValueRO.SpringAngularFrequency,
          springAngularDamping = relativeJoint.ValueRO.SpringAngularDamping,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };

        jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
        jointComponent.ValueRW.Joint.SetEntityUserData(entity);

        ecb.AddComponent<JointCreatedTag>(entity);
      }
    }

    private void CreateMouseJoints(ref SystemState state, PhysicsWorldSingleton singleton, EntityCommandBuffer ecb)
    {
      // TODO - fully implement mouse joint creation when Unity exposes MouseJointDefinition in Physics2D API
      // OR create an equivalent joint using distance joint that follows a new target component
    }
  }
}
