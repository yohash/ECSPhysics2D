using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Trigger event data stored in singleton stream.
  /// Triggers detect overlap without physical response.
  /// </summary>
  public struct TriggerEvent
  {
    // Entity references
    public Entity TriggerEntity;    // The entity with PhysicsSensor
    public Entity OtherEntity;      // The entity that entered/exited

    // Physics handles
    public PhysicsBody TriggerBody;
    public PhysicsBody OtherBody;
    public PhysicsShape TriggerShape;
    public PhysicsShape OtherShape;

    // Event metadata
    public TriggerEventType EventType;

    /// <summary>
    /// Check if this event involves a specific entity.
    /// </summary>
    public bool Involves(Entity entity)
    {
      return TriggerEntity == entity || OtherEntity == entity;
    }
  }
}
