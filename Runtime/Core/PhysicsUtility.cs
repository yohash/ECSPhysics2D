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
      // For 2D rotation around Z axis (expects radians)
      var physicsRotate = new PhysicsRotate(rotation);
      return physicsRotate;
    }

    /// <summary>
    /// Extracts Z-axis rotation from a quaternion.
    /// Assumes rotation is primarily around Z (standard for 2D).
    /// </summary>
    public static PhysicsRotate GetRotationZ(quaternion rotation)
    {
      // For 2D rotation around Z axis (PhysicsRotate expects radians)
      float3 euler = math.Euler(rotation);  // Returns radians

      var physicsRotate = new PhysicsRotate(euler.z);
      return physicsRotate;
    }

    public static PhysicsTransform PhysicsTransform(float2 position)
    {
      return new PhysicsTransform
      {
        position = position,
        rotation = GetRotationZ(0)
      };
    }

    public static PhysicsTransform PhysicsTransform(float2 position, float rotation)
    {
      return new PhysicsTransform
      {
        position = position,
        rotation = GetRotationZ(rotation)
      };
    }

    public static void SetEntityUserData(this PhysicsBody body, Entity entity)
    {
      var userData = body.userData;
      userData.int64Value = ((ulong)(uint)entity.Version << 32) | (uint)entity.Index;
      body.userData = userData;
    }

    public static void SetEntityUserData(this PhysicsJoint joint, Entity entity)
    {
      var userData = joint.userData;
      userData.int64Value = ((ulong)(uint)entity.Version << 32) | (uint)entity.Index;
      joint.userData = userData;
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