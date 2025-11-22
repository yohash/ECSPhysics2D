using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that processes raycast requests.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(BuildPhysicsWorldSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct RaycastSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var physicsWorld = physicsWorldSingleton.World;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Process raycast requests
      foreach (var (request, entity) in
          SystemAPI.Query<RefRO<RaycastRequest>>()
          .WithEntityAccess()) {

        var input = new PhysicsQuery.CastRayInput(
          request.ValueRO.Origin,
          request.ValueRO.Direction * request.ValueRO.MaxDistance
        );
        var results = physicsWorld.CastRay(input, request.ValueRO.Filter);
        var hit = results != null && results.Length > 0;

        var rayResult = new RaycastResult
        {
          Hit = hit,
          Point = hit ? results[0].point : float2.zero,
          Normal = hit ? results[0].normal : float2.zero,
          Distance = hit ? results[0].fraction * request.ValueRO.MaxDistance : request.ValueRO.MaxDistance,
          Shape = hit ? results[0].shape : default,
          Body = hit ? results[0].shape.body : default,
          HitEntity = hit ? GetEntityFromBody(results[0].shape.body) : Entity.Null
        };

        results.Dispose();

        // Store result on target entity
        if (request.ValueRO.ResultEntity != Entity.Null) {
          ecb.SetComponent(request.ValueRO.ResultEntity, rayResult);
        }

        // Remove request
        ecb.RemoveComponent<RaycastRequest>(entity);
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