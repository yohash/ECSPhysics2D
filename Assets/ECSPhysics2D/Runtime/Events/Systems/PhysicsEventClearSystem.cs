using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Clears all event buffers at the end of the frame.
  /// Events are single-frame only - unprocessed events are discarded.
  /// 
  /// Runs in LateSimulationSystemGroup to ensure all gameplay systems
  /// have had a chance to process events.
  /// </summary>
  [UpdateInGroup(typeof(LateSimulationSystemGroup))]
  public partial struct PhysicsEventClearSystem : ISystem
  {
    private int _framesSinceLastTrim;
    private const int TrimIntervalFrames = 300; // Trim excess memory every ~5 seconds at 60fps

    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      ref var buffers = ref eventsSingleton.ValueRW.Buffers;

      // Store final stats before clearing
      eventsSingleton.ValueRW.LastFrameStats = buffers.GetStats();

      // Clear all buffers
      buffers.ClearAll();

      // Periodically trim excess memory from overflow buffers
      _framesSinceLastTrim++;
      if (_framesSinceLastTrim >= TrimIntervalFrames) {
        buffers.TrimExcess(eventsSingleton.ValueRO.Config);
        _framesSinceLastTrim = 0;
      }
    }
  }
}
