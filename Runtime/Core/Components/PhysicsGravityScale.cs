using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Controls how much gravity affects this body.
  /// 0 = no gravity, 1 = normal gravity, 2 = double gravity, etc.
  /// </summary>
  public struct PhysicsGravityScale : IComponentData
  {
    public float Value;

    public static PhysicsGravityScale Default => new PhysicsGravityScale { Value = 1f };
  }
}