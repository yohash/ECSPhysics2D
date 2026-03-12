using Unity.Entities;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Bakes MultiWorldDemoSceneAuthoring into ECS singleton config.
  /// </summary>
  public class MultiWorldDemoSceneBaker : Baker<MultiWorldDemoSceneAuthoring>
  {
    public override void Bake(MultiWorldDemoSceneAuthoring authoring)
    {
      var entity = GetEntity(TransformUsageFlags.None);

      AddComponent(entity, new MultiWorldDemoConfig
      {
        SpawnPosition = authoring.SpawnPosition,
        SpawnInterval = authoring.SpawnInterval,
        CircleRadius = authoring.CircleRadius,
        CircleSpawnArea = authoring.CircleSpawnArea,
        TimeSinceLastSpawn = 0f,
        MaxCircles = authoring.MaxCircles,
        CurrentCircleCount = 0,
        ExplosionTriggerY = authoring.ExplosionTriggerY,
        DebrisCount = authoring.DebrisCount,
        DebrisRadius = authoring.DebrisRadius,
        DebrisSpreadSpeed = authoring.DebrisSpreadSpeed,
        DebrisLifetime = authoring.DebrisLifetime
      });
    }
  }
}
