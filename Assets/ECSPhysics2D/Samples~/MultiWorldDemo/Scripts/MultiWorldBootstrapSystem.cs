using Unity.Entities;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Bootstrap system that creates the MultiWorldConfiguration before
  /// PhysicsWorldInitializationSystem runs.
  /// 
  /// Creates a dual-world setup:
  /// - World 0: Main gameplay (circles)
  /// - World 1: Debris (lower fidelity simulation)
  /// </summary>
  [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
  public partial struct MultiWorldBootstrapSystem : ISystem
  {
    private bool initialized;

    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<MultiWorldDemoConfig>();
    }

    public void OnUpdate(ref SystemState state)
    {
      if (initialized) return;

      // Check if MultiWorldConfiguration already exists
      if (SystemAPI.HasSingleton<MultiWorldConfiguration>()) {
        initialized = true;
        return;
      }

      // Create dual-world configuration
      var config = MultiWorldConfiguration.CreateDualWorld();

      // Customize World 0 (main gameplay)
      var world0 = PhysicsWorldConfig.Default;
      config.SetWorldConfig(0, world0);

      // Customize World 1 (debris - lower fidelity)
      var world1 = PhysicsWorldConfig.Debris;
      config.SetWorldConfig(1, world1);

      state.EntityManager.CreateSingleton(config, "MultiWorldConfiguration");

      initialized = true;
    }
  }
}
