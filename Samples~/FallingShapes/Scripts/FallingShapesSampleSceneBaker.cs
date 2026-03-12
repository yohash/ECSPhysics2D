using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Bakes FallingShapesSampleScene into ECS singleton config.
  /// </summary>
  public class FallingShapesSampleSceneBaker : Baker<FallingShapesSampleScene>
  {
    public override void Bake(FallingShapesSampleScene authoring)
    {
      var entity = GetEntity(TransformUsageFlags.None);

      AddComponent(entity, new FallingShapesSampleConfig
      {
        SpawnHeight = authoring.SpawnHeight,
        SpawnAreaRadius = authoring.SpawnAreaRadius,
        MinSize = authoring.MinSize,
        MaxSize = authoring.MaxSize,
        SpawnCooldown = authoring.SpawnCooldown,
        MaxBodies = authoring.MaxBodies,
        TimeSinceLastSpawn = 0f
      });
    }
  }
}