using Unity.Burst;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// SAMPLE: Demonstrates consuming collision events to apply damage.
  /// Not part of core package - move to game-specific assembly.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
  [BurstCompile]
  public partial struct SampleCollisionDamageSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      ref var buffers = ref eventsSingleton.Buffers;

      // Process all collision events
      for (int i = 0; i < buffers.Collisions.Count; i++) {
        var evt = buffers.Collisions[i];

        // Only process collision begin events with significant impulse
        if (evt.EventType != CollisionEventType.Begin)
          continue;

        if (evt.NormalImpulse < 5f)  // Threshold for damage
          continue;

        // Apply damage to entities that have a damage component
        TryApplyDamage(ref state, evt.EntityA, evt.NormalImpulse);
        TryApplyDamage(ref state, evt.EntityB, evt.NormalImpulse);
      }
    }

    private void TryApplyDamage(ref SystemState state, Entity entity, float impulse)
    {
      // Example: Check if entity has a "Health" component and reduce it
      // This is game-specific - replace with your own damage system
      //
      // if (SystemAPI.HasComponent<Health>(entity))
      // {
      //     var health = SystemAPI.GetComponentRW<Health>(entity);
      //     health.ValueRW.Current -= impulse * damageMultiplier;
      // }
    }
  }
}