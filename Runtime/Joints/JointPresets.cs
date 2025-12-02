using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Joint configuration presets for common scenarios.
  /// </summary>
  public static class JointPresets
  {
    /// <summary>
    /// Vehicle suspension configurations.
    /// </summary>
    public static class Vehicle
    {
      public static WheelJoint SportsCar => new WheelJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        LocalAxisA = new float2(0, -1),
        EnableSpring = true,
        SpringHertz = 8f,      // Stiff suspension
        SpringDampingRatio = 0.8f,
        EnableLimit = true,
        LowerTranslation = -0.1f,
        UpperTranslation = 0.1f,
        EnableMotor = false
      };

      public static WheelJoint OffRoad => new WheelJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        LocalAxisA = new float2(0, -1),
        EnableSpring = true,
        SpringHertz = 3f,      // Soft suspension
        SpringDampingRatio = 0.5f,
        EnableLimit = true,
        LowerTranslation = -0.3f,
        UpperTranslation = 0.3f,
        EnableMotor = false
      };

      public static WheelJoint Monster => new WheelJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        LocalAxisA = new float2(0, -1),
        EnableSpring = true,
        SpringHertz = 2f,      // Very soft
        SpringDampingRatio = 0.3f,
        EnableLimit = true,
        LowerTranslation = -0.5f,
        UpperTranslation = 0.5f,
        EnableMotor = false
      };
    }

    /// <summary>
    /// Door hinge configurations.
    /// </summary>
    public static class Doors
    {
      public static HingeJoint StandardDoor => new HingeJoint
      {
        LocalAnchorA = new float2(-0.5f, 0),  // Hinge at edge
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = 0f,
        UpperAngle = 90f * math.PI / 180f,
        EnableMotor = false,
        EnableSpring = false
      };

      public static HingeJoint DoubleDoor => new HingeJoint
      {
        LocalAnchorA = new float2(-0.5f, 0),
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = -90f * math.PI / 180f,
        UpperAngle = 90f * math.PI / 180f,
        EnableMotor = false,
        EnableSpring = false
      };

      public static HingeJoint AutomaticDoor => new HingeJoint
      {
        LocalAnchorA = new float2(-0.5f, 0),
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = 0f,
        UpperAngle = 90f * math.PI / 180f,
        EnableMotor = true,
        MotorSpeed = 60f * math.PI / 180f,  // 60 degrees/sec
        MaxMotorTorque = 100f,
        EnableSpring = false
      };
    }

    /// <summary>
    /// Mechanical linkage configurations.
    /// </summary>
    public static class Mechanical
    {
      public static SliderJoint Piston => new SliderJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        LocalAxisA = new float2(1, 0),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerTranslation = 0f,
        UpperTranslation = 1f,
        EnableMotor = true,
        MotorSpeed = 0.5f,
        MaxMotorForce = 500f,
        EnableSpring = false
      };

      public static SliderJoint Elevator => new SliderJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        LocalAxisA = new float2(0, 1),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerTranslation = 0f,
        UpperTranslation = 10f,
        EnableMotor = true,
        MotorSpeed = 2f,
        MaxMotorForce = 5000f,
        EnableSpring = false
      };

      public static HingeJoint Gear => new HingeJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        TargetAngle = 0f,
        EnableLimit = false,
        EnableMotor = true,
        MotorSpeed = 180f * math.PI / 180f,
        MaxMotorTorque = 50f,
        EnableSpring = false
      };
    }

    /// <summary>
    /// Ragdoll joint configurations.
    /// </summary>
    public static class Ragdoll
    {
      public static HingeJoint Shoulder => new HingeJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = new float2(0, 0.15f),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = -90f * math.PI / 180f,
        UpperAngle = 90f * math.PI / 180f,
        EnableMotor = false,
        EnableSpring = true,
        SpringHertz = 5f,
        SpringDampingRatio = 0.5f
      };

      public static HingeJoint Elbow => new HingeJoint
      {
        LocalAnchorA = new float2(0, -0.15f),
        LocalAnchorB = new float2(0, 0.15f),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = -135f * math.PI / 180f,
        UpperAngle = 0f,
        EnableMotor = false,
        EnableSpring = true,
        SpringHertz = 5f,
        SpringDampingRatio = 0.5f
      };

      public static HingeJoint Hip => new HingeJoint
      {
        LocalAnchorA = new float2(0, -0.2f),
        LocalAnchorB = new float2(0, 0.2f),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = -45f * math.PI / 180f,
        UpperAngle = 45f * math.PI / 180f,
        EnableMotor = false,
        EnableSpring = true,
        SpringHertz = 8f,
        SpringDampingRatio = 0.7f
      };

      public static HingeJoint Knee => new HingeJoint
      {
        LocalAnchorA = new float2(0, -0.2f),
        LocalAnchorB = new float2(0, 0.2f),
        TargetAngle = 0f,
        EnableLimit = true,
        LowerAngle = 0f,
        UpperAngle = 135f * math.PI / 180f,
        EnableMotor = false,
        EnableSpring = true,
        SpringHertz = 8f,
        SpringDampingRatio = 0.7f
      };
    }
  }
}