using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Revolute Joint - Hinge that allows rotation around a point.
  /// Common for doors, wheels, robotic arms.
  /// </summary>
  public struct HingeJoint : IComponentData
  {
    public float2 LocalAnchorA;     // Pivot point on body A
    public float2 LocalAnchorB;     // Pivot point on body B

    // Reference angle: bodyB's rest orientation relative to bodyA (degrees).
    // 0 = bodyB aligned with bodyA at rest. Baked into localAnchorB's rotation
    // at joint creation, so LowerAngleDegrees/UpperAngleDegrees/TargetAngle
    // are measured relative to this offset.
    public float ReferenceAngleDegrees;

    // Spring target angle (degrees). Only active when EnableSpring is true.
    public float TargetAngle;

    // Angle limits (degrees, relative to ReferenceAngleDegrees).
    // Unity's PhysicsHingeJointDefinition uses degrees; Box2D's internal ±0.99π
    // clamp maps to roughly ±178°.
    public bool EnableLimit;
    public float LowerAngleDegrees;
    public float UpperAngleDegrees;

    // Motor properties
    public bool EnableMotor;
    public float MotorSpeed;        // Target angular velocity (deg/s)
    public float MaxMotorTorque;    // Maximum torque

    // Spring properties (for flexible joints)
    public bool EnableSpring;
    public float SpringHertz;       // Spring frequency
    public float SpringDampingRatio;

    // JointEvent - Break thresholds
    public float ForceThreshold;
    public float TorqueThreshold;

    public static HingeJoint CreateHinge(float2 anchor)
    {
      return new HingeJoint
      {
        LocalAnchorA = anchor,
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = false,
        EnableMotor = false,
        EnableSpring = false,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }

    public static HingeJoint CreateMotorizedHinge(float2 anchor, float speed, float maxTorque)
    {
      return new HingeJoint
      {
        LocalAnchorA = anchor,
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = false,
        EnableMotor = true,
        MotorSpeed = speed,
        MaxMotorTorque = maxTorque,
        EnableSpring = false,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }

    public static HingeJoint CreateDoor(float2 anchor, float minAngle, float maxAngle)
    {
      return new HingeJoint
      {
        LocalAnchorA = anchor,
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngleDegrees = minAngle,
        UpperAngleDegrees = maxAngle,
        EnableMotor = false,
        EnableSpring = false,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }
  }
}