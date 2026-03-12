using Unity.Burst;
using Unity.Entities;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Listens to collision events and accumulates damage on joints.
  /// When bodies connected by a joint collide with other objects,
  /// the joint accumulates damage based on impact force.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
  [BurstCompile]
  public partial struct JointDamageAccumulationSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      // Access collision events from physics singleton
      if (!SystemAPI.TryGetSingleton<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      ref var buffers = ref eventsSingleton.Buffers;

      // Skip if no collision events this frame
      if (buffers.Collisions.Count == 0)
        return;

      // Process each collision event
      for (int i = 0; i < buffers.Collisions.Count; i++) {
        var collision = buffers.Collisions[i];

        // Only process collision begin events (initial impact)
        if (collision.EventType != CollisionEventType.Begin)
          continue;

        // Ignore weak collisions (below damage threshold)
        const float minDamageImpulse = 1f;
        if (collision.NormalImpulse < minDamageImpulse)
          continue;

        // Find and damage all joints connected to these colliding bodies
        ApplyDamageToConnectedJoints(
            ref state,
            collision.EntityA,
            collision.EntityB,
            collision.NormalImpulse);
      }
    }

    /// <summary>
    /// Finds joints that connect the colliding bodies and applies damage.
    /// </summary>
    private void ApplyDamageToConnectedJoints(
        ref SystemState state,
        Entity entityA,
        Entity entityB,
        float impulse)
    {
      // Damage multiplier (tune this for gameplay feel)
      const float damageMultiplier = 1.0f;
      float damageAmount = impulse * damageMultiplier;

      // Query all joints with damage tracking
      foreach (var (jointComponent, damage) in
          SystemAPI.Query<RefRO<PhysicsJointComponent>, RefRW<JointDamage>>()) {
        // Check if this joint connects either of the colliding bodies
        bool involvesBodyA =
            jointComponent.ValueRO.BodyA == entityA ||
            jointComponent.ValueRO.BodyA == entityB;

        bool involvesBodyB =
            jointComponent.ValueRO.BodyB == entityA ||
            jointComponent.ValueRO.BodyB == entityB;

        // If collision involves at least one body connected to this joint
        if (involvesBodyA || involvesBodyB) {
          // Accumulate damage
          damage.ValueRW.Accumulated += damageAmount;

          // Optional: Cap damage at threshold (prevents overflow)
          if (damage.ValueRO.Accumulated > damage.ValueRO.BreakThreshold) {
            damage.ValueRW.Accumulated = damage.ValueRO.BreakThreshold;
          }
        }
      }
    }
  }
}
