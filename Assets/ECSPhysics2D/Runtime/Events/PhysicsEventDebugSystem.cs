using Unity.Entities;
using UnityEngine;

namespace ECSPhysics2D
{
  /// <summary>
  /// Optional debug system that logs event statistics.
  /// Disable in production builds.
  /// </summary>
  [UpdateInGroup(typeof(LateSimulationSystemGroup))]
  [UpdateBefore(typeof(PhysicsEventClearSystem))]
  public partial struct PhysicsEventDebugSystem : ISystem
  {
    private bool _enabled;
    private int _frameCount;
    private const int LogIntervalFrames = 60;

    public void OnCreate(ref SystemState state)
    {
      // Enable via define or runtime flag
#if PHYSICS_EVENT_DEBUG
      _enabled = true;
#else
      _enabled = false;
#endif
      _enabled = true;
    }

    public void OnUpdate(ref SystemState state)
    {
      if (!_enabled)
        return;

      if (!SystemAPI.TryGetSingleton<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      _frameCount++;

      var stats = eventsSingleton.LastFrameStats;

      // Log warnings for overflow usage
      if (stats.CollisionOverflowUsed) {
        Debug.LogWarning($"[PhysicsEvents] Collision overflow used! " +
            $"Count: {stats.CollisionEventCount}, Peak: {stats.PeakCollisionCount}. " +
            $"Consider increasing CollisionHotBufferSize.");
      }

      if (stats.TriggerOverflowUsed) {
        Debug.LogWarning($"[PhysicsEvents] Trigger overflow used! " +
            $"Count: {stats.TriggerEventCount}, Peak: {stats.PeakTriggerCount}. " +
            $"Consider increasing TriggerHotBufferSize.");
      }

      if (stats.SleepOverflowUsed) {
        Debug.LogWarning($"[PhysicsEvents] Sleep overflow used! " +
            $"Count: {stats.SleepEventCount}, Peak: {stats.PeakSleepCount}. " +
            $"Consider increasing SleepHotBufferSize.");
      }

      if (stats.JointThresholdOverflowUsed) {
        Debug.LogWarning($"[PhysicsEvents] Joint threshold overflow used! " +
            $"Count: {stats.JointThresholdEventCount}, Peak: {stats.PeakJointThresholdCount}. " +
            $"Consider increasing JointThresholdHotBufferSize.");
      }

      // Periodic summary
      //if (_frameCount >= LogIntervalFrames) {
      Debug.Log($"[PhysicsEvents] Summary - " +
          $"Collisions: {stats.CollisionEventCount} (peak: {stats.PeakCollisionCount}), " +
          $"Triggers: {stats.TriggerEventCount} (peak: {stats.PeakTriggerCount}), " +
          $"Sleep: {stats.SleepEventCount} (peak: {stats.PeakSleepCount}), " +
          $"JointThresholds: {stats.JointThresholdEventCount} (peak: {stats.PeakJointThresholdCount})");
      _frameCount = 0;
      //}
    }

    /// <summary>
    /// Enable/disable debug logging at runtime.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
      _enabled = enabled;
    }
  }
}