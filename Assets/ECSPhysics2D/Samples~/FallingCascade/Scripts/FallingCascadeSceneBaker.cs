using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingCascade
{
  public class FallingCascadeSceneBaker : Baker<FallingCascadeSampleScene>
  {
    public override void Bake(FallingCascadeSampleScene authoring)
    {
      var spawner = GetEntity(TransformUsageFlags.None);

      AddComponent(spawner, new CascadeCircleSpawner
      {
        SpawnHeight = authoring.SpawnHeight,
        SpawnRate = authoring.SpawnRate,
        TimeSinceLastSpawn = 0f,
        CircleRadius = authoring.CircleRadius,
        SpawnRangeX = authoring.SpawnRangeX,
        MaxCircles = authoring.MaxCircles
      });
    }
  }
}