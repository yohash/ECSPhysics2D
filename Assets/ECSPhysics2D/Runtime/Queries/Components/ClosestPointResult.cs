using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Result from closest point query.
  /// </summary>
  public struct ClosestPointResult : IComponentData
  {
    public bool Found;
    public float2 ClosestPoint;
    public float Distance;
    public PhysicsBody Body;
    public PhysicsShape Shape;
    public Entity Entity;
  }
}