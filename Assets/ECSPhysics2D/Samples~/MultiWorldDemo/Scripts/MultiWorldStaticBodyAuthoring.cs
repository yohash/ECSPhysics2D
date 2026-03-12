using Unity.Mathematics;
using UnityEngine;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Authoring component for static bodies that exist in multiple physics worlds.
  /// Use checkboxes to specify which worlds should contain this body.
  /// </summary>
  public class MultiWorldStaticBodyAuthoring : MonoBehaviour
  {
    [Header("World Presence")]
    [Tooltip("Body exists in World 0 (main gameplay)")]
    public bool ExistsInWorld0 = true;

    [Tooltip("Body exists in World 1 (debris)")]
    public bool ExistsInWorld1 = true;

    [Tooltip("Body exists in World 2")]
    public bool ExistsInWorld2 = false;

    [Tooltip("Body exists in World 3")]
    public bool ExistsInWorld3 = false;

    [Header("Shape")]
    public ShapeType Shape = ShapeType.Box;

    [Tooltip("Size for box shape")]
    public Vector2 BoxSize = new Vector2(4f, 0.5f);

    [Tooltip("Radius for circle shape")]
    public float CircleRadius = 1f;

    public enum ShapeType
    {
      Box,
      Circle
    }

    public byte GetWorldMask()
    {
      byte mask = 0;
      if (ExistsInWorld0) mask |= 1 << 0;
      if (ExistsInWorld1) mask |= 1 << 1;
      if (ExistsInWorld2) mask |= 1 << 2;
      if (ExistsInWorld3) mask |= 1 << 3;
      return mask;
    }

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);

      switch (Shape) {
        case ShapeType.Box:
          Gizmos.matrix = transform.localToWorldMatrix;
          Gizmos.DrawWireCube(Vector3.zero, new Vector3(BoxSize.x, BoxSize.y, 0.1f));
          break;

        case ShapeType.Circle:
          DrawCircleGizmo(transform.position, CircleRadius);
          break;
      }
    }

    private void DrawCircleGizmo(Vector3 center, float radius)
    {
      int segments = 32;
      float angleStep = 360f / segments;

      for (int i = 0; i < segments; i++) {
        float angle1 = i * angleStep * Mathf.Deg2Rad;
        float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

        Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0f) * radius;
        Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0f) * radius;

        Gizmos.DrawLine(p1, p2);
      }
    }
  }
}
