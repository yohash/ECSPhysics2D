using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Burst-compiled job for parallel raycasting.
  /// </summary>
  [BurstCompile]
  public struct ParallelRaycastJob : IJobParallelFor
  {
    [ReadOnly] public PhysicsWorld PhysicsWorld;
    [ReadOnly] public NativeArray<float2> Origins;
    [ReadOnly] public NativeArray<float2> Directions;
    [ReadOnly] public float MaxDistance;
    [ReadOnly] public PhysicsQuery.QueryFilter Filter;

    [WriteOnly] public NativeArray<RaycastResult> Results;

    public void Execute(int index)
    {
      var input = new PhysicsQuery.CastRayInput(
        Origins[index],
        Directions[index] * MaxDistance
      );
      var results = PhysicsWorld.CastRay(input, Filter);
      var hit = results != null && results.Length > 0;

      // default to the first hit result
      Results[index] = new RaycastResult
      {
        Hit = hit,
        Point = hit ? results[0].point : float2.zero,
        Normal = hit ? results[0].normal : float2.zero,
        Distance = hit ? results[0].fraction * MaxDistance : MaxDistance,
        Shape = hit ? results[0].shape : default,
        Body = hit ? results[0].shape.body : default,
        HitEntity = hit ? GetEntityFromBody(results[0].shape.body) : Entity.Null
      };

      results.Dispose();

      // TODO - verify that we're populating the Results array correctly in parallel
      // TBD - compare with RaycastSystem implementation... do we need to process results
      // afterwards and write to an entity command buffer?
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