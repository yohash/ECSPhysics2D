using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Buffer for overlap query results.
  /// </summary>
  [InternalBufferCapacity(16)]
  public struct OverlapResult : IBufferElementData
  {
    public PhysicsBody Body;
    public PhysicsShape Shape;
    public Entity Entity;
  }
}