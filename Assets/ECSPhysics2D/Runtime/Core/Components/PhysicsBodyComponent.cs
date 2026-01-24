using Unity.Entities;
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
    public int WorldIndex; // For future multi-world support
    // Enable Continuous Collision Detection (prevents tunneling)
    public bool EnableCCD;

    public bool IsValid => Body.isValid;
  }
}