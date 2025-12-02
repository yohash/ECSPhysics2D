using System;

namespace ECSPhysics2D
{
  /// <summary>
  /// Container for all physics event buffers.
  /// Stored in PhysicsEventsSingleton.
  /// </summary>
  public struct PhysicsEventBuffers : IDisposable
  {
    public PhysicsEventBuffer<CollisionEvent> Collisions;
    public PhysicsEventBuffer<TriggerEvent> Triggers;
    public PhysicsEventBuffer<BodySleepEvent> SleepEvents;
    public PhysicsEventBuffer<JointThresholdEvent> JointThresholds;

    private bool _isCreated;

    public bool IsCreated => _isCreated;

    /// <summary>
    /// Create all event buffers with specified configuration.
    /// </summary>
    public static PhysicsEventBuffers Create(PhysicsEventConfig config)
    {
      return new PhysicsEventBuffers
      {
        Collisions = PhysicsEventBuffer<CollisionEvent>.Create(
          config.CollisionHotBufferSize,
          config.OverflowInitialCapacity),

        Triggers = PhysicsEventBuffer<TriggerEvent>.Create(
          config.TriggerHotBufferSize,
          config.OverflowInitialCapacity),

        SleepEvents = PhysicsEventBuffer<BodySleepEvent>.Create(
          config.SleepHotBufferSize,
          config.OverflowInitialCapacity),

        JointThresholds = PhysicsEventBuffer<JointThresholdEvent>.Create(
          config.JointThresholdHotBufferSize,
          config.JointThresholdOverflowInitialCapacity),

        _isCreated = true
      };
    }

    /// <summary>
    /// Clear all buffers for next frame.
    /// </summary>
    public void ClearAll()
    {
      Collisions.Clear();
      Triggers.Clear();
      SleepEvents.Clear();
      JointThresholds.Clear();
    }

    /// <summary>
    /// Get statistics for current frame.
    /// </summary>
    public PhysicsEventStats GetStats()
    {
      return new PhysicsEventStats
      {
        CollisionEventCount = Collisions.Count,
        TriggerEventCount = Triggers.Count,
        SleepEventCount = SleepEvents.Count,
        JointThresholdEventCount = JointThresholds.Count,
        CollisionOverflowUsed = Collisions.OverflowUsed,
        TriggerOverflowUsed = Triggers.OverflowUsed,
        SleepOverflowUsed = SleepEvents.OverflowUsed,
        JointThresholdOverflowUsed = JointThresholds.OverflowUsed,
        PeakCollisionCount = Collisions.PeakCount,
        PeakTriggerCount = Triggers.PeakCount,
        PeakSleepCount = SleepEvents.PeakCount,
        PeakJointThresholdCount = JointThresholds.PeakCount
      };
    }

    /// <summary>
    /// Dispose all buffers.
    /// </summary>
    public void Dispose()
    {
      if (_isCreated) {
        Collisions.Dispose();
        Triggers.Dispose();
        SleepEvents.Dispose();
        JointThresholds.Dispose();
        _isCreated = false;
      }
    }

    /// <summary>
    /// Trim excess memory from overflow buffers.
    /// </summary>
    public void TrimExcess(PhysicsEventConfig config)
    {
      Collisions.TrimExcess(config.OverflowInitialCapacity);
      Triggers.TrimExcess(config.OverflowInitialCapacity);
      SleepEvents.TrimExcess(config.OverflowInitialCapacity);
      JointThresholds.TrimExcess(config.JointThresholdOverflowInitialCapacity);
    }
  }
}