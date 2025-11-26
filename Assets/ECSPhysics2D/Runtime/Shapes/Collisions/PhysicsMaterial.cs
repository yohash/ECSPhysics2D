using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Physics material properties that affect collision response.
  /// Can be applied per-shape for fine control.
  /// </summary>
  public struct PhysicsMaterial : IComponentData
  {
    public float Friction;         // 0 = ice, 1 = rubber
    public float Bounciness;       // 0 = no bounce, 1 = perfect bounce
    public float Density;          // kg/m² - affects mass calculation
    public float RollingResistance; // NEW in Box2D v3 - simulates rolling friction

    /// <summary>
    /// Default material similar to wood/plastic
    /// </summary>
    public static PhysicsMaterial Default => new PhysicsMaterial
    {
      Friction = 0.4f,
      Bounciness = 0.2f,
      Density = 1.0f,
      RollingResistance = 0f
    };

    /// <summary>
    /// Ice-like surface - very slippery
    /// </summary>
    public static PhysicsMaterial Ice => new PhysicsMaterial
    {
      Friction = 0.02f,
      Bounciness = 0.1f,
      Density = 0.92f,
      RollingResistance = 0f
    };

    /// <summary>
    /// Rubber material - high friction and bounce
    /// </summary>
    public static PhysicsMaterial Rubber => new PhysicsMaterial
    {
      Friction = 0.9f,
      Bounciness = 0.8f,
      Density = 1.5f,
      RollingResistance = 0.01f
    };

    /// <summary>
    /// Metal material - medium friction, no bounce
    /// </summary>
    public static PhysicsMaterial Metal => new PhysicsMaterial
    {
      Friction = 0.5f,
      Bounciness = 0.05f,
      Density = 7.8f,
      RollingResistance = 0.002f
    };

    /// <summary>
    /// Combines two materials using Box2D mixing rules
    /// </summary>
    public static PhysicsMaterial Mix(PhysicsMaterial a, PhysicsMaterial b)
    {
      return new PhysicsMaterial
      {
        // Geometric mean for friction
        Friction = math.sqrt(a.Friction * b.Friction),
        // Maximum for bounciness
        Bounciness = math.max(a.Bounciness, b.Bounciness),
        // Average for density (though density doesn't mix in collisions)
        Density = (a.Density + b.Density) * 0.5f,
        // Average for rolling resistance
        RollingResistance = (a.RollingResistance + b.RollingResistance) * 0.5f
      };
    }
  }
}