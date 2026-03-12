using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Configuration for spawning falling circles.
  /// </summary>
  public struct CircleSpawnerConfig : IComponentData
  {
    public float SpawnHeight;        // Y position to spawn at
    public float SpawnRangeX;        // Random X range: -SpawnRangeX to +SpawnRangeX
    public float SpawnInterval;      // Time between spawns (seconds)
    public float2 RadiusRange;       // Min/max circle radius
    public float Density;            // Constant density (mass = density * πr²)
    public int MaxCircles;           // Maximum concurrent circles
    public float TimeUntilNextSpawn; // Internal timer

    public static CircleSpawnerConfig Create(
        float spawnHeight = 12f,
        float spawnRangeX = 8f,
        float spawnInterval = 0.7f,
        float minRadius = 0.3f,
        float maxRadius = 1.0f,
        float density = 1.0f,
        int maxCircles = 150)
    {
      return new CircleSpawnerConfig
      {
        SpawnHeight = spawnHeight,
        SpawnRangeX = spawnRangeX,
        SpawnInterval = spawnInterval,
        RadiusRange = new float2(minRadius, maxRadius),
        Density = density,
        MaxCircles = maxCircles,
        TimeUntilNextSpawn = 0f
      };
    }
  }

  /// <summary>
  /// Tag for circles spawned by the spawner (for cleanup/counting).
  /// </summary>
  public struct SpawnedCircleTag : IComponentData { }
}
