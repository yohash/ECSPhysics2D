using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Destroys joints that have accumulated damage beyond their threshold.
  /// When a joint breaks, connected bodies become independent.
  /// 
  /// Update order: Runs before JointCreationSystem to ensure clean state.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(JointCreationSystem))]
  [BurstCompile]
  public partial struct JointBreakingSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Query all joints with damage tracking
      foreach (var (jointComponent, damage, entity) in
          SystemAPI.Query<RefRO<PhysicsJointComponent>, RefRW<JointDamage>>()
          .WithEntityAccess()) {
        // Check if damage threshold exceeded
        if (damage.ValueRO.ShouldBreak && !damage.ValueRO.Broken) {
          // Mark as broken (prevents re-processing if destruction takes multiple frames)
          damage.ValueRW.Broken = true;

          // Destroy the Box2D joint
          if (jointComponent.ValueRO.Joint.isValid) {
            jointComponent.ValueRO.Joint.Destroy();
          }

          // Destroy the joint entity
          ecb.DestroyEntity(entity);

          // Note: Connected bodies (BodyA and BodyB) remain intact
          // They are now free to move independently
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
