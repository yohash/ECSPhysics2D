using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Radial explosion force with distance falloff.
  /// Triggers the native Box2D Explode() method for optimal performance.
  /// </summary>
  public struct PhysicsExplosion : IComponentData
  {
    public float2 Center;          // World position of explosion center
    public float Radius;           // Maximum effect radius
    public float Force;            // Force from center to radius
    public float Falloff;          // Force linear falloff distance beyond radius
    public PhysicsMask AffectedLayers; // Which collision layers are affected

    public static PhysicsExplosion Create(float2 center, float radius, float force)
    {
      return new PhysicsExplosion
      {
        Center = center,
        Radius = radius,
        Force = force,
        Falloff = 0f, // No falloff by default
        AffectedLayers = ~0u // Affects all layers by default
      };
    }

    public static PhysicsExplosion Create(float2 center, float radius, float falloff, float force)
    {
      return new PhysicsExplosion
      {
        Center = center,
        Radius = radius,
        Force = force,
        Falloff = falloff,
        AffectedLayers = ~0u // Affects all layers by default
      };
    }
  }
}