using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request component to create a physics body.
  /// Removed after body creation.
  /// </summary>
  public struct PhysicsBodyCreateRequest : IComponentData
  {
    public RigidbodyType2D BodyType;
    public float2 InitialPosition;
    public float InitialRotation;
  }
}