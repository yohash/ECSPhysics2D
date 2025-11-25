using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Configuration singleton for shape spawning behavior.
  /// </summary>
  public struct ShapeSpawnerConfig : IComponentData
  {
    public float SpawnHeight;           // Y position where shapes spawn
    public float SpawnAreaRadius;       // Horizontal spawn radius
    public float MinSize;               // Minimum shape size
    public float MaxSize;               // Maximum shape size
    public float SpawnCooldown;         // Time between continuous spawns (Space held)
    public int MaxBodies;               // Maximum dynamic bodies before cleanup starts

    // Runtime state for continuous spawning
    public float TimeSinceLastSpawn;

    public static ShapeSpawnerConfig CreateDefault()
    {
      return new ShapeSpawnerConfig
      {
        SpawnHeight = 10f,
        SpawnAreaRadius = 5f,
        MinSize = 0.3f,
        MaxSize = 1.2f,
        SpawnCooldown = 0.1f,
        MaxBodies = 200,
        TimeSinceLastSpawn = 0f
      };
    }
  }
}
