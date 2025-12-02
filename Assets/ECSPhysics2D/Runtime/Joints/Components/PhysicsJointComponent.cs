using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Core joint component that links two bodies together.
  /// Joints are separate entities for maximum flexibility.
  /// </summary>
  public struct PhysicsJointComponent : IComponentData
  {
    public PhysicsJoint Joint;      // Handle to the Box2D joint
    public Entity BodyA;            // First connected body entity
    public Entity BodyB;            // Second connected body entity
    public JointType Type;          // Type of joint for queries
    public bool CollideConnected;   // Should connected bodies collide?

    public enum JointType : byte
    {
      Distance,
      Revolute,
      Prismatic,
      Wheel,
      Weld,
      Motor,
      Mouse
    }
  }
}