using Unity.Entities;
using UnityEngine;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Authoring component for the circle spawner.
  /// Place this on a GameObject in the scene to enable spawning.
  /// </summary>
  public class CircleSpawnerAuthoring : MonoBehaviour
  {
    [Header("Spawn Configuration")]
    public float SpawnHeight = 12f;
    public float SpawnRangeX = 8f;
    public float SpawnInterval = 0.7f;

    [Header("Circle Properties")]
    public float MinRadius = 0.3f;
    public float MaxRadius = 1.0f;
    public float Density = 1.0f;

    [Header("Limits")]
    public int MaxCircles = 150;
  }

  public class CircleBaker : Baker<CircleSpawnerAuthoring>
  {
    public override void Bake(CircleSpawnerAuthoring authoring)
    {
      var entity = GetEntity(TransformUsageFlags.None);

      AddComponent(entity, CircleSpawnerConfig.Create(
          spawnHeight: authoring.SpawnHeight,
          spawnRangeX: authoring.SpawnRangeX,
          spawnInterval: authoring.SpawnInterval,
          minRadius: authoring.MinRadius,
          maxRadius: authoring.MaxRadius,
          density: authoring.Density,
          maxCircles: authoring.MaxCircles
      ));
    }
  }
}