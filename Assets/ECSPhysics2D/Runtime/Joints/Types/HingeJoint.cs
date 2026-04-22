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
    // at joint creation, so LowerAngleRadians/UpperAngleRadians/TargetAngle
    // are measured relative to this offset.
    public float ReferenceAngleDegrees;

    // Spring target angle (radians). Only active when EnableSpring is true.
    public float TargetAngle;

    // Angle limits (radians, relative to ReferenceAngleDegrees).
    // Box2D v3 clamps to roughly ±0.99π.
    public bool EnableLimit;
    public float LowerAngleRadians;
    public float UpperAngleRadians;

    // Motor properties
    public bool EnableMotor;
    public float MotorSpeed;        // Target angular velocity (rad/s)
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
        LowerAngleRadians = minAngle,
        UpperAngleRadians = maxAngle,
        EnableMotor = false,
        EnableSpring = false,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }
  }
}