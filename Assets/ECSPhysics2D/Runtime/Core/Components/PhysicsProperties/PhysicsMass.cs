using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Mass properties for dynamic bodies.
  /// Using inverse values for efficient force calculations.
  /// </summary>
  public struct PhysicsMass : IComponentData
  {
    public float InverseMass;
    public float InverseInertia;
    public float2 CenterOfMass;

    public float Mass => InverseMass > 0f ? 1f / InverseMass : 0f;
    public float Inertia => InverseInertia > 0f ? 1f / InverseInertia : 0f;

    public static PhysicsMass CreateDefault(float mass = 1f)
    {
      return new PhysicsMass
      {
        InverseMass = 1f / mass,
        InverseInertia = 1f / mass, // Simplified - real inertia depends on shape
        CenterOfMass = float2.zero
      };
    }
  }
}