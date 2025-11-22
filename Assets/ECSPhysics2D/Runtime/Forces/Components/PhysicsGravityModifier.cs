using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Modifies how gravity affects this specific body.
  /// Already defined in Phase 1, enhanced here.
  /// </summary>
  public struct PhysicsGravityModifier : IComponentData
  {
    public float Scale;            // Multiplier for world gravity (0 = float, 2 = double gravity)
    public float2 CustomGravity;   // Override world gravity with custom direction/magnitude
    public bool UseCustom;         // If true, use CustomGravity instead of world gravity

    public static PhysicsGravityModifier CreateScale(float scale)
    {
      return new PhysicsGravityModifier
      {
        Scale = scale,
        CustomGravity = float2.zero,
        UseCustom = false
      };
    }

    public static PhysicsGravityModifier CreateCustom(float2 gravity)
    {
      return new PhysicsGravityModifier
      {
        Scale = 1f,
        CustomGravity = gravity,
        UseCustom = true
      };
    }
  }

}