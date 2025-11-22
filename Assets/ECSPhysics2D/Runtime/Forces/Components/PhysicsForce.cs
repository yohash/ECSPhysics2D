using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Continuous force applied every physics frame.
  /// Good for: thrust, wind, magnetic fields, gravity wells.
  /// </summary>
  public struct PhysicsForce : IComponentData
  {
    public float2 Force;           // Force vector in Newtons
    public float2 Point;           // World point where force is applied (generates torque)
    public bool UseWorldPoint;     // If false, force applied at center of mass
    public float Duration;         // How long to apply (0 = infinite)
    public float TimeRemaining;    // Countdown timer

    public static PhysicsForce CreateCentralForce(float2 force, float duration = 0f)
    {
      return new PhysicsForce
      {
        Force = force,
        Point = float2.zero,
        UseWorldPoint = false,
        Duration = duration,
        TimeRemaining = duration
      };
    }

    public static PhysicsForce CreatePointForce(float2 force, float2 worldPoint, float duration = 0f)
    {
      return new PhysicsForce
      {
        Force = force,
        Point = worldPoint,
        UseWorldPoint = true,
        Duration = duration,
        TimeRemaining = duration
      };
    }
  }
}