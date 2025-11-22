using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component for box collision geometry.
  /// Internally stored as a 4-vertex polygon.
  /// </summary>
  public struct PhysicsShapeBox : IComponentData
  {
    public float2 Size;
    public float2 Center;  // Local offset from body center
    public float Rotation; // Local rotation in radians

    public float2 HalfSize => Size * 0.5f;

    public static PhysicsShapeBox Default => new PhysicsShapeBox
    {
      Size = new float2(1f, 1f),
      Center = float2.zero,
      Rotation = 0f
    };
  }
}