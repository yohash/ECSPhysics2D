using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that processes AABB queries.
  /// 
  /// Each request specifies which physics world to query via WorldIndex.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ShapeOverlapSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct AABBQuerySystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
        return;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (request, entity) in
          SystemAPI.Query<RefRO<AABBQueryRequest>>()
          .WithEntityAccess()) {
        // Get the correct physics world for this query
        var worldIndex = request.ValueRO.WorldIndex;
        if (!singleton.IsValidWorldIndex(worldIndex)) {
          worldIndex = 0;
        }

        var physicsWorld = singleton.GetWorld(worldIndex);

        var aabb = new PhysicsAABB
        {
          lowerBound = request.ValueRO.Min,
          upperBound = request.ValueRO.Max
        };

        var overlaps = physicsWorld.OverlapAABB(aabb, request.ValueRO.Filter);

        // Store results
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
        ecb.RemoveComponent<AABBQueryRequest>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
