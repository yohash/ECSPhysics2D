using Unity.Burst;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Monitors bodies with BodySleepMonitor and generates sleep/wake events.
  /// Unlike collision/trigger events, sleep state must be polled per-body.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(PhysicsEventGatheringSystem))]
  [UpdateBefore(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct BodySleepEventGatheringSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      ref var buffers = ref eventsSingleton.ValueRW.Buffers;

      foreach (var (body, sleepMonitor, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRW<BodySleepMonitor>>()
          .WithAll<PhysicsBodyInitialized>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        var physicsBody = body.ValueRO.Body;
        bool isSleeping = !physicsBody.awake;
        bool wasSleeping = sleepMonitor.ValueRO.WasSleeping;

        // Detect state change
        if (isSleeping != wasSleeping) {
          var evt = new BodySleepEvent
          {
            Entity = entity,
            Body = physicsBody,
            EventType = isSleeping ? SleepEventType.Sleep : SleepEventType.Wake
          };

          buffers.SleepEvents.Add(evt);

          // Update tracked state
          sleepMonitor.ValueRW.WasSleeping = isSleeping;
        }
      }

      // Update statistics
      eventsSingleton.ValueRW.LastFrameStats = buffers.GetStats();
    }
  }
}