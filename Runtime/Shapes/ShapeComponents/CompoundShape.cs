using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Buffer element for compound shapes - multiple shapes per body.
  /// Stored as a DynamicBuffer on entities that need multiple collision shapes.
  /// </summary>
  [InternalBufferCapacity(4)]  // Most compound bodies have 2-4 shapes
  public struct CompoundShape : IBufferElementData
  {
    public enum ShapeType : byte
    {
      Circle,
      Box,
      Capsule,
      Polygon
    }

    public ShapeType Type;
    public PhysicsShape Shape;  // Handle to created shape

    // Shape-specific data based on Type
    // Using a union-like approach with shared memory
    public float2 Param0;  // Circle: center/radius | Box: size | Capsule: center1
    public float2 Param1;  // Box: center | Capsule: center2
    public float Param2;    // Box: rotation | Capsule: radius

    // Reference to polygon or chain data if needed
    public BlobAssetReference<ChainBlobData> ChainData;
  }
}