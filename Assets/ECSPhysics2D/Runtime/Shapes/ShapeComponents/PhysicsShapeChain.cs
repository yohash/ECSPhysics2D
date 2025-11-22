using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component for chain collision geometry (connected line segments).
  /// Perfect for terrain, level boundaries, or complex static geometry.
  /// Chains are always static or kinematic, never dynamic.
  /// </summary>
  public struct PhysicsShapeChain : IComponentData
  {
    public BlobAssetReference<ChainBlobData> ChainBlob;
    public bool IsLoop;  // If true, last vertex connects to first

    public int VertexCount => ChainBlob.IsCreated ? ChainBlob.Value.VertexCount : 0;
  }

  /// <summary>
  /// Blob data structure for efficient chain vertex storage.
  /// Allows arbitrary number of vertices without dynamic allocation.
  /// </summary>
  public struct ChainBlobData
  {
    public int VertexCount;
    public BlobArray<float2> Vertices;

    public static BlobAssetReference<ChainBlobData> Create(NativeArray<float2> vertices, Allocator allocator)
    {
      var builder = new BlobBuilder(allocator);
      ref var root = ref builder.ConstructRoot<ChainBlobData>();

      root.VertexCount = vertices.Length;
      var vertexArray = builder.Allocate(ref root.Vertices, vertices.Length);

      for (int i = 0; i < vertices.Length; i++) {
        vertexArray[i] = vertices[i];
      }

      var result = builder.CreateBlobAssetReference<ChainBlobData>(allocator);
      builder.Dispose();
      return result;
    }
  }
}