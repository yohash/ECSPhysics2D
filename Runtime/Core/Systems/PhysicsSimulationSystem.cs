using Unity.Burst;
using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Runs the Box2D physics simulation.
  /// This is where all the physics magic happens.
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
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      // Run the Box2D simulation step
      physicsWorldSingleton.World.Simulate(physicsWorldSingleton.FixedDeltaTime);
    }
  }
}