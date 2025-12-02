using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Initializes the physics events singleton and buffers at startup.
  /// </summary>
  [UpdateInGroup(typeof(InitializationSystemGroup))]
  [UpdateAfter(typeof(PhysicsWorldInitializationSystem))]
  public partial struct PhysicsEventsInitializationSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      var config = PhysicsEventConfig.Default;

      var singleton = new PhysicsEventsSingleton
      {
        Buffers = PhysicsEventBuffers.Create(config),
        Config = config,
        LastFrameStats = default
      };

      state.EntityManager.CreateSingleton(singleton, "PhysicsEventsSingleton");
    }

    public void OnDestroy(ref SystemState state)
    {
      if (SystemAPI.TryGetSingletonRW<PhysicsEventsSingleton>(out var singleton)) {
        singleton.ValueRW.Buffers.Dispose();
      }
    }
  }
}