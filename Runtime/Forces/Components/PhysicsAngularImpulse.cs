using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Angular impulse (instant rotation change).
  /// </summary>
  public struct PhysicsAngularImpulse : IComponentData
  {
    public float Impulse;          // Angular impulse in Newton-meter-seconds
  }
}