using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Singleton component that holds the PhysicsWorld instance.
  /// Only one should exist per ECS World.
  /// </summary>
  public struct PhysicsWorldSingleton : IComponentData
  {
    public PhysicsWorld World;
    public float FixedDeltaTime;

    /// <summary>
    /// Creates a default physics world with standard gravity
    /// </summary>
    public static PhysicsWorldSingleton CreateDefault()
    {
      var worldDef = new PhysicsWorldDefinition
      {
        gravity = new float2(0f, -9.81f),
        transformPlane = PhysicsWorld.TransformPlane.XY,
        simulationSubSteps = 4,
        contactFrequency = 30f,
        contactDamping = 1f,
      };

      return new PhysicsWorldSingleton
      {
        World = PhysicsWorld.Create(worldDef),
        FixedDeltaTime = 1f / 60f // 60Hz physics
      };
    }
  }
}