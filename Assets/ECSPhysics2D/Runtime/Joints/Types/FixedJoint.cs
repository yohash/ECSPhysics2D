using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Weld Joint - Rigid connection between bodies.
  /// Can have slight flexibility for breakable connections.
  /// </summary>
  public struct WeldJoint : IComponentData
  {
    public float2 LocalAnchorA;
    public float2 LocalAnchorB;
    public float ReferenceAngle;    // Fixed relative angle

    // Flexibility (0 = rigid, >0 = flexible)
    public float LinearHertz;       // Linear spring frequency
    public float LinearDampingRatio;
    public float AngularHertz;      // Angular spring frequency  
    public float AngularDampingRatio;

    public static WeldJoint CreateRigid(float2 anchorA, float2 anchorB)
    {
      return new WeldJoint
      {
        LocalAnchorA = anchorA,
        LocalAnchorB = anchorB,
        ReferenceAngle = 0f,
        LinearHertz = 0f,
        LinearDampingRatio = 0f,
        AngularHertz = 0f,
        AngularDampingRatio = 0f
      };
    }

    public static WeldJoint CreateFlexible(float2 anchorA, float2 anchorB, float flexibility)
    {
      return new WeldJoint
      {
        LocalAnchorA = anchorA,
        LocalAnchorB = anchorB,
        ReferenceAngle = 0f,
        LinearHertz = 10f * (1f - flexibility),
        LinearDampingRatio = 0.5f,
        AngularHertz = 10f * (1f - flexibility),
        AngularDampingRatio = 0.5f
      };
    }
  }
}