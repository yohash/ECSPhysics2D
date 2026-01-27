using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request component for raycasts.
  /// </summary>
  public struct RaycastRequest : IComponentData
  {
    public float2 Origin;
    public float2 Direction;
    public float MaxDistance;
    public PhysicsQuery.QueryFilter Filter;
    // Entity to store results on
    public Entity ResultEntity;
    public int WorldIndex;

    public static RaycastRequest Create(
      float2 origin,
      float2 direction,
      float maxDistance,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new RaycastRequest
      {
        Origin = origin,
        Direction = direction,
        MaxDistance = maxDistance,
        Filter = PhysicsQuery.QueryFilter.defaultFilter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }

    public static RaycastRequest Create(
      float2 origin,
      float2 direction,
      float maxDistance,
      PhysicsQuery.QueryFilter filter,
      Entity resultEntity,
      int worldIndex = 0)
    {
      return new RaycastRequest
      {
        Origin = origin,
        Direction = direction,
        MaxDistance = maxDistance,
        Filter = filter,
        ResultEntity = resultEntity,
        WorldIndex = worldIndex
      };
    }
  }
}
