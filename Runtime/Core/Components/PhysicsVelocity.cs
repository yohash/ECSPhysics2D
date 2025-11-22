using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Stores velocity data that syncs with the physics body.
  /// Updated after each physics step for dynamic bodies.
  /// Can be set directly for kinematic bodies.
  /// </summary>
  public struct PhysicsVelocity : IComponentData
  {
    public float2 Linear;
    public float Angular;

    public static PhysicsVelocity Zero => new PhysicsVelocity
    {
      Linear = float2.zero,
      Angular = 0f
    };
  }
}