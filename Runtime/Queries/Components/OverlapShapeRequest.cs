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
  }
}