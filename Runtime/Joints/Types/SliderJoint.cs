using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Prismatic Joint - Slider that allows translation along an axis.
  /// Used for pistons, elevators, sliding doors.
  /// </summary>
  public struct SliderJoint : IComponentData
  {
    public float2 LocalAnchorA;     // Anchor point on body A
    public float2 LocalAnchorB;     // Anchor point on body B
    public float2 LocalAxisA;       // Sliding axis in body A space
    public float TargetAngle;    // Maintains relative rotation

    // Translation limits
    public bool EnableLimit;
    public float LowerTranslation;  // Minimum position
    public float UpperTranslation;  // Maximum position

    // Motor properties
    public bool EnableMotor;
    public float MotorSpeed;        // Target linear velocity (m/s)
    public float MaxMotorForce;     // Maximum force

    // Spring properties
    public bool EnableSpring;
    public float SpringHertz;
    public float SpringDampingRatio;

    // Event thresholds
    public float ForceThreshold;
    public float TorqueThreshold;

    public static SliderJoint CreateSlider(float2 axis, float minDist, float maxDist)
    {
      return new SliderJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        LocalAxisA = math.normalize(axis),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerTranslation = minDist,
        UpperTranslation = maxDist,
        EnableMotor = false,
        EnableSpring = false,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }

    public static SliderJoint CreatePiston(float2 axis, float stroke, float speed)
    {
      return new SliderJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        LocalAxisA = math.normalize(axis),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerTranslation = 0f,
        UpperTranslation = stroke,
        EnableMotor = true,
        MotorSpeed = speed,
        MaxMotorForce = 1000f,
        EnableSpring = false,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }
  }
}