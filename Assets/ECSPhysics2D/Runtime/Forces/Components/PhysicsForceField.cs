using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Area force field (wind, radial well/push, vortex, damping zone) processed by
  /// <see cref="ForceFieldSystem"/> every fixed step.
  ///
  /// Semantics:
  /// - <see cref="Strength"/> is a force in newtons applied at each body's center of mass
  ///   (ApplyForceToCenter), so the response is mass-dependent: lighter bodies accelerate more.
  ///   For <see cref="FieldType.Damping"/> it is instead a damping rate in 1/s.
  /// - The field follows its entity: <see cref="Center"/> is overwritten each step from the
  ///   entity's LocalTransform, so an entity needs both this component and LocalTransform.
  /// - Only dynamic bodies whose collision categories intersect <see cref="AffectedLayers"/>
  ///   and that live in physics world <see cref="WorldIndex"/> are affected.
  /// - A field is bounded when <see cref="OuterRadius"/> is finite; bounded fields find bodies
  ///   with a Box2D broadphase query. Unbounded fields (e.g. <see cref="CreateWind"/>) check
  ///   every dynamic body and ignore <see cref="Falloff"/>.
  /// </summary>
  public struct PhysicsForceField : IComponentData
  {
    public enum FieldType : byte
    {
      Directional,    // Constant direction (wind)
      Radial,         // Positive strength pulls toward center, negative pushes away
      Vortex,         // Spiral/tornado effect
      Damping,        // Slow down zone
      Custom          // User-defined force function
    }

    /// <summary>
    /// Strength profile across the field, measured from <see cref="Center"/>.
    /// </summary>
    public enum FalloffMode : byte
    {
      /// <summary>Full strength everywhere in range (0.1.x/0.2.x behavior).</summary>
      None,
      /// <summary>Full strength at InnerRadius, fading linearly to zero at OuterRadius.</summary>
      Linear,
      /// <summary>
      /// Full strength within r0 = max(InnerRadius, MinDistance), then (r0 / d)².
      /// Hard edge at OuterRadius: the force drops from Strength * (r0 / OuterRadius)² to zero there.
      /// </summary>
      InverseSquare
    }

    /// <summary>
    /// MinDistance used when the field's MinDistance is zero or negative (e.g. a
    /// default-initialized struct), in world units. Only affects InverseSquare falloff.
    /// </summary>
    public const float DefaultMinDistance = 1f;

    /// <summary>
    /// Lower bound on a positive MinDistance, so a tiny value cannot blow up the falloff.
    /// </summary>
    public const float MinDistanceEpsilon = 1e-3f;

    public FieldType Type;
    public float2 Direction;       // For directional fields (normalized)
    public float2 Center;          // Overwritten each step from the entity's LocalTransform
    /// <summary>Force in newtons at the center of mass; for Damping, a rate in 1/s.</summary>
    public float Strength;
    public float InnerRadius;      // No effect inside this radius
    public float OuterRadius;      // No effect outside this radius; float.MaxValue = unbounded
    public float VortexAngle;      // Spiral angle for vortex
    /// <summary>
    /// Bodies are affected only if their collision categories intersect this mask. ~0u = all layers.
    /// </summary>
    public PhysicsMask AffectedLayers;
    public FalloffMode Falloff;
    /// <summary>
    /// InverseSquare clamp distance. Zero or negative uses DefaultMinDistance; positive values
    /// below MinDistanceEpsilon are raised to it.
    /// </summary>
    public float MinDistance;
    /// <summary>Physics world this field acts on. Invalid indices fall back to world 0.</summary>
    public int WorldIndex;
    /// <summary>
    /// If true, only bodies tagged with <see cref="AffectedByForceFields"/> are affected.
    /// </summary>
    public bool RequireOptInTag;

    /// <summary>
    /// True when OuterRadius is finite, so the field can use a broadphase circle query.
    /// </summary>
    public bool IsBounded => OuterRadius < float.MaxValue;

    public bool InRange(float2 position)
    {
      float distSq = math.lengthsq(position - Center);
      float innerSq = InnerRadius * InnerRadius;
      float outerSq = OuterRadius * OuterRadius;
      return distSq >= innerSq && distSq <= outerSq;
    }

    /// <summary>
    /// Strength multiplier in [0, 1] at the given distance from Center.
    /// </summary>
    public float FalloffScale(float distance)
    {
      if (!IsBounded)
        return 1f;

      switch (Falloff) {
        case FalloffMode.Linear: {
            var band = OuterRadius - InnerRadius;
            if (band <= 0f)
              return 1f;
            return 1f - math.saturate((distance - InnerRadius) / band);
          }

        case FalloffMode.InverseSquare: {
            var minDistance = MinDistance > 0f ? math.max(MinDistance, MinDistanceEpsilon) : DefaultMinDistance;
            var r0 = math.max(InnerRadius, minDistance);
            var ratio = r0 / math.max(distance, minDistance);
            return math.saturate(ratio * ratio);
          }

        default:
          return 1f;
      }
    }

    public float2 CalculateForce(float2 position)
    {
      if (!InRange(position))
        return float2.zero;

      switch (Type) {
        case FieldType.Directional:
          return Direction * Strength * FalloffScale(math.length(position - Center));

        case FieldType.Radial:
          var toCenter = Center - position;
          var distance = math.length(toCenter);
          if (distance < 0.001f)
            return float2.zero;
          return toCenter / distance * Strength * FalloffScale(distance);

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

          return (radialForce + tangentForce) * FalloffScale(dist);

        case FieldType.Damping:
          // Damping is handled separately, return zero force
          return float2.zero;

        default:
          return float2.zero;
      }
    }

    /// <summary>
    /// Velocity multiplier for a Damping field over one step: exp(-Strength * dt).
    /// Always in (0, 1] for non-negative Strength, so velocity never reverses.
    /// </summary>
    public float DampingFactor(float deltaTime)
    {
      return math.exp(-Strength * deltaTime);
    }

    /// <summary>
    /// Unbounded directional field affecting all layers. Falloff does not apply.
    /// </summary>
    public static PhysicsForceField CreateWind(float2 direction, float strength)
    {
      return CreateWind(direction, strength, ~0u);
    }

    /// <summary>
    /// Unbounded directional field affecting only <paramref name="affectedLayers"/>.
    /// Falloff does not apply.
    /// </summary>
    public static PhysicsForceField CreateWind(float2 direction, float strength, PhysicsMask affectedLayers)
    {
      return new PhysicsForceField
      {
        Type = FieldType.Directional,
        Direction = math.normalize(direction),
        Strength = strength,
        InnerRadius = 0f,
        OuterRadius = float.MaxValue,
        AffectedLayers = affectedLayers,
        MinDistance = DefaultMinDistance
      };
    }

    /// <summary>
    /// Radial field affecting all layers: positive strength pulls toward the center,
    /// negative pushes away. Prefer named arguments, e.g.
    /// <c>CreateRadial(center, strength: 50f, radius: 8f, falloff: FalloffMode.Linear)</c>.
    /// </summary>
    public static PhysicsForceField CreateRadial(float2 center, float strength, float radius,
        FalloffMode falloff = FalloffMode.None)
    {
      return CreateRadial(center, strength, radius, ~0u, falloff);
    }

    /// <summary>
    /// Radial field affecting only <paramref name="affectedLayers"/>: positive strength pulls
    /// toward the center, negative pushes away. Prefer named arguments, e.g.
    /// <c>CreateRadial(center, strength: 50f, radius: 8f, affectedLayers: mask, falloff: FalloffMode.Linear)</c>.
    /// </summary>
    public static PhysicsForceField CreateRadial(float2 center, float strength, float radius,
        PhysicsMask affectedLayers, FalloffMode falloff = FalloffMode.None)
    {
      return new PhysicsForceField
      {
        Type = FieldType.Radial,
        Center = center,
        Strength = strength,
        InnerRadius = 0f,
        OuterRadius = radius,
        AffectedLayers = affectedLayers,
        Falloff = falloff,
        MinDistance = DefaultMinDistance
      };
    }

    /// <summary>
    /// Vortex field affecting all layers: inward pull (strength * cos angle) combined with
    /// tangential spin (strength * sin angle). Prefer named arguments, e.g.
    /// <c>CreateVortex(center, strength: 30f, radius: 6f, falloff: FalloffMode.Linear)</c>.
    /// </summary>
    public static PhysicsForceField CreateVortex(float2 center, float strength, float radius,
        float angle = math.PI / 4f, FalloffMode falloff = FalloffMode.None)
    {
      return CreateVortex(center, strength, radius, ~0u, angle, falloff);
    }

    /// <summary>
    /// Vortex field affecting only <paramref name="affectedLayers"/>: inward pull
    /// (strength * cos angle) combined with tangential spin (strength * sin angle).
    /// Prefer named arguments, e.g.
    /// <c>CreateVortex(center, strength: 30f, radius: 6f, affectedLayers: mask)</c>.
    /// A positional uint mask (<c>CreateVortex(center, strength, radius, uintMask)</c>) is
    /// ambiguous with the angle overload; pass <c>affectedLayers:</c> or cast to PhysicsMask.
    /// </summary>
    public static PhysicsForceField CreateVortex(float2 center, float strength, float radius,
        PhysicsMask affectedLayers, float angle = math.PI / 4f, FalloffMode falloff = FalloffMode.None)
    {
      return new PhysicsForceField
      {
        Type = FieldType.Vortex,
        Center = center,
        Strength = strength,
        InnerRadius = 0f,
        OuterRadius = radius,
        VortexAngle = angle,
        AffectedLayers = affectedLayers,
        Falloff = falloff,
        MinDistance = DefaultMinDistance
      };
    }
  }
}
