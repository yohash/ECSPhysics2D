using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component for capsule collision geometry.
  /// NEW in Box2D v3! A rounded rectangle - perfect for characters.
  /// </summary>
  public struct PhysicsShapeCapsule : IComponentData
  {
    public float2 Center1;  // First circle center
    public float2 Center2;  // Second circle center
    public float Radius;     // Radius of the end caps

    public float Length => math.distance(Center1, Center2);
    public float2 Direction => math.normalizesafe(Center2 - Center1);

    /// <summary>
    /// Creates a vertical capsule (common for characters)
    /// </summary>
    public static PhysicsShapeCapsule CreateVertical(float height, float radius)
    {
      var halfHeight = height * 0.5f - radius;
      return new PhysicsShapeCapsule
      {
        Center1 = new float2(0, -halfHeight),
        Center2 = new float2(0, halfHeight),
        Radius = radius
      };
    }

    /// <summary>
    /// Creates a horizontal capsule
    /// </summary>
    public static PhysicsShapeCapsule CreateHorizontal(float width, float radius)
    {
      var halfWidth = width * 0.5f - radius;
      return new PhysicsShapeCapsule
      {
        Center1 = new float2(-halfWidth, 0),
        Center2 = new float2(halfWidth, 0),
        Radius = radius
      };
    }
  }
}