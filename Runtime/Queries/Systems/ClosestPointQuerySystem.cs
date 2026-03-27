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
  public partial struct ClosestPointQuerySystem : ISystem
  {
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
        bool found = false;
        float closestDistSq = searchRadius * searchRadius;
        ClosestPointResult result = default;

        for (int i = 0; i < overlaps.Length; i++) {
          var shape = overlaps[i].shape;
          var body = overlaps[i].shape.body;

          // Get shape's closest point to query point          
          var closestPoint = shape.ClosestPoint(request.ValueRO.Point);
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
            found = true;
          }
        }

        // Determine if query point is inside the closest shape to orient the normal correctly.
        // ClosestPoint() always returns a surface point; the normal sign depends on which side we're on.
        if (found) {
          var pointInput = new PhysicsQuery.OverlapPointInput
          {
            Position = request.ValueRO.Point,
            Filter = request.ValueRO.Filter
          };
          var pointHits = new NativeList<PhysicsQuery.OverlapPointHit>(4, Allocator.Temp);
          bool isInside = false;

          if (physicsWorld.OverlapPoint(pointInput, ref pointHits)) {
            for (int j = 0; j < pointHits.Length; j++) {
              if (pointHits[j].shape == result.Shape) {
                isInside = true;
                break;
              }
            }
          }
          pointHits.Dispose();

          // diff = closestPoint - queryPoint
          // Outside: outward normal = normalize(-diff)  Inside: outward normal = normalize(diff)
          var diff = result.ClosestPoint - request.ValueRO.Point;
          var diffLenSq = math.lengthsq(diff);
          result.Normal = diffLenSq > 1e-10f
            ? math.normalize(isInside ? diff : -diff)
            : float2.zero;
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
  }
}