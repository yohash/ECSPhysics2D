using UnityEngine;

namespace ECSPhysics2D.Samples.FallingCascade
{
  public class FallingCascadeSampleScene : MonoBehaviour
  {
    [Header("Spawn Configuration")]
    public float SpawnHeight = 12f;
    public float SpawnRate = 0.2f;
    public float CircleRadius = 0.3f;
    public float SpawnRangeX = 8f;
    public int MaxCircles = 200;
  }
}