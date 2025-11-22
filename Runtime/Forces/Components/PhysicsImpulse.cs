using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// One-shot impulse applied once then removed.
  /// Good for: explosions, jumping, bullet impacts, collisions.
  /// </summary>
  public struct PhysicsImpulse : IComponentData
  {
    public float2 Impulse;         // Impulse vector in Newton-seconds
    public float2 Point;           // World point where impulse is applied
    public bool UseWorldPoint;     // If false, impulse applied at center of mass

    public static PhysicsImpulse CreateCentralImpulse(float2 impulse)
    {
      return new PhysicsImpulse
      {
        Impulse = impulse,
        Point = float2.zero,
        UseWorldPoint = false
      };
    }

    public static PhysicsImpulse CreatePointImpulse(float2 impulse, float2 worldPoint)
    {
      return new PhysicsImpulse
      {
        Impulse = impulse,
        Point = worldPoint,
        UseWorldPoint = true
      };
    }
  }

}