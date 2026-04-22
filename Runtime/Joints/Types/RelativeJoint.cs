using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Motor Joint - Controls relative position and rotation.
  /// Moves bodies to target configurations.
  /// </summary>
  public struct RelativeJoint : IComponentData
  {
    public float2 LocalAnchorA;
    public float2 LocalAnchorB;

    // Velocity targets (NOT position offsets)
    public float2 LinearVelocity;
    public float AngularVelocity;

    public float MaxForce;
    public float MaxTorque;

    // Spring parameters
    public float SpringLinearFrequency;
    public float SpringLinearDamping;
    public float SpringAngularFrequency;
    public float SpringAngularDamping;

    // Event thresholds
    public float ForceThreshold;
    public float TorqueThreshold;

    public static RelativeJoint Create(float maxForce, float maxTorque)
    {
      return new RelativeJoint
      {
        LinearVelocity = float2.zero,
        AngularVelocity = 0f,
        MaxForce = maxForce,
        MaxTorque = maxTorque,
        SpringAngularDamping = 0f,
        SpringLinearDamping = 0f,
        SpringAngularFrequency = 0f,
        SpringLinearFrequency = 0f,
        ForceThreshold = float.MaxValue,
        TorqueThreshold = float.MaxValue
      };
    }
  }
}