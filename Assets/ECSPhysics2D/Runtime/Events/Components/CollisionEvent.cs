using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Collision event data stored in singleton stream.
  /// Contains all information about a contact between two bodies.
  /// </summary>
  public struct CollisionEvent
  {
    // Entity references (retrieved from PhysicsBody.userData)
    public Entity EntityA;
    public Entity EntityB;

    // Physics handles (for advanced queries)
    public PhysicsBody BodyA;
    public PhysicsBody BodyB;
    public PhysicsShape ShapeA;
    public PhysicsShape ShapeB;

    // Contact geometry
    public float2 ContactPoint;     // World-space contact location
    public float2 ContactNormal;    // Normal pointing from A to B

    // Contact dynamics
    public float NormalImpulse;     // Impact force magnitude
    public float TangentImpulse;    // Friction force magnitude

    // Event metadata
    public CollisionEventType EventType;

    /// <summary>
    /// Check if this event involves a specific entity.
    /// </summary>
    public bool Involves(Entity entity)
    {
      return EntityA == entity || EntityB == entity;
    }

    /// <summary>
    /// Get the other entity in this collision pair.
    /// Returns Entity.Null if the provided entity is not involved.
    /// </summary>
    public Entity GetOther(Entity entity)
    {
      if (EntityA == entity)
        return EntityB;
      if (EntityB == entity)
        return EntityA;
      return Entity.Null;
    }

    /// <summary>
    /// Get contact normal relative to the specified entity.
    /// Normal will point away from the entity.
    /// </summary>
    public float2 GetNormalRelativeTo(Entity entity)
    {
      if (EntityA == entity)
        return ContactNormal;
      if (EntityB == entity)
        return -ContactNormal;
      return float2.zero;
    }
  }
}