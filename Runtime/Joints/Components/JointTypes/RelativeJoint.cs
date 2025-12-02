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

    // Joint constraint tuning
    public float TuningFrequency;
    public float TuningDamping;

    public static RelativeJoint Create(float maxForce, float maxTorque, float frequency, float damping)
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
        TuningFrequency = frequency,
        TuningDamping = damping,
      };
    }
  }
}