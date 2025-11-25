using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Damping coefficients reduce velocity over time.
  /// Simulates air resistance and rotational friction.
  /// </summary>
  public struct PhysicsDamping : IComponentData
  {
    public float Linear;
    public float Angular;

    public static PhysicsDamping Default => new PhysicsDamping
    {
      Linear = 0f,
      Angular = 0f
    };
  }
}