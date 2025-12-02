using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag component marking an entity as interested in joint threshold events.
  /// Optional optimization for filtering which joint entities to monitor.
  /// </summary>
  public struct JointThresholdReceiver : IComponentData { }
}