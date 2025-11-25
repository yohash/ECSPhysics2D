using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag for static bodies - never moves, only provides collision.
  /// Transform synced once at creation.
  /// </summary>
  public struct PhysicsStaticTag : IComponentData { }
}