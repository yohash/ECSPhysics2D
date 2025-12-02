using Unity.Collections;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Extension methods for working with physics events.
  /// </summary>
  public static class PhysicsEventExtensions
  {
    /// <summary>
    /// Find all collision events involving a specific entity.
    /// Allocates result list with specified allocator.
    /// </summary>
    public static NativeList<CollisionEvent> GetCollisionsFor(
        this ref PhysicsEventBuffers buffers,
        Entity entity,
        Allocator allocator)
    {
      var results = new NativeList<CollisionEvent>(16, allocator);

      for (int i = 0; i < buffers.Collisions.Count; i++) {
        var evt = buffers.Collisions[i];
        if (evt.Involves(entity)) {
          results.Add(evt);
        }
      }

      return results;
    }

    /// <summary>
    /// Check if entity had any collision begin events this frame.
    /// </summary>
    public static bool HadCollisionBegin(
        this ref PhysicsEventBuffers buffers,
        Entity entity)
    {
      for (int i = 0; i < buffers.Collisions.Count; i++) {
        var evt = buffers.Collisions[i];
        if (evt.EventType == CollisionEventType.Begin && evt.Involves(entity)) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Get the strongest collision impact for an entity this frame.
    /// Returns null if no collisions.
    /// </summary>
    public static CollisionEvent? GetStrongestImpact(
        this ref PhysicsEventBuffers buffers,
        Entity entity)
    {
      CollisionEvent? strongest = null;
      float maxImpulse = 0f;

      for (int i = 0; i < buffers.Collisions.Count; i++) {
        var evt = buffers.Collisions[i];
        if (evt.EventType == CollisionEventType.Begin &&
            evt.Involves(entity) &&
            evt.NormalImpulse > maxImpulse) {
          maxImpulse = evt.NormalImpulse;
          strongest = evt;
        }
      }

      return strongest;
    }

    // ========================================================================
    // Trigger Event Extensions
    // ========================================================================

    /// <summary>
    /// Find all trigger events involving a specific entity.
    /// </summary>
    public static NativeList<TriggerEvent> GetTriggersFor(
        this ref PhysicsEventBuffers buffers,
        Entity entity,
        Allocator allocator)
    {
      var results = new NativeList<TriggerEvent>(16, allocator);

      for (int i = 0; i < buffers.Triggers.Count; i++) {
        var evt = buffers.Triggers[i];
        if (evt.Involves(entity)) {
          results.Add(evt);
        }
      }

      return results;
    }

    /// <summary>
    /// Check if entity entered any trigger this frame.
    /// </summary>
    public static bool EnteredTrigger(
        this ref PhysicsEventBuffers buffers,
        Entity entity)
    {
      for (int i = 0; i < buffers.Triggers.Count; i++) {
        var evt = buffers.Triggers[i];
        if (evt.EventType == TriggerEventType.Enter && evt.Involves(entity)) {
          return true;
        }
      }
      return false;
    }

    // ========================================================================
    // Joint Threshold Event Extensions
    // ========================================================================

    /// <summary>
    /// Find all joint threshold events for a specific joint entity.
    /// </summary>
    public static NativeList<JointThresholdEvent> GetJointThresholdsFor(
        this ref PhysicsEventBuffers buffers,
        Entity jointEntity,
        Allocator allocator)
    {
      var results = new NativeList<JointThresholdEvent>(4, allocator);

      for (int i = 0; i < buffers.JointThresholds.Count; i++) {
        var evt = buffers.JointThresholds[i];
        if (evt.JointEntity == jointEntity) {
          results.Add(evt);
        }
      }

      return results;
    }

    /// <summary>
    /// Check if a specific joint exceeded its threshold this frame.
    /// </summary>
    public static bool JointExceededThreshold(
        this ref PhysicsEventBuffers buffers,
        Entity jointEntity)
    {
      for (int i = 0; i < buffers.JointThresholds.Count; i++) {
        if (buffers.JointThresholds[i].JointEntity == jointEntity) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Get the first joint threshold event for a joint entity, if any.
    /// </summary>
    public static JointThresholdEvent? GetFirstJointThreshold(
        this ref PhysicsEventBuffers buffers,
        Entity jointEntity)
    {
      for (int i = 0; i < buffers.JointThresholds.Count; i++) {
        var evt = buffers.JointThresholds[i];
        if (evt.JointEntity == jointEntity) {
          return evt;
        }
      }
      return null;
    }

    /// <summary>
    /// Find all joint threshold events for joints connected to a body entity.
    /// Requires the body to have a JointReference buffer.
    /// </summary>
    public static NativeList<JointThresholdEvent> GetJointThresholdsForBody(
        this ref PhysicsEventBuffers buffers,
        DynamicBuffer<JointReference> jointRefs,
        Allocator allocator)
    {
      var results = new NativeList<JointThresholdEvent>(jointRefs.Length, allocator);

      for (int i = 0; i < jointRefs.Length; i++) {
        var jointEntity = jointRefs[i].JointEntity;

        for (int j = 0; j < buffers.JointThresholds.Count; j++) {
          var evt = buffers.JointThresholds[j];
          if (evt.JointEntity == jointEntity) {
            results.Add(evt);
          }
        }
      }

      return results;
    }

    /// <summary>
    /// Check if any joint connected to a body exceeded its threshold this frame.
    /// </summary>
    public static bool AnyConnectedJointExceededThreshold(
        this ref PhysicsEventBuffers buffers,
        DynamicBuffer<JointReference> jointRefs)
    {
      for (int i = 0; i < jointRefs.Length; i++) {
        var jointEntity = jointRefs[i].JointEntity;

        for (int j = 0; j < buffers.JointThresholds.Count; j++) {
          if (buffers.JointThresholds[j].JointEntity == jointEntity) {
            return true;
          }
        }
      }
      return false;
    }
  }
}