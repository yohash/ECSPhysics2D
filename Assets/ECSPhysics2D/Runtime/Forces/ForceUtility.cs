using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Utility methods for force calculations.
  /// </summary>
  public static class ForceUtility
  {
    /// <summary>
    /// Calculate force needed to reach target velocity in one timestep.
    /// </summary>
    public static float2 CalculateForceForVelocity(float2 currentVelocity, float2 targetVelocity, float mass, float deltaTime)
    {
      var deltaV = targetVelocity - currentVelocity;
      return deltaV * mass / deltaTime;
    }

    /// <summary>
    /// Calculate impulse needed to reach target velocity instantly.
    /// </summary>
    public static float2 CalculateImpulseForVelocity(float2 currentVelocity, float2 targetVelocity, float mass)
    {
      var deltaV = targetVelocity - currentVelocity;
      return deltaV * mass;
    }

    /// <summary>
    /// Calculate explosion impulse for a point, using the same distance profile as Box2D's Explode():
    /// full magnitude within explosionRadius, fading linearly to zero over falloff beyond it.
    /// This is a point approximation; Box2D additionally scales by each shape's projected perimeter.
    /// </summary>
    public static float2 CalculateExplosionImpulse(float2 bodyPosition, float2 explosionCenter,
        float explosionForce, float explosionRadius, float falloff)
    {
      var delta = bodyPosition - explosionCenter;
      var distance = math.length(delta);

      if (distance > explosionRadius + falloff || distance < 0.001f)
        return float2.zero;

      var direction = delta / distance;
      float forceMagnitude = explosionForce;

      // scale for linear falloff beyond radius in the "falloff" distance
      if (falloff > 0f && distance > explosionRadius) {
        forceMagnitude *= math.max(0f, 1f - (distance - explosionRadius) / falloff);
      }

      return direction * forceMagnitude;
    }
  }
}