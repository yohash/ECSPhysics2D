using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Distance Joint - Maintains distance between two anchor points.
  /// NEW in Box2D v3: Now supports motors!
  /// </summary>
  public struct DistanceJoint : IComponentData
  {
    public float2 LocalAnchorA;     // Anchor point on body A (local space)
    public float2 LocalAnchorB;     // Anchor point on body B (local space)
    public float Length;            // Target distance to maintain
    public float MinLength;         // Minimum allowed distance
    public float MaxLength;         // Maximum allowed distance
    public float SpringHertz;             // Spring frequency (0 = rigid)
    public float SpringDamping;      // Spring damping (0-1)

    // Motor properties (NEW in v3!)
    public bool EnableMotor;
    public float MotorSpeed;        // Target speed (m/s)
    public float MaxMotorForce;     // Maximum motor force

    public static DistanceJoint CreateRope(float length, float stretch = 0.1f)
    {
      return new DistanceJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        Length = length,
        MinLength = length * (1f - stretch),
        MaxLength = length * (1f + stretch),
        SpringHertz = 0f,  // Rigid rope
        SpringDamping = 0f,
        EnableMotor = false
      };
    }

    public static DistanceJoint CreateSpring(float length, float frequency, float damping)
    {
      return new DistanceJoint
      {
        LocalAnchorA = float2.zero,
        LocalAnchorB = float2.zero,
        Length = length,
        MinLength = 0f,
        MaxLength = float.MaxValue,
        SpringHertz = frequency,
        SpringDamping = damping,
        EnableMotor = false
      };
    }
  }
}