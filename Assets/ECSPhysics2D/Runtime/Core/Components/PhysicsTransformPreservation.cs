using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Preserves the Z position during 2D physics simulation.
  /// Physics operates in XY plane, but entities exist in 3D space.
  /// </summary>
  public struct PhysicsTransformPreservation : IComponentData
  {
    public float ZPosition;
    public float3 Scale; // Preserved but not used by physics
  }
}