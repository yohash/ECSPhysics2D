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
          var distSq = math.lengthsq(closestPoint - request.ValueRO.Point);

          if (distSq < closestDistSq) {
            closestDistSq = distSq;
            result = new ClosestPointResult
            {
              Found = true,
              ClosestPoint = closestPoint,
              Distance = math.sqrt(distSq),
              Body = body,
              Shape = shape,
              Entity = GetEntityFromBody(body)
            };
            found = true;
          }
        }

        // Store result
        if (request.ValueRO.ResultEntity != Entity.Null && found) {
          ecb.SetComponent(request.ValueRO.ResultEntity, result);
        }

        overlaps.Dispose();
        ecb.RemoveComponent<ClosestPointRequest>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private Entity GetEntityFromBody(PhysicsBody body)
    {
      if (!body.isValid) {
        return Entity.Null;
      }
      var entityIndex = body.userData.intValue;
      return new Entity { Index = entityIndex };
    }
  }
}