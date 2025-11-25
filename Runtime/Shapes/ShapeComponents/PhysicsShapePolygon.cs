using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component for arbitrary convex polygon collision geometry.
  /// Maximum 8 vertices in Box2D v3 (was 8 in v2 as well).
  /// Vertices must be in counter-clockwise order.
  /// </summary>
  public struct PhysicsShapePolygon : IComponentData
  {
    // Fixed-size array for Burst compatibility
    // Using float2x4 gives us 8 float2 values (2 columns, 4 rows each)
    public float2x4 Vertices0to3;
    public float2x4 Vertices4to7;
    public int VertexCount;

    public void SetVertex(int index, float2 vertex)
    {
      if (index < 4) {
        switch (index) {
          case 0:
            Vertices0to3.c0 = vertex;
            break;
          case 1:
            Vertices0to3.c1 = vertex;
            break;
          case 2:
            Vertices0to3.c2 = vertex;
            break;
          case 3:
            Vertices0to3.c3 = vertex;
            break;
        }
      } else if (index < 8) {
        switch (index - 4) {
          case 0:
            Vertices4to7.c0 = vertex;
            break;
          case 1:
            Vertices4to7.c1 = vertex;
            break;
          case 2:
            Vertices4to7.c2 = vertex;
            break;
          case 3:
            Vertices4to7.c3 = vertex;
            break;
        }
      }
    }

    public float2 GetVertex(int index)
    {
      if (index < 4) {
        return index switch
        {
          0 => Vertices0to3.c0,
          1 => Vertices0to3.c1,
          2 => Vertices0to3.c2,
          3 => Vertices0to3.c3,
          _ => float2.zero
        };
      } else if (index < 8) {
        return (index - 4) switch
        {
          0 => Vertices4to7.c0,
          1 => Vertices4to7.c1,
          2 => Vertices4to7.c2,
          3 => Vertices4to7.c3,
          _ => float2.zero
        };
      }
      return float2.zero;
    }

    /// <summary>
    /// Creates a regular polygon (triangle, pentagon, hexagon, etc.)
    /// </summary>
    public static PhysicsShapePolygon CreateRegular(int sides, float radius)
    {
      sides = math.clamp(sides, 3, 8);
      var polygon = new PhysicsShapePolygon { VertexCount = sides };

      float angleStep = 2f * math.PI / sides;
      for (int i = 0; i < sides; i++) {
        float angle = i * angleStep;
        polygon.SetVertex(i, new float2(
            radius * math.cos(angle),
            radius * math.sin(angle)
        ));
      }

      return polygon;
    }

    public PhysicsShapePolygon Rotate(float rotation)
    {
      var cos = math.cos(rotation);
      var sin = math.sin(rotation);
      PhysicsShapePolygon rotated = this;
      for (int i = 0; i < VertexCount; i++) {
        var v = GetVertex(i);
        rotated.SetVertex(i, new float2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        ));
      }
      return rotated;
    }

    public static PhysicsShapePolygon FromBox(PhysicsShapeBox box)
    {
      var halfSize = box.HalfSize;
      var cos = math.cos(box.Rotation);
      var sin = math.sin(box.Rotation);
      float2 Rotate(float2 point)
      {
        return new float2(
            point.x * cos - point.y * sin,
            point.x * sin + point.y * cos
        );
      }
      var polygon = new PhysicsShapePolygon { VertexCount = 4 };
      polygon.SetVertex(0, Rotate(new float2(-halfSize.x, -halfSize.y)) + box.Center);
      polygon.SetVertex(1, Rotate(new float2(halfSize.x, -halfSize.y)) + box.Center);
      polygon.SetVertex(2, Rotate(new float2(halfSize.x, halfSize.y)) + box.Center);
      polygon.SetVertex(3, Rotate(new float2(-halfSize.x, halfSize.y)) + box.Center);
      return polygon;
    }
  }
}