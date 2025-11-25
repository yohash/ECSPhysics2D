using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Processes spawn requests and creates physics entities.
  /// Runs before BuildPhysicsWorld so bodies are created this frame.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(BuildPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct ShapeSpawningSystem : ISystem
  {
    private Random random;

    public void OnCreate(ref SystemState state)
    {
      random = new Random((uint)System.DateTime.Now.Ticks);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<ShapeSpawnerConfig>(out var config))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (request, requestEntity) in
          SystemAPI.Query<RefRO<SpawnShapeRequest>>()
          .WithEntityAccess()) {
        // Create shape entity
        var shapeEntity = ecb.CreateEntity();

        // Transform
        ecb.AddComponent(shapeEntity, LocalTransform.FromPosition(
            request.ValueRO.SpawnPosition.x,
            request.ValueRO.SpawnPosition.y,
            0f
        ));

        // Physics body (uninitialized - BuildPhysicsWorldSystem will create it)
        ecb.AddComponent(shapeEntity, new PhysicsBodyComponent
        {
          WorldIndex = 0
        });

        // Dynamic tag (physics drives transform)
        ecb.AddComponent<PhysicsDynamicTag>(shapeEntity);

        // Velocity component
        ecb.AddComponent(shapeEntity, PhysicsVelocity.Zero);

        // Mass
        ecb.AddComponent(shapeEntity, PhysicsMass.CreateDefault(1f));

        // Damping
        ecb.AddComponent(shapeEntity, PhysicsDamping.Default);

        // Gravity
        ecb.AddComponent(shapeEntity, PhysicsGravityScale.Default);

        // Material
        ecb.AddComponent(shapeEntity, new PhysicsMaterial
        {
          Friction = 0.4f,
          Bounciness = random.NextFloat(0.1f, 0.6f),
          Density = 1f,
          RollingResistance = 0.05f
        });

        // Collision filter - debris layer
        ecb.AddComponent(shapeEntity, CollisionFilter.Create(
            CollisionLayers.Debris,
            CollisionLayers.Terrain,
            CollisionLayers.Debris
        ));

        var size = request.ValueRO.Size;

        var shapeType = request.ValueRO.Type;
        if (shapeType == SpawnShapeRequest.ShapeType.Random) {
          // Add random shape
          shapeType = (SpawnShapeRequest.ShapeType)random.NextInt(1, 5); // 1-4 (skip Random)
        }

        switch (shapeType) {
          case SpawnShapeRequest.ShapeType.Circle:
            ecb.AddComponent(shapeEntity,
              new PhysicsShapeCircle
              {
                Radius = size * 0.5f,
                Center = float2.zero
              });
            break;

          case SpawnShapeRequest.ShapeType.Box:
            ecb.AddComponent(shapeEntity,
              new PhysicsShapeBox
              {
                Size = new float2(size, size),
                Center = float2.zero,
                Rotation = random.NextFloat(0f, math.PI * 2f)
              });
            break;

          case SpawnShapeRequest.ShapeType.Capsule:
            ecb.AddComponent(shapeEntity,
              PhysicsShapeCapsule.CreateWithRotation(
                size * 1.5f,
                size * 0.3f,
                random.NextFloat(0f, math.PI * 2f)
              ));
            break;

          case SpawnShapeRequest.ShapeType.Polygon:
            // Random triangle, pentagon, or hexagon
            var sides = random.NextInt(3, 7);
            ecb.AddComponent(shapeEntity,
              PhysicsShapePolygon.CreateRegular(
                sides,
                size * 0.5f
              ).Rotate(random.NextFloat(0f, math.PI * 2f)));
            break;
        }

        // Remove the request
        ecb.DestroyEntity(requestEntity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
