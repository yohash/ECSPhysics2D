using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Configuration component for windmill initialization.
  /// This gets converted to actual entities by WindmillInitializationSystem.
  /// Removed after initialization completes.
  /// </summary>
  public struct WindmillConfig : IComponentData
  {
    public float3 SpawnPosition;
    public float CentralRadius;
    public float SpokeLength;
    public float SpokeWidth;
    public float RotationSpeed;
    public float MaxTorque;
    public float SpokeBreakThreshold;
    public float Friction;
    public float Bounciness;
    public float Density;

    public static WindmillConfig Create(
        float3 position,
        float centralRadius,
        float spokeLength,
        float spokeWidth,
        float rotationSpeed,
        float maxTorque,
        float spokeBreakThreshold,
        float friction,
        float bounciness,
        float density)
    {
      return new WindmillConfig
      {
        SpawnPosition = position,
        CentralRadius = centralRadius,
        SpokeLength = spokeLength,
        SpokeWidth = spokeWidth,
        RotationSpeed = rotationSpeed,
        MaxTorque = maxTorque,
        SpokeBreakThreshold = spokeBreakThreshold,
        Friction = friction,
        Bounciness = bounciness,
        Density = density
      };
    }
  }
}
