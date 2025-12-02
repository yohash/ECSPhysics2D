using Unity.Burst;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Gathers joint threshold events from Box2D after simulation completes.
  /// 
  /// Unlike the callback-based approach (GetJointThresholdCallbackTargets + 
  /// SendJointThresholdCallbacks), this system directly polls jointThresholdEvents
  /// for Burst compatibility and ECS-native event handling.
  /// 
  /// Pipeline position: After simulation, alongside other event gathering systems.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(PhysicsSimulationSystem))]
  [UpdateBefore(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct JointThresholdEventGatheringSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var worldSingleton))
        return;

      if (!SystemAPI.TryGetSingletonRW<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      var physicsWorld = worldSingleton.World;
      ref var buffers = ref eventsSingleton.ValueRW.Buffers;
      var currentTime = SystemAPI.Time.ElapsedTime;

      GatherJointThresholdEvents(physicsWorld, ref buffers, currentTime);
    }

    private void GatherJointThresholdEvents(
        UnityEngine.LowLevelPhysics2D.PhysicsWorld world,
        ref PhysicsEventBuffers buffers,
        double currentTime)
    {
      var thresholdEvents = world.jointThresholdEvents;

      for (int i = 0; i < thresholdEvents.Length; i++) {
        var rawEvent = thresholdEvents[i];
        var joint = rawEvent.joint;

        // Skip invalid joints
        if (!joint.isValid)
          continue;

        // Unpack entity from joint userData (set during joint creation)
        // We'll unpack entities A and B and compare the two to determine whether
        // we fire off one or two events per joint threshold exceedance.
        var jointEntityA = joint.bodyA.GetEntityUserData();
        var jointEntityB = joint.bodyB.GetEntityUserData();

        // Skip if neither entity could be retrieved
        if (jointEntityA == Entity.Null && jointEntityB == Entity.Null)
          continue;

        // compare entities to avoid duplicate events
        var areTheSame = jointEntityA == jointEntityB;
        if (areTheSame) {
          // Both bodies point to the same entity - only fire one event
          // and continue to next event
          var evt = new JointThresholdEvent
          {
            JointEntity = jointEntityA,
            Joint = joint,
            TimeStamp = currentTime,
            ConstraintForce = joint.currentConstraintForce,
            ConstraintTorque = joint.currentConstraintTorque
          };
          buffers.JointThresholds.Add(evt);
          continue;
        }

        // Fire event for body A
        if (jointEntityA != Entity.Null) {
          var evt = new JointThresholdEvent
          {
            JointEntity = jointEntityA,
            Joint = joint,
            TimeStamp = currentTime,
            ConstraintForce = joint.currentConstraintForce,
            ConstraintTorque = joint.currentConstraintTorque
          };

          buffers.JointThresholds.Add(evt);
        }

        // Fire event for body B
        if (jointEntityB != Entity.Null) {
          var evtB = new JointThresholdEvent
          {
            JointEntity = jointEntityB,
            Joint = joint,
            TimeStamp = currentTime,
            ConstraintForce = joint.currentConstraintForce,
            ConstraintTorque = joint.currentConstraintTorque
          };
          buffers.JointThresholds.Add(evtB);
        }
      }
    }
  }
}