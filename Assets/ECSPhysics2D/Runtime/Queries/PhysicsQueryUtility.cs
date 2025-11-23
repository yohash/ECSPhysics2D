using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Utility class for creating and managing physics queries.
  /// </summary>
  public static class PhysicsQueryUtility
  {
    /// <summary>
    /// Creates a raycast request entity.
    /// </summary>
    public static Entity CreateRaycast(
      EntityManager em,
      float2 origin,
      float2 direction,
      float distance,
      PhysicsQuery.QueryFilter filter)
    {
      var requestEntity = em.CreateEntity();
      var resultEntity = em.CreateEntity();

      em.AddComponentData(requestEntity, new RaycastRequest
      {
        Origin = origin,
        Direction = math.normalize(direction),
        MaxDistance = distance,
        Filter = filter,
        ResultEntity = resultEntity
      });

      em.AddComponentData(resultEntity, new RaycastResult());

      return resultEntity;
    }

    /// <summary>
    /// Creates a batch of parallel raycasts using jobs.
    /// </summary>
    public static JobHandle ScheduleParallelRaycasts(
      PhysicsWorld physicsWorld,
      NativeArray<float2> origins,
      NativeArray<float2> directions,
      float maxDistance,
      PhysicsQuery.QueryFilter filter,
      NativeArray<RaycastResult> results,
      int batchSize = 32,
      JobHandle dependency = default)
    {
      var job = new ParallelRaycastJob
      {
        PhysicsWorld = physicsWorld,
        Origins = origins,
        Directions = directions,
        MaxDistance = maxDistance,
        Filter = filter,
        Results = results
      };

      return job.Schedule(origins.Length, batchSize, dependency);
    }

    /// <summary>
    /// Creates an overlap query for a circular area.
    /// </summary>
    public static Entity CreateCircleOverlap(
      EntityManager em,
      float2 center,
      float radius,
      PhysicsQuery.QueryFilter filter)
    {
      var requestEntity = em.CreateEntity();
      var resultEntity = em.CreateEntity();

      em.AddComponentData(requestEntity, new OverlapShapeRequest
      {
        Type = OverlapShapeRequest.ShapeType.Circle,
        Position = center,
        Rotation = 0f,
        Size = new float2(radius, 0),
        Filter = filter,
        ResultEntity = resultEntity
      });

      em.AddBuffer<OverlapResult>(resultEntity);

      return resultEntity;
    }

    /// <summary>
    /// Creates an AABB query.
    /// </summary>
    public static Entity CreateAABBQuery(EntityManager em, float2 min, float2 max, PhysicsQuery.QueryFilter filter)
    {
      var requestEntity = em.CreateEntity();
      var resultEntity = em.CreateEntity();

      em.AddComponentData(requestEntity, new AABBQueryRequest
      {
        Min = min,
        Max = max,
        Filter = filter,
        ResultEntity = resultEntity
      });

      em.AddBuffer<OverlapResult>(resultEntity);

      return resultEntity;
    }

    /// <summary>
    /// Helper to create a query filter for specific layers.
    /// </summary>
    public static PhysicsQuery.QueryFilter CreateLayerFilter(params int[] layers)
    {
      uint mask = 0;
      foreach (var layer in layers) {
        mask |= (uint)(1 << layer);
      }

      return new PhysicsQuery.QueryFilter
      {
        // for a one off query, we can set all bits in the 'categories' field
        // so this query "is using" all categories
        categories = 0xFFFFFFFF,
        // only hit the specified layers
        hitCategories = mask,
      };
    }

    /// <summary>
    /// Performs an immediate raycast without going through ECS.
    /// Useful for editor tools or immediate queries.
    /// </summary>
    public static bool RaycastImmediate(
      PhysicsWorld world,
      float2 origin,
      float2 direction,
      float maxDistance,
      out RaycastResult result,
      PhysicsQuery.QueryFilter filter = default)
    {
      var input = new PhysicsQuery.CastRayInput(origin, direction * maxDistance);
      var results = world.CastRay(input, filter);
      var hit = results != null && results.Length > 0;

      result = new RaycastResult
      {
        Hit = hit,
        Point = hit ? results[0].point : float2.zero,
        Normal = hit ? results[0].normal : float2.zero,
        Distance = hit ? results[0].fraction * maxDistance : maxDistance,
        Shape = hit ? results[0].shape : default,
        Body = hit ? results[0].shape.body : default,
        HitEntity = hit ? results[0].shape.body.GetEntityUserData() : Entity.Null
      };

      return hit;
    }

    /// <summary>
    /// Performs an immediate circle overlap query.
    /// </summary>
    public static int OverlapCircleImmediate(
        PhysicsWorld world,
        float2 center,
        float radius,
        ref NativeList<PhysicsBody> results,
        PhysicsQuery.QueryFilter filter = default)
    {
      var geometry = new CircleGeometry
      {
        center = center,
        radius = radius
      };

      var shapeProxy = new PhysicsShape.ShapeProxy(geometry);
      var overlaps = world.OverlapShapeProxy(shapeProxy, filter);

      results.Clear();
      for (int i = 0; i < overlaps.Length; i++) {
        results.Add(overlaps[i].shape.body);
      }

      int count = overlaps.Length;
      overlaps.Dispose();

      return count;
    }
  }
}