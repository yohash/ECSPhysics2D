using Unity.Burst;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// SAMPLE: Demonstrates consuming trigger events for zone detection.
  /// Not part of core package - move to game-specific assembly.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
  [BurstCompile]
  public partial struct SampleTriggerZoneSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      ref var buffers = ref eventsSingleton.Buffers;

      // Process all trigger events
      for (int i = 0; i < buffers.Triggers.Count; i++) {
        var evt = buffers.Triggers[i];

        switch (evt.EventType) {
          case TriggerEventType.Enter:
            OnTriggerEnter(ref state, evt);
            break;

          case TriggerEventType.Exit:
            OnTriggerExit(ref state, evt);
            break;
        }
      }
    }

    private void OnTriggerEnter(ref SystemState state, TriggerEvent evt)
    {
      // Example: Check if this is a pickup zone
      // if (SystemAPI.HasComponent<PickupZone>(evt.TriggerEntity) &&
      //     SystemAPI.HasComponent<CanPickup>(evt.OtherEntity))
      // {
      //     // Collect the pickup
      //     var pickup = SystemAPI.GetComponent<PickupZone>(evt.TriggerEntity);
      //     // Add to inventory, destroy pickup, etc.
      // }
    }

    private void OnTriggerExit(ref SystemState state, TriggerEvent evt)
    {
      // Example: Player left a danger zone
      // if (SystemAPI.HasComponent<DangerZone>(evt.TriggerEntity) &&
      //     SystemAPI.HasComponent<Player>(evt.OtherEntity))
      // {
      //     // Stop applying damage-over-time
      // }
    }
  }
}
