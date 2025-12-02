using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Singleton component providing access to the physics event buffers.
  /// Create via PhysicsEventsInitializationSystem.
  /// </summary>
  public struct PhysicsEventsSingleton : IComponentData
  {
    /// <summary>
    /// Reference to the event buffer container.
    /// Access via GetBuffers() to get the actual buffer struct.
    /// </summary>
    internal PhysicsEventBuffers Buffers;

    /// <summary>
    /// Configuration for buffer sizes.
    /// </summary>
    public PhysicsEventConfig Config;

    /// <summary>
    /// Statistics from last frame (for debugging/profiling).
    /// </summary>
    public PhysicsEventStats LastFrameStats;
  }
}