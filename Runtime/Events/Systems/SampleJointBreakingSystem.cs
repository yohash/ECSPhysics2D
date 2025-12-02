using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECSPhysics2D.Samples
{
  /// <summary>
  /// SAMPLE: Demonstrates three patterns for handling joint threshold events.
  /// 
  /// Pattern A: Immediate Breaking - Destroy joint on first threshold event
  /// Pattern B: Damage Accumulation - Track stress over time, break at threshold
  /// Pattern C: Feedback Only - Visual/audio effects without destruction
  /// 
  /// Not part of core package - move to game-specific assembly.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
  [BurstCompile]
  public partial struct SampleJointBreakingSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      ref var buffers = ref eventsSingleton.Buffers;

      // Skip if no joint threshold events this frame
      if (buffers.JointThresholds.Count == 0)
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Process all joint threshold events
      for (int i = 0; i < buffers.JointThresholds.Count; i++) {
        var evt = buffers.JointThresholds[i];

        // Skip invalid events (joint may have been destroyed)
        if (!evt.IsValid)
          continue;

        // Choose processing pattern based on game needs:
        ProcessWithImmediateBreaking(ref state, evt, ecb);
        // ProcessWithDamageAccumulation(ref state, evt, ecb);
        // ProcessWithFeedbackOnly(ref state, evt);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    /// <summary>
    /// Pattern A: Immediate Breaking
    /// Any threshold exceedance immediately destroys the joint.
    /// Simple but may feel too fragile for gameplay.
    /// </summary>
    private void ProcessWithImmediateBreaking(
        ref SystemState state,
        JointThresholdEvent evt,
        EntityCommandBuffer ecb)
    {
      // Destroy the Box2D joint
      if (evt.Joint.isValid) {
        evt.Joint.Destroy();
      }

      // Destroy the joint entity
      ecb.DestroyEntity(evt.JointEntity);

      // Optional: Spawn break effects at joint location
      // SpawnBreakEffect(evt.Joint.anchorA);
    }

    /// <summary>
    /// Pattern B: Damage Accumulation
    /// Track cumulative stress, break when health depleted.
    /// Requires JointHealth component on joint entities.
    /// </summary>
    private void ProcessWithDamageAccumulation(
        ref SystemState state,
        JointThresholdEvent evt,
        EntityCommandBuffer ecb)
    {
      // Check if joint has health tracking
      if (!SystemAPI.HasComponent<JointHealth>(evt.JointEntity))
        return;

      var health = SystemAPI.GetComponentRW<JointHealth>(evt.JointEntity);

      // Accumulate damage (each threshold event = 1 damage)
      // Could also factor in time since last event for stress rate
      health.ValueRW.CurrentHealth -= 1f;
      health.ValueRW.LastStressTime = evt.TimeStamp;

      // Check for break condition
      if (health.ValueRO.CurrentHealth <= 0f) {
        if (evt.Joint.isValid) {
          evt.Joint.Destroy();
        }
        ecb.DestroyEntity(evt.JointEntity);
      }
    }

    /// <summary>
    /// Pattern C: Feedback Only
    /// Generate visual/audio feedback without destruction.
    /// Useful for "creaking" effects or warnings.
    /// </summary>
    private void ProcessWithFeedbackOnly(
        ref SystemState state,
        JointThresholdEvent evt)
    {
      // Example: Add a "stressed" visual component
      // if (!SystemAPI.HasComponent<JointStressedVisual>(evt.JointEntity))
      // {
      //     ecb.AddComponent<JointStressedVisual>(evt.JointEntity);
      // }

      // Example: Trigger audio event
      // AudioEvents.PlayCreakSound(evt.Joint.anchorA);

      // Example: Update stress indicator UI
      // var stressUI = SystemAPI.GetComponentRW<JointStressIndicator>(evt.JointEntity);
      // stressUI.ValueRW.StressLevel = 1f;
      // stressUI.ValueRW.LastStressTime = evt.TimeStamp;
    }
  }

  /// <summary>
  /// Optional component for tracking joint health/durability.
  /// Add to joint entities that should use damage accumulation pattern.
  /// </summary>
  public struct JointHealth : IComponentData
  {
    public float MaxHealth;
    public float CurrentHealth;
    public double LastStressTime;

    public static JointHealth Create(float maxHealth)
    {
      return new JointHealth
      {
        MaxHealth = maxHealth,
        CurrentHealth = maxHealth,
        LastStressTime = 0
      };
    }

    public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
    public bool IsBroken => CurrentHealth <= 0f;
  }
}