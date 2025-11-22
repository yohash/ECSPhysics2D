using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Zone-based force field that affects all bodies within.
  /// Used with trigger volumes for area effects.
  /// </summary>
  public struct PhysicsForceField : IComponentData
  {
    public enum FieldType : byte
    {
      Directional,    // Constant direction (wind)
      Radial,         // Push away from/pull toward center
      Vortex,         // Spiral/tornado effect
      Damping,        // Slow down zone
      Custom          // User-defined force function
    }

    public FieldType Type;
    public float2 Direction;       // For directional fields
    public float2 Center;          // For radial/vortex fields
    public float Strength;         // Force magnitude
    public float InnerRadius;      // No effect inside this radius
    public float OuterRadius;      // No effect outside this radius
    public float VortexAngle;      // Spiral angle for vortex
    public PhysicsMask AffectedLayers;

    public bool InRange(float2 position)
    {
      float distSq = math.lengthsq(position - Center);
      float innerSq = InnerRadius * InnerRadius;
      float outerSq = OuterRadius * OuterRadius;
      return distSq >= innerSq && distSq <= outerSq;
    }

    public float2 CalculateForce(float2 position)
    {
      if (!InRange(position))
        return float2.zero;

      switch (Type) {
        case FieldType.Directional:
          return Direction * Strength;

        case FieldType.Radial:
          var toCenter = Center - position;
          var distance = math.length(toCenter);
          if (distance < 0.001f)
            return float2.zero;
          return math.normalize(toCenter) * Strength;

        case FieldType.Vortex:
          var radialDir = position - Center;
          var dist = math.length(radialDir);
          if (dist < 0.001f)
            return float2.zero;

          // Combine radial and tangential forces
          var normalized = radialDir / dist;
          var tangent = new float2(-normalized.y, normalized.x);

          // Vortex combines inward pull with rotation
          var radialForce = -normalized * Strength * math.cos(VortexAngle);
          var tangentForce = tangent * Strength * math.sin(VortexAngle);

          return radialForce + tangentForce;

        case FieldType.Damping:
          // Damping is handled separately, return zero force
          return float2.zero;

        default:
          return float2.zero;
      }
    }

    public static PhysicsForceField CreateWind(float2 direction, float strength)
    {
      return new PhysicsForceField
      {
        Type = FieldType.Directional,
        Direction = math.normalize(direction),
        Strength = strength,
        InnerRadius = 0f,
        OuterRadius = float.MaxValue,
        AffectedLayers = ~0u
      };
    }

    public static PhysicsForceField CreateRadial(float2 center, float strength, float radius)
    {
      return new PhysicsForceField
      {
        Type = FieldType.Radial,
        Center = center,
        Strength = strength,
        InnerRadius = 0f,
        OuterRadius = radius,
        AffectedLayers = ~0u
      };
    }

    public static PhysicsForceField CreateVortex(float2 center, float strength, float radius, float angle = math.PI / 4f)
    {
      return new PhysicsForceField
      {
        Type = FieldType.Vortex,
        Center = center,
        Strength = strength,
        InnerRadius = 0f,
        OuterRadius = radius,
        VortexAngle = angle,
        AffectedLayers = ~0u
      };
    }
  }
}