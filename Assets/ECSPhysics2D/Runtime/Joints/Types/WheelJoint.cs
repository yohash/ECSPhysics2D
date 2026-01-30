using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Wheel Joint - Specialized for vehicle suspension.
  /// Combines spring suspension with wheel rotation.
  /// </summary>
  public struct WheelJoint : IComponentData
  {
    public float2 LocalAnchorA;     // Suspension mount on chassis
    public float2 LocalAnchorB;     // Wheel center
    public float2 LocalAxisA;       // Suspension axis (usually vertical)

    // Suspension spring
    public bool EnableSpring;
    public float SpringHertz;       // Suspension frequency
    public float SpringDampingRatio; // Suspension damping

    // Translation limits (suspension travel)
    public bool EnableLimit;
    public float LowerTranslation;
    public float UpperTranslation;

    // Wheel motor
    public bool EnableMotor;
    public float MotorSpeed;        // Wheel angular velocity
    public float MaxMotorTorque;    // Drive torque

    // Event thresholds
    public float ForceThreshold;
    public float TorqueThreshold;

    public static WheelJoint CreateSuspension(float2 mount, float travel, float frequency)
    {
      return new WheelJoint
      {
        LocalAnchorA = mount,
        LocalAnchorB = float2.zero,
        LocalAxisA = new float2(0, -1), // Vertical suspension
        EnableSpring = true,
        SpringHertz = frequency,
        SpringDampingRatio = 0.7f,
        EnableLimit = true,
        LowerTranslation = -travel * 0.5f,
        UpperTranslation = travel * 0.5f,
        EnableMotor = false,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }

    public static WheelJoint CreateDriveWheel(float2 mount, float travel, float frequency, float maxTorque)
    {
      var wheel = CreateSuspension(mount, travel, frequency);
      wheel.EnableMotor = true;
      wheel.MaxMotorTorque = maxTorque;
      return wheel;
    }
  }
}