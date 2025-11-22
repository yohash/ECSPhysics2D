using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that processes AABB queries.
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
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var physicsWorld = physicsWorldSingleton.World;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var (request, entity) in
          SystemAPI.Query<RefRO<AABBQueryRequest>>()
          .WithEntityAccess()) {

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
              Entity = GetEntityFromBody(overlaps[i].shape.body)
            });
          }
        }

        overlaps.Dispose();
        ecb.RemoveComponent<AABBQueryRequest>(entity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private Entity GetEntityFromBody(PhysicsBody body)
    {
      if (!body.isValid)
        return Entity.Null;
      var entityIndex = body.userData.intValue;
      return new Entity { Index = entityIndex };
    }
  }
}