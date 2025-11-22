using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Results from a raycast query.
  /// </summary>
  public struct RaycastResult : IComponentData
  {
    public bool Hit;
    public float2 Point;
    public float2 Normal;
    public float Distance;
    public PhysicsBody Body;
    public PhysicsShape Shape;
    public Entity HitEntity;       // Retrieved from body userData
  }
}