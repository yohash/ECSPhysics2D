using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component for circular collision geometry.
  /// Most efficient shape type - use when possible.
  /// </summary>
  public struct PhysicsShapeCircle : IComponentData
  {
    public float Radius;
    public float2 Center;  // Local offset from body center

    public static PhysicsShapeCircle Default => new PhysicsShapeCircle
    {
      Radius = 0.5f,
      Center = float2.zero
    };
  }
}