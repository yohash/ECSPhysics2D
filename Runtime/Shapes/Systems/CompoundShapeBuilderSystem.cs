using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Handles bodies with multiple shapes using CompoundShapeElement buffers.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ShapeCreationSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct CompoundShapeBuilderSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (body, shapeBuffer, material, filter, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, DynamicBuffer<CompoundShape>,
              RefRO<PhysicsMaterial>, RefRO<CollisionFilter>>()
          .WithNone<ShapesCreatedTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid || shapeBuffer.IsEmpty)
          continue;

        var shapeRefs = new NativeList<PhysicsShape>(shapeBuffer.Length, Allocator.Temp);

        int shapesCreated = 0;

        for (int i = 0; i < shapeBuffer.Length; i++) {
          var element = shapeBuffer[i];

          var createdShape = CreateCompoundShape(body.ValueRO.Body, element, material.ValueRO, filter.ValueRO);
          if (createdShape.isValid) {
            shapeRefs.Add(createdShape);
          }

          shapesCreated++;
        }

        if (shapesCreated > 0) {
          var refBuffer = ecb.AddBuffer<PhysicsShapeReference>(entity);
          for (int i = 0; i < shapeRefs.Length; i++) {
            refBuffer.Add(new PhysicsShapeReference { Shape = shapeRefs[i] });
          }

          ecb.AddComponent(entity, new ShapesCreatedTag { ShapeCount = shapesCreated });
        }

        shapeRefs.Dispose();
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private PhysicsShape CreateCompoundShape(PhysicsBody body, CompoundShape element,
        PhysicsMaterial material, CollisionFilter filter)
    {
      var shapeDef = new PhysicsShapeDefinition
      {
        surfaceMaterial = new PhysicsShape.SurfaceMaterial
        {
          friction = material.Friction,
          bounciness = material.Bounciness
        },
        density = material.Density,
        isTrigger = filter.GenerateTriggerEvents,
        contactEvents = filter.GenerateCollisionEvents,
        contactFilter = new PhysicsShape.ContactFilter
        {
          categories = filter.Categories(),
          contacts = filter.Mask(),
          groupIndex = filter.GroupIndex
        }
      };

      switch (element.Type) {
        case CompoundShape.ShapeType.Circle:
          var circleGeometry = new CircleGeometry
          {
            center = element.Param0,
            radius = element.Param1.x
          };

          var circleShape = body.CreateShape(circleGeometry, shapeDef);
          if (material.RollingResistance > 0 && circleShape.isValid) {
            circleShape.rollingResistance = material.RollingResistance;
          }

          return circleShape;

        case CompoundShape.ShapeType.Box:
          // Box is created as a 4-vertex polygon
          var vertices = new NativeArray<float2>(4, Allocator.Temp);
          var halfSize = element.Param0 * 0.5f;
          var center = element.Param1;
          var rotation = element.Param2;

          // Create box vertices in local space
          vertices[0] = RotatePoint(new float2(-halfSize.x, -halfSize.y), rotation) + center;
          vertices[1] = RotatePoint(new float2(halfSize.x, -halfSize.y), rotation) + center;
          vertices[2] = RotatePoint(new float2(halfSize.x, halfSize.y), rotation) + center;
          vertices[3] = RotatePoint(new float2(-halfSize.x, halfSize.y), rotation) + center;

          var boxGeometry = new PolygonGeometry
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

          var boxShape = body.CreateShape(boxGeometry, shapeDef);
          if (material.RollingResistance > 0 && boxShape.isValid) {
            boxShape.rollingResistance = material.RollingResistance;
          }

          vertices.Dispose();
          return boxShape;

        case CompoundShape.ShapeType.Capsule:
          var capsuleGeometry = new CapsuleGeometry
          {
            center1 = element.Param0,
            center2 = element.Param1,
            radius = element.Param2
          };

          // Create the capsule shape
          var capsuleShape = body.CreateShape(capsuleGeometry, shapeDef);
          if (material.RollingResistance > 0 && capsuleShape.isValid) {
            capsuleShape.rollingResistance = material.RollingResistance;
          }

          return capsuleShape;

        case CompoundShape.ShapeType.Polygon:
          // TBD
          return default;

        default:
          return default;
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