using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component that links an ECS Entity to a Box2D PhysicsBody.
  /// This is the core bridge between ECS and the physics engine.
  /// </summary>
  public struct PhysicsBodyComponent : IComponentData
  {
    public PhysicsBody Body;

    // Multi-world support: index of the physics world this body belongs to
    public int WorldIndex;

    // Damping coefficients
    public float LinearDamping;
    public float AngularDamping;

    // Initial velocities applied on body creation
    public float2 InitialLinearVelocity;
    public float InitialAngularVelocity;

    // Multiplier for gravity effect on this body
    public float GravityScale;

    // Enable Continuous Collision Detection (prevents tunneling)
    public bool EnableCCD;

    public bool IsValid => Body.isValid;
  }
}