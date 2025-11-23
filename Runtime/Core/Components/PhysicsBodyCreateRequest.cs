using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request component to create a physics body.
  /// Removed after body creation.
  /// </summary>
  public struct PhysicsBodyCreateRequest : IComponentData
  {
    public PhysicsBody.BodyType BodyType;
    public float2 InitialPosition;
    public float InitialRotation;
  }
}