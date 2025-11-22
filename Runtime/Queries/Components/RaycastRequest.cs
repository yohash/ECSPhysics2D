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
    public Entity ResultEntity;    // Entity to store results on
  }
}