using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Mouse Joint - Interactive dragging joint.
  /// Pulls a body toward a world target with spring forces.
  /// </summary>
  public struct MouseJoint : IComponentData
  {
    public float2 Target;           // World space target
    public float2 LocalAnchor;      // Anchor on the body
    public float Hertz;             // Response frequency
    public float DampingRatio;      // Response damping
    public float MaxForce;          // Maximum pull force

    public static MouseJoint Create(float2 worldTarget, float stiffness = 5f)
    {
      return new MouseJoint
      {
        Target = worldTarget,
        LocalAnchor = float2.zero,
        Hertz = stiffness,
        DampingRatio = 0.7f,
        MaxForce = 1000f
      };
    }
  }
}