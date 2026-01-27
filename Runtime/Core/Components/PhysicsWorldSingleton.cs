using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Singleton component that holds all PhysicsWorld instances.
  /// Supports multiple worlds for partitioned simulation.
  /// 
  /// Access worlds via GetWorld(index) or the default World property (index 0).
  /// </summary>
  public struct PhysicsWorldSingleton : IComponentData, IDisposable
  {
    /// <summary>
    /// Internal storage for physics worlds. Use GetWorld() for access.
    /// </summary>
    internal PhysicsWorld World0;
    internal PhysicsWorld World1;
    internal PhysicsWorld World2;
    internal PhysicsWorld World3;
    internal PhysicsWorld World4;
    internal PhysicsWorld World5;
    internal PhysicsWorld World6;
    internal PhysicsWorld World7;

    /// <summary>
    /// Number of active physics worlds.
    /// </summary>
    public int WorldCount;

    /// <summary>
    /// Fixed timestep for physics simulation.
    /// </summary>
    public float FixedDeltaTime;

    /// <summary>
    /// Default world (index 0) for backward compatibility.
    /// </summary>
    public PhysicsWorld World => World0;

    /// <summary>
    /// Gets a physics world by index.
    /// </summary>
    public PhysicsWorld GetWorld(int index)
    {
      return index switch
      {
        0 => World0,
        1 => World1,
        2 => World2,
        3 => World3,
        4 => World4,
        5 => World5,
        6 => World6,
        7 => World7,
        _ => World0
      };
    }

    /// <summary>
    /// Sets a physics world at the specified index.
    /// </summary>
    internal void SetWorld(int index, PhysicsWorld world)
    {
      switch (index) {
        case 0:
          World0 = world;
          break;
        case 1:
          World1 = world;
          break;
        case 2:
          World2 = world;
          break;
        case 3:
          World3 = world;
          break;
        case 4:
          World4 = world;
          break;
        case 5:
          World5 = world;
          break;
        case 6:
          World6 = world;
          break;
        case 7:
          World7 = world;
          break;
      }
    }

    /// <summary>
    /// Checks if a world index is valid.
    /// </summary>
    public bool IsValidWorldIndex(int index)
    {
      return index >= 0 && index < WorldCount;
    }

    /// <summary>
    /// Creates a default single-world physics configuration.
    /// </summary>
    public static PhysicsWorldSingleton CreateDefault()
    {
      var worldDef = new PhysicsWorldDefinition
      {
        gravity = new float2(0f, -9.81f),
        transformPlane = PhysicsWorld.TransformPlane.XY,
        simulateType = PhysicsWorld.SimulationType.Script,
        simulationSubSteps = 4,
        contactFrequency = 30f,
        contactDamping = 1f,
        contactHitEventThreshold = 0.01f,
      };

      return new PhysicsWorldSingleton
      {
        World0 = PhysicsWorld.Create(worldDef),
        WorldCount = 1,
        FixedDeltaTime = 1f / 60f
      };
    }

    /// <summary>
    /// Creates a multi-world physics configuration from MultiWorldConfiguration.
    /// </summary>
    public static PhysicsWorldSingleton Create(MultiWorldConfiguration config)
    {
      var singleton = new PhysicsWorldSingleton
      {
        WorldCount = config.WorldCount,
        FixedDeltaTime = config.FixedDeltaTime
      };

      for (int i = 0; i < config.WorldCount; i++) {
        var worldConfig = config.GetWorldConfig(i);
        var worldDef = worldConfig.ToDefinition();
        singleton.SetWorld(i, PhysicsWorld.Create(worldDef));
      }

      return singleton;
    }

    /// <summary>
    /// Disposes all physics worlds.
    /// </summary>
    public void Dispose()
    {
      for (int i = 0; i < WorldCount; i++) {
        var world = GetWorld(i);
        if (world.isValid) {
          world.Destroy();
        }
      }
      WorldCount = 0;
    }
  }
}
