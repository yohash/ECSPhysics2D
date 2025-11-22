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
  }
}