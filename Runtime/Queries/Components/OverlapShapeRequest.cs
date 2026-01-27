using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request for shape overlap query.
  /// </summary>
  public struct OverlapShapeRequest : IComponentData
  {
    public enum ShapeType : byte
    {
      Circle,
      Box,
      Capsule
    }

    public ShapeType Type;
    public float2 Position;
    public float Rotation;
    // Circle: x=radius, Box: xy=size, Capsule: x=height, y=radius
    public float2 Size;
    public PhysicsQuery.QueryFilter Filter;
    public Entity ResultEntity;
    public int WorldIndex;

    public static OverlapShapeRequest CreateCircle(
      float2 position,
      float radius,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new OverlapShapeRequest
      {
        Type = ShapeType.Circle,
        Position = position,
        Rotation = 0f,
        Size = new float2(radius, 0f),
        Filter = PhysicsQuery.QueryFilter.defaultFilter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }

    public static OverlapShapeRequest CreateBox(
      float2 position,
      float2 size,
      float rotation,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new OverlapShapeRequest
      {
        Type = ShapeType.Box,
        Position = position,
        Rotation = rotation,
        Size = size,
        Filter = PhysicsQuery.QueryFilter.defaultFilter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }

    public static OverlapShapeRequest CreateCapsule(
      float2 position,
      float height,
      float radius,
      float rotation,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new OverlapShapeRequest
      {
        Type = ShapeType.Capsule,
        Position = position,
        Rotation = rotation,
        Size = new float2(height, radius),
        Filter = PhysicsQuery.QueryFilter.defaultFilter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }
  }
}
