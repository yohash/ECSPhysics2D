using UnityEngine;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// MonoBehaviour that configures the MultiWorldDemo sample scene.
  /// Place on a GameObject in the SubScene to activate all sample systems.
  /// </summary>
  public class MultiWorldDemoSceneAuthoring : MonoBehaviour
  {
    [Header("Circle Spawning")]
    [Tooltip("Position where circles spawn")]
    public Vector3 SpawnPosition = new Vector3(0f, 8f, 0f);

    [Tooltip("Time between circle spawns")]
    public float SpawnInterval = 1.5f;

    [Tooltip("Radius of spawned circles")]
    public float CircleRadius = 0.5f;

    [Tooltip("X-area circles will spawn")]
    public float CircleSpawnArea = 2f;

    [Tooltip("Maximum number of circles in scene")]
    public int MaxCircles = 10;

    [Header("Explosion Settings")]
    [Tooltip("Y position that triggers explosion")]
    public float ExplosionTriggerY = -2f;

    [Tooltip("Number of debris pieces per explosion")]
    public int DebrisCount = 12;

    [Tooltip("Radius of debris circles")]
    public float DebrisRadius = 0.15f;

    [Tooltip("Initial spread speed of debris")]
    public float DebrisSpreadSpeed = 5f;

    [Tooltip("How long debris lives before cleanup")]
    public float DebrisLifetime = 4f;
  }
}
