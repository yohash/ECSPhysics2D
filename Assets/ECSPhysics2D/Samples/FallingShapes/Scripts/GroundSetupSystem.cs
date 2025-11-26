using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Creates the static ground plane and spawner config on startup.
  /// </summary>
  [UpdateInGroup(typeof(InitializationSystemGroup))]
  public partial struct GroundSetupSystem : ISystem
  {
    private bool initialized;

    public void OnCreate(ref SystemState state)
    {
      initialized = false;
    }

    public void OnUpdate(ref SystemState state)
    {
      if (initialized) {
        state.Enabled = false;
        return;
      }

      // Create spawner config singleton
      var configEntity = state.EntityManager.CreateEntity();
      state.EntityManager.AddComponentData(configEntity, ShapeSpawnerConfig.CreateDefault());

      // Create ground entity
      var groundEntity = state.EntityManager.CreateEntity();

      // Position at origin
      state.EntityManager.AddComponentData(groundEntity, LocalTransform.FromPosition(0f, 0f, 0f));

      // Physics components
      state.EntityManager.AddComponentData(groundEntity, new PhysicsBodyComponent
      {
        WorldIndex = 0
      });

      state.EntityManager.AddComponent<PhysicsStaticTag>(groundEntity);

      // ===== GENERATE A CHAIN SHAPE =====
      // Create curved ground (20 units wide, slight downward curve)
      int vertexCount = 21; // More vertices = smoother curve
      var vertices = new NativeArray<float2>(vertexCount * 2, Allocator.Temp);

      float width = 20f;
      float curveDepth = 1f; // How far center dips below edges

      for (int i = 0; i < vertexCount; i++) {
        float t = i / (float)(vertexCount - 1); // 0 to 1
        float x = (t - 0.5f) * width; // -10 to +10

        // Parabola: y = -curveDepth * 4 * (t - 0.5)^2
        float normalizedT = (t - 0.5f) * 2f; // -1 to +1
        float y = -curveDepth * (1f - normalizedT * normalizedT);

        vertices[vertexCount - i - 1] = new float2(x, y);
      }

      for (int i = 0; i < vertexCount; i++) {
        float t = i / (float)(vertexCount - 1); // 0 to 1
        float x = (t - 0.5f) * width; // -10 to +10

        // Parabola: y = -curveDepth * 4 * (t - 0.5)^2
        float normalizedT = (t - 0.5f) * 2f; // -1 to +1
        float y = -curveDepth * (1f - normalizedT * normalizedT) - 1f;

        vertices[vertexCount + i] = new float2(x, y);
      }

      var chainBlob = ChainBlobData.Create(vertices, Allocator.Persistent);
      vertices.Dispose();

      state.EntityManager.AddComponentData(groundEntity, new PhysicsShapeChain
      {
        ChainBlob = chainBlob,
        IsLoop = true // Upper and lower chain are built in proper outward-colliding order
      });
      // ===== END CHAIN SHAPE =====

      // Material properties
      state.EntityManager.AddComponentData(groundEntity, new PhysicsMaterial
      {
        Friction = 0.6f,
        Bounciness = 0.2f,
        Density = 1f,
        RollingResistance = 0f
      });

      // Collision filter - ground is on Terrain layer
      state.EntityManager.AddComponentData(groundEntity, CollisionFilter.Create(
          CollisionLayers.Terrain,
          CollisionLayers.Default,
          CollisionLayers.Debris
      ));

      initialized = true;
    }
  }
}
