using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// One-shot radial explosion impulse, applied via the native Box2D Explode() method.
  /// Every shape within Radius receives the same impulse magnitude (impulse per unit length
  /// = Force / Radius, scaled by the shape's projected perimeter); there is no distance falloff
  /// inside the radius. With Falloff > 0, the impulse fades linearly to zero over a band
  /// extending Falloff beyond the radius, so the total reach is Radius + Falloff.
  /// Note that for a fixed Force, a larger Radius gives each shape a weaker impulse.
  /// </summary>
  public struct PhysicsExplosion : IComponentData
  {
    public float2 Center;              // World position of explosion center
    public float Radius;               // Full-strength radius (total reach is Radius + Falloff)
    public float Force;                // Impulse strength; impulse per unit length = Force / Radius
    public float Falloff;              // Impulse linear falloff distance beyond radius
    public PhysicsMask AffectedLayers; // Which collision layers are affected
    public int WorldIndex;             // Which physics world to apply explosion in (default 0)

    /// <summary>
    /// Explosion with no falloff, affecting all layers.
    /// </summary>
    /// <param name="center">World position of the explosion center.</param>
    /// <param name="radius">Radius within which bodies receive the full impulse.</param>
    /// <param name="force">Explosion strength; converted to impulse per unit length as force / radius.</param>
    /// <param name="worldIndex">Physics world to apply the explosion in.</param>
    public static PhysicsExplosion Create(float2 center, float radius, float force, int worldIndex = 0)
    {
      return new PhysicsExplosion
      {
        Center = center,
        Radius = radius,
        Force = force,
        AffectedLayers = ~0u, // Affects all layers by default
        Falloff = 0f,
        WorldIndex = worldIndex
      };
    }

    /// <summary>
    /// Explosion with a linear falloff band beyond the radius, affecting all layers.
    /// Named distinctly from Create so that force and falloff (both floats) cannot be
    /// silently swapped by overload resolution.
    /// </summary>
    /// <param name="center">World position of the explosion center.</param>
    /// <param name="radius">Radius within which bodies receive the full impulse.</param>
    /// <param name="force">Explosion strength; converted to impulse per unit length as force / radius.</param>
    /// <param name="falloff">Distance beyond the radius over which the impulse fades linearly to zero.</param>
    /// <param name="worldIndex">Physics world to apply the explosion in.</param>
    public static PhysicsExplosion CreateWithFalloff(float2 center, float radius, float force, float falloff, int worldIndex = 0)
    {
      return new PhysicsExplosion
      {
        Center = center,
        Radius = radius,
        Force = force,
        AffectedLayers = ~0u, // Affects all layers by default
        Falloff = falloff,
        WorldIndex = worldIndex
      };
    }

    /// <summary>
    /// Explosion with a linear falloff band beyond the radius, restricted to the given layers.
    /// </summary>
    /// <param name="center">World position of the explosion center.</param>
    /// <param name="radius">Radius within which bodies receive the full impulse.</param>
    /// <param name="force">Explosion strength; converted to impulse per unit length as force / radius.</param>
    /// <param name="falloff">Distance beyond the radius over which the impulse fades linearly to zero.</param>
    /// <param name="affectedLayers">Collision layers the explosion affects.</param>
    /// <param name="worldIndex">Physics world to apply the explosion in.</param>
    public static PhysicsExplosion Create(
        float2 center,
        float radius,
        float force,
        float falloff,
        PhysicsMask affectedLayers,
        int worldIndex = 0)
    {
      return new PhysicsExplosion
      {
        Center = center,
        Radius = radius,
        Force = force,
        Falloff = falloff,
        AffectedLayers = affectedLayers,
        WorldIndex = worldIndex
      };
    }
  }
}
