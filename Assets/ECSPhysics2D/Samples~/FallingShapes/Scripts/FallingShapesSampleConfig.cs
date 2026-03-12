using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Singleton config for Falling Shapes sample.
  /// Presence of this component enables all Falling Shapes systems.
  /// </summary>
  public struct FallingShapesSampleConfig : IComponentData
  {
    public float SpawnHeight;
    public float SpawnAreaRadius;
    public float MinSize;
    public float MaxSize;
    public float SpawnCooldown;
    public int MaxBodies;
    public float TimeSinceLastSpawn;

    public static FallingShapesSampleConfig CreateDefault()
    {
      return new FallingShapesSampleConfig
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