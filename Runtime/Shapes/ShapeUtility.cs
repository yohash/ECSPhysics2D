using Unity.Mathematics;
using Unity.Collections;

namespace ECSPhysics2D
{
  /// <summary>
  /// Shape utility methods for validation and conversion.
  /// </summary>
  public static class ShapeUtility
  {
    /// <summary>
    /// Validates that polygon vertices are convex and CCW.
    /// </summary>
    public static bool ValidatePolygon(NativeArray<float2> vertices)
    {
      if (vertices.Length < 3 || vertices.Length > 8)
        return false;

      // Check for CCW winding using cross product
      float area = 0f;
      for (int i = 0; i < vertices.Length; i++) {
        var v1 = vertices[i];
        var v2 = vertices[(i + 1) % vertices.Length];
        area += v1.x * v2.y - v2.x * v1.y;
      }

      if (area <= 0f) // Clockwise or degenerate
        return false;

      // Check convexity
      for (int i = 0; i < vertices.Length; i++) {
        var v0 = vertices[i];
        var v1 = vertices[(i + 1) % vertices.Length];
        var v2 = vertices[(i + 2) % vertices.Length];

        var edge1 = v1 - v0;
        var edge2 = v2 - v1;
        var cross = edge1.x * edge2.y - edge1.y * edge2.x;

        if (cross <= 0f) // Not convex
          return false;
      }

      return true;
    }

    /// <summary>
    /// Ensures vertices are in CCW order, reverses if needed.
    /// </summary>
    public static void EnsureCCW(NativeArray<float2> vertices)
    {
      float area = 0f;
      for (int i = 0; i < vertices.Length; i++) {
        var v1 = vertices[i];
        var v2 = vertices[(i + 1) % vertices.Length];
        area += v1.x * v2.y - v2.x * v1.y;
      }

      if (area < 0f) // Clockwise, need to reverse
      {
        for (int i = 0; i < vertices.Length / 2; i++) {
          var temp = vertices[i];
          vertices[i] = vertices[vertices.Length - 1 - i];
          vertices[vertices.Length - 1 - i] = temp;
        }
      }
    }

    /// <summary>
    /// Computes the centroid of a polygon.
    /// </summary>
    public static float2 ComputeCentroid(NativeArray<float2> vertices)
    {
      float2 centroid = float2.zero;
      float area = 0f;

      for (int i = 0; i < vertices.Length; i++) {
        var v1 = vertices[i];
        var v2 = vertices[(i + 1) % vertices.Length];
        float a = v1.x * v2.y - v2.x * v1.y;
        area += a;
        centroid += (v1 + v2) * a;
      }

      return centroid / (3f * area);
    }
  }
}