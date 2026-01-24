using Unity.Entities;
using UnityEngine;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Authoring component for spring-loaded platforms.
  /// Bakes to a config component that gets processed at runtime.
  /// </summary>
  public class SpringPlatformAuthoring : MonoBehaviour
  {
    [Header("Platform Dimensions")]
    public float PlatformWidth = 3f;
    public float PlatformHeight = 0.3f;
    public float AnchorSize = 0.5f;
    public bool Flip = false;

    [Header("Spring Properties")]
    public float SpringFrequency = 2.0f;
    public float SpringDamping = 0.5f;
    public float LowerAngleLimit = -45f;  // Degrees
    public float UpperAngleLimit = 30f;   // Degrees

    [Header("Joint Damage")]
    public float BreakThreshold = 75f;

    [Header("Material")]
    public float Friction = 0.5f;
    public float Bounciness = 0.2f;
    public float Density = 1f;

    class Baker : Baker<SpringPlatformAuthoring>
    {
      public override void Bake(SpringPlatformAuthoring authoring)
      {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        // Store configuration - the initialization system will create the actual entities
        AddComponent(entity, SpringPlatformConfig.Create(
            position: authoring.transform.position,
            platformWidth: authoring.PlatformWidth,
            platformHeight: authoring.PlatformHeight,
            anchorSize: authoring.AnchorSize,
            springFrequency: authoring.SpringFrequency,
            springDamping: authoring.SpringDamping,
            lowerAngleDegrees: authoring.LowerAngleLimit,
            upperAngleDegrees: authoring.UpperAngleLimit,
            breakThreshold: authoring.BreakThreshold,
            friction: authoring.Friction,
            bounciness: authoring.Bounciness,
            density: authoring.Density,
            flip: authoring.Flip
        ));
      }
    }
  }
}
