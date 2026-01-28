using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Singleton configuration for the MultiWorldDemo sample.
  /// Presence of this component enables all MultiWorldDemo systems.
  /// </summary>
  public struct MultiWorldDemoConfig : IComponentData
  {
    // Spawning
    public float3 SpawnPosition;
    public float SpawnInterval;
    public float CircleRadius;
    public float CircleSpawnArea;
    public float TimeSinceLastSpawn;
    public int MaxCircles;
    public int CurrentCircleCount;

    // Explosion
    public float ExplosionTriggerY;
    public int DebrisCount;
    public float DebrisRadius;
    public float DebrisSpreadSpeed;
    public float DebrisLifetime;

    public static MultiWorldDemoConfig Default => new MultiWorldDemoConfig
    {
      SpawnPosition = new float3(0f, 8f, 0f),
      SpawnInterval = 1.5f,
      CircleRadius = 0.5f,
      CircleSpawnArea = 2f,
      TimeSinceLastSpawn = 0f,
      MaxCircles = 10,
      CurrentCircleCount = 0,
      ExplosionTriggerY = -2f,
      DebrisCount = 12,
      DebrisRadius = 0.15f,
      DebrisSpreadSpeed = 5f,
      DebrisLifetime = 4f
    };
  }
}
