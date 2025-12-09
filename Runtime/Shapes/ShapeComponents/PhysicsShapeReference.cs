using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Buffer element tracking PhysicsShape handles attached to a body.
  /// Enables runtime modification of shape properties (collision filters, materials).
  /// Capacity of 4 covers most use cases - simple bodies (1 shape) and 
  /// moderately complex compound bodies (2-4 shapes).
  /// </summary>
  [InternalBufferCapacity(4)]
  public struct PhysicsShapeReference : IBufferElementData
  {
    public PhysicsShape Shape;
  }
}