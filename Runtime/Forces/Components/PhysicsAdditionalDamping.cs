using Unity.Entities;

namespace ECSPhysics2D
{

  /// <summary>
  /// Additional damping beyond the body's inherent damping.
  /// Good for: water zones, air resistance, slow-motion effects.
  /// </summary>
  public struct PhysicsAdditionalDamping : IComponentData
  {
    public float LinearDamping;    // Reduces linear velocity
    public float AngularDamping;   // Reduces angular velocity
    public float Duration;         // How long to apply (0 = infinite)
    public float TimeRemaining;    // Countdown timer

    public static PhysicsAdditionalDamping CreateTemporary(float linear, float angular, float duration)
    {
      return new PhysicsAdditionalDamping
      {
        LinearDamping = linear,
        AngularDamping = angular,
        Duration = duration,
        TimeRemaining = duration
      };
    }
  }
}