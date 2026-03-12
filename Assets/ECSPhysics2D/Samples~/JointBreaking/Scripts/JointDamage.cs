using Unity.Entities;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Tracks cumulative damage on a joint from collisions.
  /// When damage exceeds threshold, joint breaks.
  /// </summary>
  public struct JointDamage : IComponentData
  {
    public float Accumulated;
    public float BreakThreshold;
    public bool Broken;

    public static JointDamage Create(float breakThreshold)
    {
      return new JointDamage
      {
        Accumulated = 0f,
        BreakThreshold = breakThreshold,
        Broken = false
      };
    }

    public float DamagePercent => Accumulated / BreakThreshold;
    public bool ShouldBreak => Accumulated >= BreakThreshold;
  }
}
