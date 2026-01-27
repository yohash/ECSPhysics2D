using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request for AABB (Axis-Aligned Bounding Box) query.
  /// </summary>
  public struct AABBQueryRequest : IComponentData
  {
    public float2 Min;
    public float2 Max;
    public PhysicsQuery.QueryFilter Filter;
    public Entity ResultEntity;
    public int WorldIndex;

    public static AABBQueryRequest Create(
      float2 min,
      float2 max,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new AABBQueryRequest
      {
        Min = min,
        Max = max,
        Filter = PhysicsQuery.QueryFilter.defaultFilter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }

    public static AABBQueryRequest Create(
      float2 min,
      float2 max,
      PhysicsQuery.QueryFilter filter,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new AABBQueryRequest
      {
        Min = min,
        Max = max,
        Filter = filter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }

    public static AABBQueryRequest CreateFromCenter(
      float2 center,
      float2 halfExtents,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new AABBQueryRequest
      {
        Min = center - halfExtents,
        Max = center + halfExtents,
        Filter = PhysicsQuery.QueryFilter.defaultFilter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }
  }
}
