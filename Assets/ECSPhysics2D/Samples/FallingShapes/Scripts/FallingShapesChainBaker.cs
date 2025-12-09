using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Bakes ChainAuthoring GameObject into ECS entity with physics components.
  /// </summary>
  public class FallingShapesChainBaker : Baker<FallingShapesChainAuthoring>
  {
    public override void Bake(FallingShapesChainAuthoring authoring)
    {
      var entity = GetEntity(TransformUsageFlags.Dynamic);

      // Transform (preserved from GameObject)
      // Unity automatically handles LocalTransform baking

      // Physics body
      AddComponent(entity, new PhysicsBodyComponent
      {
        WorldIndex = 0
      });
      AddComponent<PhysicsStaticTag>(entity);

      // Generate chain vertices
      var vertices = GenerateChainVertices(authoring);
      var chainBlob = ChainBlobData.Create(vertices, Allocator.Persistent);
      vertices.Dispose();

      AddComponent(entity, new PhysicsShapeChain
      {
        ChainBlob = chainBlob,
        IsLoop = authoring.IsLoop
      });

      // Material
      AddComponent(entity, new PhysicsMaterial
      {
        Friction = authoring.Friction,
        Bounciness = authoring.Bounciness,
        Density = authoring.Density,
        RollingResistance = 0f
      });

      // Collision filter
      AddComponent(entity, ParseCollisionFilter(authoring));
    }

    private NativeArray<float2> GenerateChainVertices(FallingShapesChainAuthoring authoring)
    {
      int vertexCount = authoring.SegmentCount + 1;
      var vertices = new NativeArray<float2>(vertexCount * 2, Allocator.Temp);

      for (int i = 0; i < vertexCount; i++) {
        float t = i / (float)(vertexCount - 1);
        float x = (t - 0.5f) * authoring.Width;

        // Parabola: y = -curveDepth * 4 * (t - 0.5)^2
        float normalizedT = (t - 0.5f) * 2f;
        float y = -authoring.CurveDepth * (1f - normalizedT * normalizedT);

        vertices[vertexCount - i - 1] = new float2(x, y);
      }

      for (int i = 0; i < vertexCount; i++) {
        float t = i / (float)(vertexCount - 1);
        float x = (t - 0.5f) * authoring.Width;

        // Parabola: y = -curveDepth * 4 * (t - 0.5)^2
        float normalizedT = (t - 0.5f) * 2f;
        float y = -authoring.CurveDepth * (1f - normalizedT * normalizedT) - 0.25f;

        vertices[vertexCount + i] = new float2(x, y);
      }

      return vertices;
    }

    private CollisionFilter ParseCollisionFilter(FallingShapesChainAuthoring authoring)
    {
      uint categoryBits = (uint)(1 << authoring.CollisionLayer);

      // Parse comma-separated layer indices
      uint maskBits = 0;
      var layers = authoring.CollidesWithLayers;
      foreach (var layer in layers) {
        maskBits |= (uint)(1 << layer);
      }

      return new CollisionFilter
      {
        CategoryBits = categoryBits,
        MaskBits = maskBits,
        GroupIndex = 0
      };
    }
  }
}