using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Buffer element for tracking joints on a body.
  /// Optional - for fast body-to-joint queries.
  /// </summary>
  [InternalBufferCapacity(4)]
  public struct JointReference : IBufferElementData
  {
    public Entity JointEntity;
    public bool IsBodyA;            // Is this body A or B in the joint?
  }
}