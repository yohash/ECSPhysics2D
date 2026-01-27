using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request for closest point query.
  /// </summary>
  public struct ClosestPointRequest : IComponentData
  {
    public float2 Point;
    public float MaxDistance;
    public PhysicsQuery.QueryFilter Filter;
    public Entity ResultEntity;
    public int WorldIndex;

    public static ClosestPointRequest Create(
      float2 point,
      float maxDistance,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new ClosestPointRequest
      {
        Point = point,
        MaxDistance = maxDistance,
        Filter = PhysicsQuery.QueryFilter.defaultFilter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }

    public static ClosestPointRequest Create(
      float2 point,
      float maxDistance,
      PhysicsQuery.QueryFilter filter,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new ClosestPointRequest
      {
        Point = point,
        MaxDistance = maxDistance,
        Filter = filter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }
  }
}
