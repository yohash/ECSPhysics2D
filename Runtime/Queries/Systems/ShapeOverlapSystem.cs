using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that processes shape overlap queries.
  /// 
  /// Each request specifies which physics world to query via WorldIndex.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(RaycastSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ShapeOverlapSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (request, entity) in
          SystemAPI.Query<RefRO<OverlapShapeRequest>>()
          .WithEntityAccess()) {
        // Get the correct physics world for this query
        var worldIndex = request.ValueRO.WorldIndex;
        if (!singleton.IsValidWorldIndex(worldIndex)) {
          worldIndex = 0;
        }

        var physicsWorld = singleton.GetWorld(worldIndex);
        var overlaps = new NativeArray<PhysicsQuery.WorldOverlapResult>(16, Allocator.Temp);

        switch (request.ValueRO.Type) {
          case OverlapShapeRequest.ShapeType.Circle: {
              var geometry = new CircleGeometry
              {
                center = request.ValueRO.Position,
                radius = request.ValueRO.Size.x
              };
              var shapeProxy = new PhysicsShape.ShapeProxy(geometry);
              overlaps = physicsWorld.OverlapShapeProxy(shapeProxy, request.ValueRO.Filter);
            }
            break;

          case OverlapShapeRequest.ShapeType.Box: {
              var vertices = new NativeArray<float2>(4, Allocator.Temp);
              var halfSize = request.ValueRO.Size * 0.5f;
              var rotation = request.ValueRO.Rotation;
              var center = request.ValueRO.Position;

              // Create box vertices in local space
              vertices[0] = RotatePoint(new float2(-halfSize.x, -halfSize.y), rotation) + center;
              vertices[1] = RotatePoint(new float2(halfSize.x, -halfSize.y), rotation) + center;
              vertices[2] = RotatePoint(new float2(halfSize.x, halfSize.y), rotation) + center;
              vertices[3] = RotatePoint(new float2(-halfSize.x, halfSize.y), rotation) + center;

              var geometry = new PolygonGeometry
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

              var shapeProxy = new PhysicsShape.ShapeProxy(geometry);
              overlaps = physicsWorld.OverlapShapeProxy(shapeProxy, request.ValueRO.Filter);

              vertices.Dispose();
            }
            break;

          case OverlapShapeRequest.ShapeType.Capsule: {
              var halfHeight = request.ValueRO.Size.x * 0.5f - request.ValueRO.Size.y;

              // compute center while considering rotation
              var center1 = RotatePoint(new float2(0, -halfHeight), request.ValueRO.Rotation) + request.ValueRO.Position;
              var center2 = RotatePoint(new float2(0, halfHeight), request.ValueRO.Rotation) + request.ValueRO.Position;

              var geometry = new CapsuleGeometry
              {
                center1 = center1,
                center2 = center2,
                radius = request.ValueRO.Size.y
              };

              var shapeProxy = new PhysicsShape.ShapeProxy(geometry);
              overlaps = physicsWorld.OverlapShapeProxy(shapeProxy, request.ValueRO.Filter);
            }
            break;
        }

        // Store results in buffer
        if (request.ValueRO.ResultEntity != Entity.Null) {
          var buffer = ecb.SetBuffer<OverlapResult>(request.ValueRO.ResultEntity);

          for (int i = 0; i < overlaps.Length; i++) {
            buffer.Add(new OverlapResult
            {
              Body = overlaps[i].shape.body,
              Shape = overlaps[i].shape,
              Entity = overlaps[i].shape.body.GetEntityUserData()
            });
          }
        }

        overlaps.Dispose();
        ecb.RemoveComponent<OverlapShapeRequest>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
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
