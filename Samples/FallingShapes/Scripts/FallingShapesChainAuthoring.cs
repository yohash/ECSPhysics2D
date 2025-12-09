using UnityEngine;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Authoring component for creating physics chain shapes.
  /// </summary>
  public class FallingShapesChainAuthoring : MonoBehaviour
  {
    [Header("Chain Geometry")]
    public float Width = 20f;
    public int SegmentCount = 20;
    public bool IsLoop = false;

    [Header("Curve (optional)")]
    public bool UseCurve = false;
    public float CurveDepth = 0f;

    [Header("Physics Material")]
    public float Friction = 0.3f;
    public float Bounciness = 0.5f;
    public float Density = 1f;

    [Header("Collision Layer")]
    public int CollisionLayer = 5; // Default: Terrain
    [Tooltip("Layers this chain collides with (comma-separated indices)")]
    public int[] CollidesWithLayers = new int[] { 0, 7 }; // Default,Debris
  }
}