using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that processes closest point queries.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(AABBQuerySystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ClosestPointQuerySystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var physicsWorld = physicsWorldSingleton.World;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);


      foreach (var (request, entity) in
          SystemAPI.Query<RefRO<ClosestPointRequest>>()
          .WithEntityAccess()) {

        // Query nearby shapes
        var searchRadius = request.ValueRO.MaxDistance;
        var aabb = new PhysicsAABB
        {
          lowerBound = request.ValueRO.Point - new float2(searchRadius, searchRadius),
          upperBound = request.ValueRO.Point + new float2(searchRadius, searchRadius)
        };

        var overlaps = physicsWorld.OverlapAABB(aabb, request.ValueRO.Filter);

        // Find closest point among all overlapping shapes
        float closestDistSq = searchRadius * searchRadius;
        ClosestPointResult result = default;

        for (int i = 0; i < overlaps.Length; i++) {
          var shape = overlaps[i].shape;
          var body = overlaps[i].shape.body;

          // Get shape's closest point to query point
          var closestPoint = shape.ClosestPoint(request.ValueRO.Point);

          // Workaround: Unity's ClosestPoint() returns the interior query point for
          // polygons when the query point is inside the shape. Snap to the nearest
          // edge surface point for consistent boundary behaviour.
          if (shape.shapeType == PhysicsShape.ShapeType.Polygon &&
              math.distancesq(closestPoint, request.ValueRO.Point) < 1e-10f) {
            closestPoint = SnapPolygonToNearestEdge(shape, body, request.ValueRO.Point);
          }
          var difference = new float2
          {
            x = closestPoint.x - request.ValueRO.Point.x,
            y = closestPoint.y - request.ValueRO.Point.y
          };
          var distSq = math.lengthsq(difference);

          if (distSq < closestDistSq) {
            closestDistSq = distSq;
            result = new ClosestPointResult
            {
              Found = true,
              ClosestPoint = closestPoint,
              Distance = math.sqrt(distSq),
              Body = body,
              Shape = shape,
              Entity = body.GetEntityUserData()
            };
          }
        }

        if (found) {
          result.Normal = ComputeOutwardNormal(result.Shape, result.Body, result.ClosestPoint, request.ValueRO.Point);
        }

        // Store result — always write, even when not found, so consumers
        // don't read stale data from a previous frame's query.
        if (request.ValueRO.ResultEntity != Entity.Null) {
          ecb.SetComponent(request.ValueRO.ResultEntity, result);
        }

        overlaps.Dispose();
        ecb.RemoveComponent<ClosestPointRequest>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    [BurstCompile]
    private float2 TransformPoint(float2 localPoint, float2 bodyPos, float bodyAngle)
    {
      float cos = math.cos(bodyAngle);
      float sin = math.sin(bodyAngle);
      return bodyPos + new float2(
        localPoint.x * cos - localPoint.y * sin,
        localPoint.x * sin + localPoint.y * cos
      );
    }

    [BurstCompile]
    private float2 ComputeOutwardNormal(
      PhysicsShape shape, PhysicsBody body, float2 closestPoint, float2 queryPoint)
    {
      var pos = body.position;
      var angle = body.rotation.angle;

      switch (shape.shapeType) {
        case PhysicsShape.ShapeType.Circle: {
          var worldCenter = TransformPoint(shape.circleGeometry.center, pos, angle);
          var diff = closestPoint - worldCenter;
          return math.lengthsq(diff) > 1e-10f ? math.normalize(diff) : float2.zero;
        }

        case PhysicsShape.ShapeType.Capsule: {
          var geo = shape.capsuleGeometry;
          var c1 = TransformPoint(geo.center1, pos, angle);
          var c2 = TransformPoint(geo.center2, pos, angle);
          var axis = c2 - c1;
          var axisLenSq = math.lengthsq(axis);
          var t = axisLenSq > 1e-10f
            ? math.saturate(math.dot(closestPoint - c1, axis) / axisLenSq)
            : 0.5f;
          var diff = closestPoint - (c1 + t * axis);
          return math.lengthsq(diff) > 1e-10f ? math.normalize(diff) : float2.zero;
        }

        case PhysicsShape.ShapeType.Polygon: {
          var geo = shape.polygonGeometry;
          float minDistSq = float.MaxValue;
          float2 bestNormal = float2.zero;
          for (int v = 0; v < geo.count; v++) {
            var a = TransformPoint(geo.vertices[v], pos, angle);
            var b = TransformPoint(geo.vertices[(v + 1) % geo.count], pos, angle);
            var edge = b - a;
            var edgeLenSq = math.lengthsq(edge);
            var t = edgeLenSq > 1e-10f
              ? math.saturate(math.dot(closestPoint - a, edge) / edgeLenSq)
              : 0f;
            var proj = a + t * edge;
            var dSq = math.distancesq(closestPoint, proj);
            if (dSq < minDistSq) {
              minDistSq = dSq;
              bestNormal = math.normalize(new float2(edge.y, -edge.x));
            }
          }
          return bestNormal;
        }

        case PhysicsShape.ShapeType.Segment: {
          var geo = shape.segmentGeometry;
          var edge = TransformPoint(geo.point2, pos, angle) - TransformPoint(geo.point1, pos, angle);
          var perp = new float2(edge.y, -edge.x);
          // Segments have no interior; orient the normal toward the query point side.
          var toQuery = queryPoint - closestPoint;
          if (math.dot(perp, toQuery) < 0) perp = -perp;
          return math.lengthsq(perp) > 1e-10f ? math.normalize(perp) : float2.zero;
        }

        case PhysicsShape.ShapeType.ChainSegment: {
          var geo = shape.chainSegmentGeometry;
          var edge = TransformPoint(geo.segment.point2, pos, angle) - TransformPoint(geo.segment.point1, pos, angle);
          var perp = new float2(edge.y, -edge.x);
          var toQuery = queryPoint - closestPoint;
          if (math.dot(perp, toQuery) < 0) perp = -perp;
          return math.lengthsq(perp) > 1e-10f ? math.normalize(perp) : float2.zero;
        }

        default:
          return float2.zero;
      }
    }

    /// <summary>
    /// Workaround for Unity's PolygonGeometry.ClosestPoint() returning the interior
    /// query point when the point is inside the polygon. Projects the query point
    /// onto every edge and returns the nearest projection in world space.
    /// </summary>
    [BurstCompile]
    private float2 SnapPolygonToNearestEdge(
      PhysicsShape shape, PhysicsBody body, float2 queryPoint)
    {
      var geo = shape.polygonGeometry;
      var pos = body.position;
      var angle = body.rotation.angle;
      float minDistSq = float.MaxValue;
      float2 nearest = queryPoint;
      for (int v = 0; v < geo.count; v++) {
        var a = TransformPoint(geo.vertices[v], pos, angle);
        var b = TransformPoint(geo.vertices[(v + 1) % geo.count], pos, angle);
        var edge = b - a;
        var edgeLenSq = math.lengthsq(edge);
        var t = edgeLenSq > 1e-10f
          ? math.saturate(math.dot(queryPoint - a, edge) / edgeLenSq)
          : 0f;
        var proj = a + t * edge;
        var dSq = math.distancesq(queryPoint, proj);
        if (dSq < minDistSq) {
          minDistSq = dSq;
          nearest = proj;
        }
      }
      return nearest;
    }
  }
}