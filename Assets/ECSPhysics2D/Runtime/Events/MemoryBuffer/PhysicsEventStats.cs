namespace ECSPhysics2D
{
  /// <summary>
  /// Per-frame statistics about event processing.
  /// </summary>
  public struct PhysicsEventStats
  {
    // Event counts
    public int CollisionEventCount;
    public int TriggerEventCount;
    public int SleepEventCount;
    public int JointThresholdEventCount;

    // Overflow flags (indicates hot buffer exceeded)
    public bool CollisionOverflowUsed;
    public bool TriggerOverflowUsed;
    public bool SleepOverflowUsed;
    public bool JointThresholdOverflowUsed;

    // Peak counts (for tuning buffer sizes)
    public int PeakCollisionCount;
    public int PeakTriggerCount;
    public int PeakSleepCount;
    public int PeakJointThresholdCount;

    /// <summary>
    /// Total events this frame across all types.
    /// </summary>
    public int TotalEventCount =>
      CollisionEventCount + TriggerEventCount + SleepEventCount + JointThresholdEventCount;

    /// <summary>
    /// Whether any overflow buffer was used this frame.
    /// If consistently true, consider increasing hot buffer sizes.
    /// </summary>
    public bool AnyOverflowUsed =>
      CollisionOverflowUsed || TriggerOverflowUsed || SleepOverflowUsed || JointThresholdOverflowUsed;
  }
}