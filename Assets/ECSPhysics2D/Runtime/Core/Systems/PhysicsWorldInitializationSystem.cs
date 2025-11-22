using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Initializes the physics world singleton at startup.
  /// </summary>
  [UpdateInGroup(typeof(InitializationSystemGroup))]
  public partial struct PhysicsWorldInitializationSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      // Create the physics world singleton
      var singleton = PhysicsWorldSingleton.CreateDefault();
      state.EntityManager.CreateSingleton(singleton);
    }

    public void OnDestroy(ref SystemState state)
    {
      // Clean up physics world when system is destroyed
      if (SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorld)) {
        physicsWorld.World.Destroy();
      }
    }
  }
}