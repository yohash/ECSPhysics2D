using UnityEngine;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// MonoBehaviour that enables the Falling Shapes sample scene.
  /// Place on a GameObject in the SubScene to activate all sample systems.
  /// </summary>
  public class FallingShapesSampleScene : MonoBehaviour
  {
    [Header("Spawn Configuration")]
    public float SpawnHeight = 10f;
    public float SpawnAreaRadius = 5f;
    public float MinSize = 0.3f;
    public float MaxSize = 1.2f;
    public float SpawnCooldown = 0.1f;
    public int MaxBodies = 200;
  }
}