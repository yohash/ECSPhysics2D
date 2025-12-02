using ECSPhysics2D;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that creates joints between physics bodies.
  /// Joints are separate entities that reference two body entities.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(BuildPhysicsWorldSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  //[BurstCompile]
  public partial struct JointCreationSystem : ISystem
  {
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var physicsWorld = physicsWorldSingleton.World;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Create Distance Joints
      CreateDistanceJoints(ref state, physicsWorld, ecb);

      // Create Revolute/Hinge Joints
      CreateHingeJoints(ref state, physicsWorld, ecb);

      // Create Prismatic/Slider Joints
      CreateSliderJoints(ref state, physicsWorld, ecb);

      // Create Wheel Joints
      CreateWheelJoints(ref state, physicsWorld, ecb);

      // Create Weld/Fixed Joints
      CreateFixedJoints(ref state, physicsWorld, ecb);

      // Create Motor/Relative Joints
      CreateRelativeJoints(ref state, physicsWorld, ecb);

      // Create Mouse Joints
      CreateMouseJoints(ref state, physicsWorld, ecb);

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void CreateDistanceJoints(ref SystemState state, PhysicsWorld physicsWorld, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, distanceJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<DistanceJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        // Get physics bodies from entities
        if (!SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA) ||
            !SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB))
          continue;

        var bodyA = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA).Body;
        var bodyB = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB).Body;

        if (!bodyA.isValid || !bodyB.isValid)
          continue;

        // Create distance joint definition
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
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyA, entity, true);
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyB, entity, false);
      }
    }

    private void CreateHingeJoints(ref SystemState state, PhysicsWorld physicsWorld, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, hingeJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<HingeJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA) ||
            !SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB))
          continue;

        var bodyA = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA).Body;
        var bodyB = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB).Body;

        if (!bodyA.isValid || !bodyB.isValid)
          continue;

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
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyA, entity, true);
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyB, entity, false);
      }
    }

    private void CreateSliderJoints(ref SystemState state, PhysicsWorld physicsWorld, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, sliderJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<SliderJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA) ||
            !SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB))
          continue;

        var bodyA = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA).Body;
        var bodyB = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB).Body;

        if (!bodyA.isValid || !bodyB.isValid)
          continue;

        var jointDef = new PhysicsSliderJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(sliderJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(sliderJoint.ValueRO.LocalAnchorB),
          // No axis parameter available in Unity's API
          //localAxisA = sliderJoint.ValueRO.LocalAxisA,
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
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyA, entity, true);
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyB, entity, false);
      }
    }

    private void CreateWheelJoints(ref SystemState state, PhysicsWorld physicsWorld, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, wheelJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<WheelJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA) ||
            !SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB))
          continue;

        var bodyA = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA).Body;
        var bodyB = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB).Body;

        if (!bodyA.isValid || !bodyB.isValid)
          continue;

        var jointDef = new PhysicsWheelJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(wheelJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(wheelJoint.ValueRO.LocalAnchorB),
          // No axis parameter available in Unity's API
          //localAxisA = wheelJoint.ValueRO.LocalAxisA,
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
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyA, entity, true);
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyB, entity, false);
      }
    }

    private void CreateFixedJoints(ref SystemState state, PhysicsWorld physicsWorld, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, fixedJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<WeldJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA) ||
            !SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB))
          continue;

        var bodyA = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA).Body;
        var bodyB = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB).Body;

        if (!bodyA.isValid || !bodyB.isValid)
          continue;

        var jointDef = new PhysicsFixedJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(fixedJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(fixedJoint.ValueRO.LocalAnchorB),
          // referenceAngle not exposed in Unity API
          //referenceAngle = fixedJoint.ValueRO.ReferenceAngle,
          tuningFrequency = fixedJoint.ValueRO.LinearHertz,
          tuningDamping = fixedJoint.ValueRO.LinearDampingRatio,
          angularFrequency = fixedJoint.ValueRO.AngularHertz,
          angularDamping = fixedJoint.ValueRO.AngularDampingRatio,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };

        jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
        jointComponent.ValueRW.Joint.SetEntityUserData(entity);

        ecb.AddComponent<JointCreatedTag>(entity);
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyA, entity, true);
        AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyB, entity, false);
      }
    }

    private void CreateRelativeJoints(ref SystemState state, PhysicsWorld physicsWorld, EntityCommandBuffer ecb)
    {
      foreach (var (jointComponent, relativeJoint, entity) in
          SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<RelativeJoint>>()
          .WithNone<JointCreatedTag>()
          .WithEntityAccess()) {
        if (!SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA) ||
            !SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB))
          continue;

        var bodyA = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyA).Body;
        var bodyB = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB).Body;

        if (!bodyA.isValid || !bodyB.isValid)
          continue;

        var jointDef = new PhysicsRelativeJointDefinition
        {
          bodyA = bodyA,
          bodyB = bodyB,
          localAnchorA = PhysicsUtility.PhysicsTransform(relativeJoint.ValueRO.LocalAnchorA),
          localAnchorB = PhysicsUtility.PhysicsTransform(relativeJoint.ValueRO.LocalAnchorB),
          // Unity uses velocities instead of offsets
          // compare with (https://box2d.org/documentation/group__motor__joint.html)
          linearVelocity = relativeJoint.ValueRO.LinearVelocity,
          angularVelocity = relativeJoint.ValueRO.AngularVelocity,
          maxForce = relativeJoint.ValueRO.MaxForce,
          maxTorque = relativeJoint.ValueRO.MaxTorque,
          // Spring system (Unity addition)
          springLinearFrequency = relativeJoint.ValueRO.SpringLinearFrequency,
          springLinearDamping = relativeJoint.ValueRO.SpringLinearDamping,
          springAngularFrequency = relativeJoint.ValueRO.SpringAngularFrequency,
          springAngularDamping = relativeJoint.ValueRO.SpringAngularDamping,
          collideConnected = jointComponent.ValueRO.CollideConnected
        };
      }
    }

    private void CreateMouseJoints(ref SystemState state, PhysicsWorld physicsWorld, EntityCommandBuffer ecb)
    {
      // TODO - fully implement mouse joint creation when Unity exposes MouseJointDefinition in Physics2D API
      // OR create an equivalent joint using distance joint that follows a new target component, which is updated
      // in a new system

      //foreach (var (jointComponent, mouseJoint, entity) in
      //    SystemAPI.Query<RefRW<PhysicsJointComponent>, RefRO<MouseJoint>>()
      //    .WithNone<JointCreatedTag>()
      //    .WithEntityAccess()) {
      //  // Mouse joint only connects to one body (BodyB)
      //  if (!SystemAPI.HasComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB))
      //    continue;

      //  var bodyB = SystemAPI.GetComponent<PhysicsBodyComponent>(jointComponent.ValueRO.BodyB).Body;
      //  if (!bodyB.isValid)
      //    continue;

      //  // Unity has not exposed MouseJointDefinition in Physics2D API as of now
      //  // compare with Box2D documentation: https://box2d.org/documentation/group__mouse__joint.html
      //  //var jointDef = new PhysicsMouseJointDefinition
      //  //{
      //  //  bodyA = default, // Mouse joint doesn't use bodyA
      //  //  bodyB = bodyB,
      //  //  target = mouseJoint.ValueRO.Target,
      //  //  localAnchor = mouseJoint.ValueRO.LocalAnchor,
      //  //  hertz = mouseJoint.ValueRO.Hertz,
      //  //  dampingRatio = mouseJoint.ValueRO.DampingRatio,
      //  //  maxForce = mouseJoint.ValueRO.MaxForce
      //  //};

      //  jointComponent.ValueRW.Joint = physicsWorld.CreateJoint(jointDef);
      //  jointComponent.ValueRW.Joint.SetEntityUserData(entity);

      //  ecb.AddComponent<JointCreatedTag>(entity);
      //  AddJointReferenceToBody(ref state, ecb, jointComponent.ValueRO.BodyB, entity, false);
      //}
    }

    private void AddJointReferenceToBody(ref SystemState state, EntityCommandBuffer ecb, Entity bodyEntity, Entity jointEntity, bool isBodyA)
    {
      if (!SystemAPI.HasBuffer<JointReference>(bodyEntity)) {
        ecb.AddBuffer<JointReference>(bodyEntity);
      }
      // Note: Can't append to buffer in ECB directly, need to do this in a follow-up system
      // or use a different approach for immediate buffer updates
    }
  }
}