namespace ECSPhysics2D
{
  /// <summary>
  /// Configuration for physics event buffer allocation.
  /// </summary>
  public struct PhysicsEventConfig
  {
    // Collision events
    public int CollisionHotBufferSize;

    // Trigger events
    public int TriggerHotBufferSize;

    // Sleep events
    public int SleepHotBufferSize;

    // Joint threshold events
    public int JointThresholdHotBufferSize;
    public int JointThresholdOverflowInitialCapacity;

    // Shared overflow settings
    public int OverflowInitialCapacity;

    public static PhysicsEventConfig Default => new PhysicsEventConfig
    {
      // Hot buffer sizes (zero-allocation fast path)
      CollisionHotBufferSize = 256,
      TriggerHotBufferSize = 128,
      SleepHotBufferSize = 64,
      JointThresholdHotBufferSize = 32,  // Fewer joints than collisions typically

      // Overflow settings
      OverflowInitialCapacity = 1024,
      JointThresholdOverflowInitialCapacity = 128,  // Smaller for joints
    };
  }
}