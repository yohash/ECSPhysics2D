using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Utility class for creating joints at runtime.
  /// </summary>
  public static class JointUtility
  {
    public static Entity CreateHingeJoint(EntityManager em, Entity bodyA, Entity bodyB,
        float2 anchor, float minAngle, float maxAngle)
    {
      var jointEntity = em.CreateEntity();

      em.AddComponentData(jointEntity, new PhysicsJointComponent
      {
        BodyA = bodyA,
        BodyB = bodyB,
        Type = PhysicsJointComponent.JointType.Revolute,
        CollideConnected = false
      });

      em.AddComponentData(jointEntity, HingeJoint.CreateDoor(anchor, minAngle, maxAngle));

      return jointEntity;
    }
  }
}