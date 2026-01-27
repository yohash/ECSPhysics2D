using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Singleton component that configures multi-world physics.
  /// Set this before PhysicsWorldInitializationSystem runs to customize world setup.
  /// 
  /// If not present, a single default world (index 0) is created.
  /// </summary>
  public struct MultiWorldConfiguration : IComponentData
  {
    /// <summary>
    /// Number of physics worlds to create. Maximum 8.
    /// </summary>
    public int WorldCount;

    /// <summary>
    /// Fixed timestep for physics simulation (applies to all worlds).
    /// </summary>
    public float FixedDeltaTime;

    /// <summary>
    /// Per-world configurations. Use GetWorldConfig/SetWorldConfig for access.
    /// </summary>
    public PhysicsWorldConfig World0;
    public PhysicsWorldConfig World1;
    public PhysicsWorldConfig World2;
    public PhysicsWorldConfig World3;
    public PhysicsWorldConfig World4;
    public PhysicsWorldConfig World5;
    public PhysicsWorldConfig World6;
    public PhysicsWorldConfig World7;

    public const int MaxWorlds = 8;

    public PhysicsWorldConfig GetWorldConfig(int index)
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
        _ => PhysicsWorldConfig.Default
      };
    }

    public void SetWorldConfig(int index, PhysicsWorldConfig config)
    {
      switch (index) {
        case 0:
          World0 = config;
          break;
        case 1:
          World1 = config;
          break;
        case 2:
          World2 = config;
          break;
        case 3:
          World3 = config;
          break;
        case 4:
          World4 = config;
          break;
        case 5:
          World5 = config;
          break;
        case 6:
          World6 = config;
          break;
        case 7:
          World7 = config;
          break;
      }
    }

    /// <summary>
    /// Creates a default single-world configuration.
    /// </summary>
    public static MultiWorldConfiguration CreateSingleWorld()
    {
      return new MultiWorldConfiguration
      {
        WorldCount = 1,
        FixedDeltaTime = 1f / 60f,
        World0 = PhysicsWorldConfig.Default
      };
    }

    /// <summary>
    /// Creates a dual-world configuration: main gameplay + debris.
    /// </summary>
    public static MultiWorldConfiguration CreateDualWorld()
    {
      return new MultiWorldConfiguration
      {
        WorldCount = 2,
        FixedDeltaTime = 1f / 60f,
        World0 = PhysicsWorldConfig.Default,
        World1 = PhysicsWorldConfig.Debris
      };
    }

    /// <summary>
    /// Creates a custom multi-world configuration.
    /// </summary>
    public static MultiWorldConfiguration Create(int worldCount, float fixedDeltaTime = 1f / 60f)
    {
      var config = new MultiWorldConfiguration
      {
        WorldCount = math.clamp(worldCount, 1, MaxWorlds),
        FixedDeltaTime = fixedDeltaTime
      };

      // Initialize all worlds with default config
      for (int i = 0; i < config.WorldCount; i++) {
        config.SetWorldConfig(i, PhysicsWorldConfig.Default);
      }

      return config;
    }
  }
}
