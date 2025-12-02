using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag component marking an entity as interested in collision events.
  /// Gameplay systems can use this to filter which entities to check
  /// against the collision event stream.
  /// 
  /// Optional optimization - not required to receive events, but useful
  /// for building entity queries of "things that care about collisions."
  /// </summary>
  public struct CollisionEventReceiver : IComponentData { }
}