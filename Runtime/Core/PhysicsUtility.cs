using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  public static class PhysicsUtility
  {
    /// <summary>
    /// Extracts Z-axis rotation from a quaternion.
    /// Assumes rotation is primarily around Z (standard for 2D).
    /// </summary>
    public static PhysicsRotate GetRotationZ(float rotation)
    {
      // For 2D rotation around Z axis
      float3 euler = math.degrees(rotation);

      var physicsRotate = new PhysicsRotate(euler.z);
      return physicsRotate;
    }

    /// <summary>
    /// Extracts Z-axis rotation from a quaternion.
    /// Assumes rotation is primarily around Z (standard for 2D).
    /// </summary>
    public static PhysicsRotate GetRotationZ(quaternion rotation)
    {
      // For 2D rotation around Z axis
      float3 euler = math.degrees(math.Euler(rotation));

      var physicsRotate = new PhysicsRotate(euler.z);
      return physicsRotate;
    }

    public static void SetEntityUserData(this PhysicsBody body, Entity entity)
    {
      var userData = body.userData;
      userData.int64Value = ((ulong)(uint)entity.Version << 32) | (uint)entity.Index;
      body.userData = userData;
    }

    public static Entity GetEntityUserData(this PhysicsBody body)
    {
      if (!body.isValid)
        return Entity.Null;

      ulong packed = body.userData.int64Value;
      var entity = new Entity
      {
        Index = (int)(packed & 0xFFFFFFFF),
        Version = (int)(packed >> 32)
      };

      return entity;
      // TBD - verify entity still exists?
      //return em.Exists(entity) ? entity : Entity.Null;
    }

    /// <summary>
    /// Creates a quaternion from Z-axis rotation.
    /// </summary>
    public static quaternion CreateRotationZ(PhysicsRotate rotation)
    {
      var degrees = rotation.angle;
      var radians = math.radians(degrees);
      return quaternion.RotateZ(radians);
    }

    /// <summary>
    /// Converts PhysicsBodyType enum to appropriate component types.
    /// </summary>
    public static ComponentType GetBodyTypeTag(PhysicsBody.BodyType bodyType)
    {
      return bodyType switch
      {
        PhysicsBody.BodyType.Dynamic => ComponentType.ReadOnly<PhysicsDynamicTag>(),
        PhysicsBody.BodyType.Kinematic => ComponentType.ReadOnly<PhysicsKinematicTag>(),
        PhysicsBody.BodyType.Static => ComponentType.ReadOnly<PhysicsStaticTag>(),
        _ => ComponentType.ReadOnly<PhysicsStaticTag>()
      };
    }
  }
}