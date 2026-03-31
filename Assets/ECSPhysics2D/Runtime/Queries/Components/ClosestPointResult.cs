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
    // Outward surface normal at the closest point. Derived analytically from PhysicsShape geometry
    // (shapeType + geometry accessors). Zero when the query point lies exactly on the surface.
    public float2 Normal;
    public float Distance;
    public PhysicsBody Body;
    public PhysicsShape Shape;
    public Entity Entity;
  }
}