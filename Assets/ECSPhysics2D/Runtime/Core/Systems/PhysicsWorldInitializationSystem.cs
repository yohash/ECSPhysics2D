using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Initializes physics worlds based on MultiWorldConfiguration.
  /// 
  /// If MultiWorldConfiguration singleton exists, uses it to create multiple worlds.
  /// Otherwise, creates a single default world for backward compatibility.
  /// 
  /// To customize world setup, create a MultiWorldConfiguration singleton before
  /// this system runs (e.g., in a bootstrap system).
  /// </summary>
  [UpdateInGroup(typeof(InitializationSystemGroup))]
  public partial struct PhysicsWorldInitializationSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      PhysicsWorldSingleton singleton;

      // Check if user has provided a MultiWorldConfiguration
      if (SystemAPI.TryGetSingleton<MultiWorldConfiguration>(out var config)) {
        singleton = PhysicsWorldSingleton.Create(config);
      } else {
        // Fall back to single default world
        singleton = PhysicsWorldSingleton.CreateDefault();
      }

      state.EntityManager.CreateSingleton(singleton, "PhysicsWorldSingleton");
    }

    public void OnDestroy(ref SystemState state)
    {
      if (SystemAPI.TryGetSingletonRW<PhysicsWorldSingleton>(out var singleton)) {
        singleton.ValueRW.Dispose();
      }
    }
  }
}
