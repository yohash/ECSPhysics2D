using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Creates shapes for all geometry types based on components.
  /// Handles single shapes and compound shapes.
  /// 
  /// Shapes are created on the body specified in PhysicsBodyComponent,
  /// which is already associated with the correct physics world.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(BuildPhysicsWorldSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ShapeCreationSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      CreateCircleShapes(ref state, ecb);
      CreateBoxShapes(ref state, ecb);
      CreateCapsuleShapes(ref state, ecb);
      CreatePolygonShapes(ref state, ecb);
      CreateChainShapes(ref state, ecb);

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void CreateCircleShapes(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, shape, material, filter, entity) in
          SystemAPI.Query<
            RefRO<PhysicsBodyComponent>,
            RefRO<PhysicsShapeCircle>,
            RefRO<PhysicsMaterial>,
            RefRO<CollisionFilter>>()
          .WithNone<ShapesCreatedTag>()
          .WithEntityAccess()) {

        if (!body.ValueRO.IsValid)
          continue;

        var shapeDef = new PhysicsShapeDefinition
        {
          surfaceMaterial = new PhysicsShape.SurfaceMaterial
          {
            friction = material.ValueRO.Friction,
            bounciness = material.ValueRO.Bounciness
          },
          density = material.ValueRO.Density,
          isTrigger = filter.ValueRO.GenerateTriggerEvents,
          contactEvents = filter.ValueRO.GenerateCollisionEvents,
          contactFilter = filter.ValueRO.ToContactFilter()
        };

        var shapeGeometry = new CircleGeometry
        {
          center = shape.ValueRO.Center,
          radius = shape.ValueRO.Radius
        };

        // Shape is created on the body, which is already in the correct world
        var physicsShape = body.ValueRO.Body.CreateShape(shapeGeometry, shapeDef);

        // Store rolling resistance if supported
        if (material.ValueRO.RollingResistance > 0) {
          physicsShape.rollingResistance = material.ValueRO.RollingResistance;
        }

        // Store shape reference for runtime modification
        var shapeBuffer = ecb.AddBuffer<PhysicsShapeReference>(entity);
        shapeBuffer.Add(new PhysicsShapeReference { Shape = physicsShape });

        ecb.AddComponent(entity, new ShapesCreatedTag { ShapeCount = 1 });
      }
    }

    private void CreateBoxShapes(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, shape, material, filter, entity) in
          SystemAPI.Query<
            RefRO<PhysicsBodyComponent>,
            RefRO<PhysicsShapeBox>,
            RefRO<PhysicsMaterial>,
            RefRO<CollisionFilter>>()
          .WithNone<ShapesCreatedTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        // Box is created as a 4-vertex polygon
        var vertices = new NativeArray<float2>(4, Allocator.Temp);
        var halfSize = shape.ValueRO.HalfSize;
        var center = shape.ValueRO.Center;
        var rotation = shape.ValueRO.Rotation;

        // Create box vertices in local space
        vertices[0] = RotatePoint(new float2(-halfSize.x, -halfSize.y), rotation) + center;
        vertices[1] = RotatePoint(new float2(halfSize.x, -halfSize.y), rotation) + center;
        vertices[2] = RotatePoint(new float2(halfSize.x, halfSize.y), rotation) + center;
        vertices[3] = RotatePoint(new float2(-halfSize.x, halfSize.y), rotation) + center;

        var shapeDef = new PhysicsShapeDefinition
        {
          surfaceMaterial = new PhysicsShape.SurfaceMaterial
          {
            friction = material.ValueRO.Friction,
            bounciness = material.ValueRO.Bounciness
          },
          density = material.ValueRO.Density,
          isTrigger = filter.ValueRO.GenerateTriggerEvents,
          contactEvents = filter.ValueRO.GenerateCollisionEvents,
          contactFilter = filter.ValueRO.ToContactFilter()
        };

        var shapeGeometry = new PolygonGeometry
        {
          count = 4,
          vertices = new PhysicsShape.ShapeArray
          {
            vertex0 = vertices[0],
            vertex1 = vertices[1],
            vertex2 = vertices[2],
            vertex3 = vertices[3]
          }
        }.Validate();

        var physicsShape = body.ValueRO.Body.CreateShape(shapeGeometry, shapeDef);

        // Store shape reference for runtime modification
        var shapeBuffer = ecb.AddBuffer<PhysicsShapeReference>(entity);
        shapeBuffer.Add(new PhysicsShapeReference { Shape = physicsShape });

        if (material.ValueRO.RollingResistance > 0) {
          physicsShape.rollingResistance = material.ValueRO.RollingResistance;
        }

        vertices.Dispose();
        ecb.AddComponent(entity, new ShapesCreatedTag { ShapeCount = 1 });
      }
    }

    private void CreateCapsuleShapes(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, shape, material, filter, entity) in
          SystemAPI.Query<
            RefRO<PhysicsBodyComponent>,
            RefRO<PhysicsShapeCapsule>,
            RefRO<PhysicsMaterial>,
            RefRO<CollisionFilter>>()
          .WithNone<ShapesCreatedTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        var shapeDef = new PhysicsShapeDefinition
        {
          surfaceMaterial = new PhysicsShape.SurfaceMaterial
          {
            friction = material.ValueRO.Friction,
            bounciness = material.ValueRO.Bounciness
          },
          density = material.ValueRO.Density,
          isTrigger = filter.ValueRO.GenerateTriggerEvents,
          contactEvents = filter.ValueRO.GenerateCollisionEvents,
          contactFilter = filter.ValueRO.ToContactFilter()
        };

        var shapeGeometry = new CapsuleGeometry
        {
          center1 = shape.ValueRO.Center1,
          center2 = shape.ValueRO.Center2,
          radius = shape.ValueRO.Radius
        };

        var physicsShape = body.ValueRO.Body.CreateShape(shapeGeometry, shapeDef);

        // Store shape reference for runtime modification
        var shapeBuffer = ecb.AddBuffer<PhysicsShapeReference>(entity);
        shapeBuffer.Add(new PhysicsShapeReference { Shape = physicsShape });

        if (material.ValueRO.RollingResistance > 0) {
          physicsShape.rollingResistance = material.ValueRO.RollingResistance;
        }

        ecb.AddComponent(entity, new ShapesCreatedTag { ShapeCount = 1 });
      }
    }

    private void CreatePolygonShapes(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, shape, material, filter, entity) in
          SystemAPI.Query<
            RefRO<PhysicsBodyComponent>,
            RefRO<PhysicsShapePolygon>,
            RefRO<PhysicsMaterial>,
            RefRO<CollisionFilter>>()
          .WithNone<ShapesCreatedTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        var vertices = new NativeArray<float2>(shape.ValueRO.VertexCount, Allocator.Temp);
        for (int i = 0; i < shape.ValueRO.VertexCount; i++) {
          vertices[i] = shape.ValueRO.GetVertex(i);
        }

        // Validate polygon
        if (!ShapeUtility.ValidatePolygon(vertices)) {
          // Try to fix by ensuring CCW order
          ShapeUtility.EnsureCCW(vertices);

          // Re-validate
          if (!ShapeUtility.ValidatePolygon(vertices)) {
            UnityEngine.Debug.LogError($"Invalid polygon on entity {entity}: Not convex or degenerate");
            vertices.Dispose();
            continue;
          }
        }

        var shapeDef = new PhysicsShapeDefinition
        {
          surfaceMaterial = new PhysicsShape.SurfaceMaterial
          {
            friction = material.ValueRO.Friction,
            bounciness = material.ValueRO.Bounciness
          },
          density = material.ValueRO.Density,
          isTrigger = filter.ValueRO.GenerateTriggerEvents,
          contactEvents = filter.ValueRO.GenerateCollisionEvents,
          contactFilter = filter.ValueRO.ToContactFilter()
        };

        var shapeArray = new PhysicsShape.ShapeArray
        {
          vertex0 = shape.ValueRO.GetVertex(0),
          vertex1 = shape.ValueRO.GetVertex(1),
          vertex2 = shape.ValueRO.GetVertex(2),
          vertex3 = shape.ValueRO.GetVertex(3),
          vertex4 = shape.ValueRO.GetVertex(4),
          vertex5 = shape.ValueRO.GetVertex(5),
          vertex6 = shape.ValueRO.GetVertex(6),
          vertex7 = shape.ValueRO.GetVertex(7),
        };

        var shapeGeometry = new PolygonGeometry
        {
          vertices = shapeArray,
          count = shape.ValueRO.VertexCount
        }.Validate();

        var physicsShape = body.ValueRO.Body.CreateShape(shapeGeometry, shapeDef);

        // Store shape reference for runtime modification
        var shapeBuffer = ecb.AddBuffer<PhysicsShapeReference>(entity);
        shapeBuffer.Add(new PhysicsShapeReference { Shape = physicsShape });

        if (material.ValueRO.RollingResistance > 0) {
          physicsShape.rollingResistance = material.ValueRO.RollingResistance;
        }

        vertices.Dispose();
        ecb.AddComponent(entity, new ShapesCreatedTag { ShapeCount = 1 });
      }
    }

    private void CreateChainShapes(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, shape, material, filter, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRO<PhysicsShapeChain>,
              RefRO<PhysicsMaterial>, RefRO<CollisionFilter>>()
          .WithNone<ShapesCreatedTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid || !shape.ValueRO.ChainBlob.IsCreated)
          continue;

        ref var chainData = ref shape.ValueRO.ChainBlob.Value;
        var vertices = new NativeArray<Vector2>(chainData.VertexCount, Allocator.Temp);

        for (int i = 0; i < chainData.VertexCount; i++) {
          vertices[i] = chainData.Vertices[i];
        }

        var chainDef = new PhysicsChainDefinition
        {
          isLoop = shape.ValueRO.IsLoop,
          contactFilter = filter.ValueRO.ToContactFilter(),
          surfaceMaterial = new PhysicsShape.SurfaceMaterial
          {
            friction = material.ValueRO.Friction,
            bounciness = material.ValueRO.Bounciness
          },
          triggerEvents = filter.ValueRO.GenerateTriggerEvents,
        };

        var shapeGeometry = new ChainGeometry(vertices);

        body.ValueRO.Body.CreateChain(shapeGeometry, chainDef);

        vertices.Dispose();
        ecb.AddComponent(entity, new ShapesCreatedTag { ShapeCount = 1 });
      }
    }

    private float2 RotatePoint(float2 point, float angle)
    {
      float cos = math.cos(angle);
      float sin = math.sin(angle);
      return new float2(
          point.x * cos - point.y * sin,
          point.x * sin + point.y * cos
      );
    }
  }
}
