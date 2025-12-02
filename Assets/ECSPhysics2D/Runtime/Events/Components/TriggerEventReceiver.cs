using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag component marking an entity as interested in trigger events.
  /// </summary>
  public struct TriggerEventReceiver : IComponentData { }
}