using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Body sleep state change event.
  /// Useful for optimization callbacks (pause processing for sleeping bodies).
  /// </summary>
  public struct BodySleepEvent
  {
    public Entity Entity;
    public PhysicsBody Body;
    public SleepEventType EventType;
  }
}