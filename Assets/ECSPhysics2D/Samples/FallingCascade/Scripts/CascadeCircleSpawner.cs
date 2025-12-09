using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingCascade
{
  /// <summary>
  /// Configuration singleton for cascade circle spawning.
  /// </summary>
  public struct CascadeCircleSpawner : IComponentData
  {
    public float SpawnHeight;
    public float SpawnRate;
    public float TimeSinceLastSpawn;
    public float CircleRadius;
    public float SpawnRangeX;
    public int MaxCircles;

    public static CascadeCircleSpawner CreateDefault()
    {
      return new CascadeCircleSpawner
      {
        SpawnHeight = 12f,
        SpawnRate = 0.2f,
        TimeSinceLastSpawn = 0f,
        CircleRadius = 0.3f,
        SpawnRangeX = 8f,
        MaxCircles = 200
      };
    }
  }
}