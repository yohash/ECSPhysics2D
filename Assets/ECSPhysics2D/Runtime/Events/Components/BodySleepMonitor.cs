using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag component marking an entity for sleep state monitoring.
  /// Only entities with this tag will generate BodySleepEvents.
  /// </summary>
  public struct BodySleepMonitor : IComponentData
  {
    public bool WasSleeping;  // Previous frame's sleep state for change detection
  }
}