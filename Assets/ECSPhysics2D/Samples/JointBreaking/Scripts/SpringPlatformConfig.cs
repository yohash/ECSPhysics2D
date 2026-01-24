using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Configuration component for spring platform initialization.
  /// This gets converted to actual entities by SpringPlatformInitializationSystem.
  /// Removed after initialization completes.
  /// </summary>
  public struct SpringPlatformConfig : IComponentData
  {
    public float3 SpawnPosition;      // Where to create the platform
    public float PlatformWidth;
    public float PlatformHeight;
    public float AnchorSize;
    public float SpringFrequency;
    public float SpringDamping;
    public float LowerAngleLimit;     // Radians
    public float UpperAngleLimit;     // Radians
    public float BreakThreshold;
    public float Friction;
    public float Bounciness;
    public float Density;
    public bool Flip;

    public static SpringPlatformConfig Create(
        float3 position,
        float platformWidth,
        float platformHeight,
        float anchorSize,
        float springFrequency,
        float springDamping,
        float lowerAngleDegrees,
        float upperAngleDegrees,
        float breakThreshold,
        float friction,
        float bounciness,
        float density,
        bool flip)
    {
      return new SpringPlatformConfig
      {
        SpawnPosition = position,
        PlatformWidth = platformWidth,
        PlatformHeight = platformHeight,
        AnchorSize = anchorSize,
        SpringFrequency = springFrequency,
        SpringDamping = springDamping,
        LowerAngleLimit = lowerAngleDegrees,
        UpperAngleLimit = upperAngleDegrees,
        BreakThreshold = breakThreshold,
        Friction = friction,
        Bounciness = bounciness,
        Density = density,
        Flip = flip
      };
    }
  }
}
