using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Event generated when a joint exceeds its configured force or torque threshold.
  /// 
  /// IMPORTANT: Box2D threshold events are binary notifications only.
  /// They signal "threshold exceeded" but do NOT include actual force/torque values.
  /// Users configure thresholds during joint creation via ForceThreshold/TorqueThreshold
  /// properties on joint components.
  /// </summary>
  public struct JointThresholdEvent
  {
    /// <summary>
    /// Entity containing the PhysicsJointComponent that exceeded threshold.
    /// </summary>
    public Entity JointEntity;

    /// <summary>
    /// Raw Box2D joint handle for advanced queries.
    /// Check isValid before use - joint may have been destroyed.
    /// </summary>
    public PhysicsJoint Joint;

    /// <summary>
    /// Simulation time when threshold was exceeded.
    /// Useful for debouncing or time-based damage accumulation.
    /// </summary>
    public double TimeStamp;

    /// <summary>
    /// Force (Newtons) the joint applies to bodyB at anchor point 
    /// to maintain constraint
    /// </summary>
    public float2 ConstraintForce;

    /// <summary>
    /// Torque (Newton-meters) the joint applies to bodyB to 
    /// maintain rotational constraint
    /// </summary>
    public float ConstraintTorque;

    /// <summary>
    /// Check if this event's joint is still valid.
    /// Joint may have been destroyed between event generation and processing.
    /// </summary>
    public bool IsValid => Joint.isValid;
  }
}