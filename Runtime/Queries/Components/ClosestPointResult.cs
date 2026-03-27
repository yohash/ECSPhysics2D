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
    // PhysicsShape has no native normal API. Normal is computed via OverlapPoint inside/outside
    // detection: outward from the surface toward the exterior, zero if query point is on the surface.
    public float2 Normal;
    public float Distance;
    public PhysicsBody Body;
    public PhysicsShape Shape;
    public Entity Entity;
  }
}