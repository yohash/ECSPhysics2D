using Unity.Burst;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Simulates all active physics worlds.
  /// 
  /// Each world is simulated sequentially with the same fixed timestep.
  /// Worlds can be individually disabled via MultiWorldConfiguration.
  /// 
  /// Note: Future optimization could parallelize world simulation on separate threads
  /// since worlds are independent (no cross-world interactions).
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(BuildPhysicsWorldSystem))]
  [UpdateBefore(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct PhysicsSimulationSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      // Get per-world enabled state if configuration exists
      var hasConfig = SystemAPI.TryGetSingleton<MultiWorldConfiguration>(out var config);

      // Simulate each world
      for (int i = 0; i < singleton.WorldCount; i++) {
        // Check if world is enabled (default to true if no config)
        bool enabled = !hasConfig || config.GetWorldConfig(i).Enabled;
        if (!enabled)
          continue;

        var world = singleton.GetWorld(i);
        if (world.isValid) {
          world.Simulate(singleton.FixedDeltaTime);
        }
      }
    }
  }
}
