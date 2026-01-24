using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Authoring component for windmill structure.
  /// Bakes to a config component that gets processed at runtime.
  /// </summary>
  public class WindmillAuthoring : MonoBehaviour
  {
    [Header("Windmill Dimensions")]
    public float CentralRadius = 0.8f;
    public float SpokeLength = 2.5f;
    public float SpokeWidth = 0.4f;

    [Header("Motor Properties")]
    public float RotationSpeed = 3f;  // rad/s
    public float MaxTorque = 250f;

    [Header("Spoke Damage")]
    public float SpokeBreakThreshold = 120f;

    [Header("Material")]
    public float Friction = 0.4f;
    public float Bounciness = 0.2f;
    public float Density = 1.5f;

    class Baker : Baker<WindmillAuthoring>
    {
      public override void Bake(WindmillAuthoring authoring)
      {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        // Store configuration - the initialization system will create the actual entities
        AddComponent(entity, WindmillConfig.Create(
            position: authoring.transform.position,
            centralRadius: authoring.CentralRadius,
            spokeLength: authoring.SpokeLength,
            spokeWidth: authoring.SpokeWidth,
            rotationSpeed: authoring.RotationSpeed,
            maxTorque: authoring.MaxTorque,
            spokeBreakThreshold: authoring.SpokeBreakThreshold,
            friction: authoring.Friction,
            bounciness: authoring.Bounciness,
            density: authoring.Density
        ));
      }
    }
  }
}
