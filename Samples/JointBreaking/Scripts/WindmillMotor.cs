using Unity.Entities;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Applies constant angular velocity to simulate a powered motor.
  /// Used for the windmill's continuous rotation.
  /// </summary>
  public struct WindmillMotor : IComponentData
  {
    public float TargetAngularVelocity;  // radians per second
    public float MaxTorque;              // maximum torque to apply

    public static WindmillMotor Create(float angularVelocity, float maxTorque = 200f)
    {
      return new WindmillMotor
      {
        TargetAngularVelocity = angularVelocity,
        MaxTorque = maxTorque
      };
    }
  }
}
