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
    public float2 Center;              // World position of explosion center
    public float Radius;               // Maximum effect radius
    public float Force;                // Force from center to radius
    public float Falloff;              // Force linear falloff distance beyond radius
    public PhysicsMask AffectedLayers; // Which collision layers are affected
    public int WorldIndex;             // Which physics world to apply explosion in (default 0)

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

    public static PhysicsExplosion Create(float2 center, float radius, float falloff, float force, int worldIndex = 0)
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
