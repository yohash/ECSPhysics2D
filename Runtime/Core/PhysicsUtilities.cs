using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
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
    public static ComponentType GetBodyTypeTag(RigidbodyType2D bodyType)
    {
      return bodyType switch
      {
        RigidbodyType2D.Dynamic => ComponentType.ReadOnly<PhysicsDynamicTag>(),
        RigidbodyType2D.Kinematic => ComponentType.ReadOnly<PhysicsKinematicTag>(),
        RigidbodyType2D.Static => ComponentType.ReadOnly<PhysicsStaticTag>(),
        _ => ComponentType.ReadOnly<PhysicsStaticTag>()
      };
    }
  }
}