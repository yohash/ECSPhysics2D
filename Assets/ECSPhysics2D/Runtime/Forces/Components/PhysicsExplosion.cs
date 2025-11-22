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
    public float Force;            // Force at center (falls off with distance)
    public ExplosionFalloff Falloff;
    public PhysicsMask AffectedLayers; // Which collision layers are affected

    public enum ExplosionFalloff : byte
    {
      Linear,      // Force = maxForce * (1 - distance/radius)
      Quadratic,   // Force = maxForce * (1 - distance/radius)²
      Constant     // Force = maxForce everywhere in radius
    }

    public static PhysicsExplosion Create(float2 center, float radius, float force)
    {
      return new PhysicsExplosion
      {
        Center = center,
        Radius = radius,
        Force = force,
        Falloff = ExplosionFalloff.Linear,
        AffectedLayers = ~0u // Affects all layers by default
      };
    }
  }

}