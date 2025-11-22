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
  }
}