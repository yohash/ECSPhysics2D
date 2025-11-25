using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag for kinematic bodies - controlled by game logic but affects dynamics.
  /// Game logic drives transform (ECS -> Physics).
  /// </summary>
  public struct PhysicsKinematicTag : IComponentData { }
}