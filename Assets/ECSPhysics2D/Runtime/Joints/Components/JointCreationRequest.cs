using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request to create a joint between bodies.
  /// </summary>
  public struct JointCreationRequest : IComponentData
  {
    public Entity BodyA;
    public Entity BodyB;
    public PhysicsJointComponent.JointType Type;
  }
}