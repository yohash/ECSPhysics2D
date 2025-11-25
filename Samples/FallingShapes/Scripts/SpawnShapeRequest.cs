using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Request component to spawn a shape at a specific location.
  /// Consumed by ShapeSpawningSystem.
  /// </summary>
  public struct SpawnShapeRequest : IComponentData
  {
    public enum ShapeType : byte
    {
      Random,
      Circle,
      Box,
      Capsule,
      Polygon
    }

    public float2 SpawnPosition;
    public ShapeType Type;
    public float Size;  // 0 = use random from config

    public static SpawnShapeRequest CreateRandom(float2 position, float size)
    {
      return new SpawnShapeRequest
      {
        SpawnPosition = position,
        Type = ShapeType.Random,
        Size = size
      };
    }

    public static SpawnShapeRequest Create(float2 position, ShapeType type, float size = 0f)
    {
      return new SpawnShapeRequest
      {
        SpawnPosition = position,
        Type = type,
        Size = size
      };
    }
  }
}
