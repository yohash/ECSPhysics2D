using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag for dynamic bodies - fully simulated by physics.
  /// Physics drives transform (Physics -> ECS).
  /// </summary>
  public struct PhysicsDynamicTag : IComponentData { }
}