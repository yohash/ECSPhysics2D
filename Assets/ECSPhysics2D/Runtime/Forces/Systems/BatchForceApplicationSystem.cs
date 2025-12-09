using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that applies forces to multiple bodies efficiently.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(BuildPhysicsWorldSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct BatchForceApplicationSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      var deltaTime = SystemAPI.Time.DeltaTime;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Apply continuous forces
      ApplyContinuousForces(ref state, deltaTime, ecb);

      // Apply impulses
      ApplyImpulses(ref state, ecb);

      // Apply torques
      ApplyTorques(ref state, deltaTime, ecb);

      // Apply angular impulses
      ApplyAngularImpulses(ref state, ecb);

      // Process force queues
      ProcessForceQueues(ref state, deltaTime, ecb);

      // Process impulse queues
      ProcessImpulseQueues(ref state, deltaTime, ecb);

      // Clear forces on request
      ClearForces(ref state, ecb);

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void ApplyContinuousForces(ref SystemState state, float deltaTime, EntityCommandBuffer ecb)
    {
      foreach (var (body, force, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRW<PhysicsForce>>()
          .WithAll<PhysicsDynamicTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        var physicsBody = body.ValueRO.Body;

        if (force.ValueRO.UseWorldPoint) {
          physicsBody.ApplyForce(force.ValueRO.Force, force.ValueRO.Point);
        } else {
          physicsBody.ApplyForceToCenter(force.ValueRO.Force);
        }

        // Update duration
        if (force.ValueRO.Duration > 0) {
          force.ValueRW.TimeRemaining -= deltaTime;
          if (force.ValueRW.TimeRemaining <= 0) {
            ecb.RemoveComponent<PhysicsForce>(entity);
          }
        }
      }
    }

    private void ApplyImpulses(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, impulse, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRO<PhysicsImpulse>>()
          .WithAll<PhysicsDynamicTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        var physicsBody = body.ValueRO.Body;

        if (impulse.ValueRO.UseWorldPoint) {
          physicsBody.ApplyLinearImpulse(impulse.ValueRO.Impulse, impulse.ValueRO.Point);
        } else {
          physicsBody.ApplyLinearImpulseToCenter(impulse.ValueRO.Impulse);
        }

        // Remove impulse component after applying (one-shot)
        ecb.RemoveComponent<PhysicsImpulse>(entity);
      }
    }

    private void ApplyTorques(ref SystemState state, float deltaTime, EntityCommandBuffer ecb)
    {
      foreach (var (body, torque, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRW<PhysicsTorque>>()
          .WithAll<PhysicsDynamicTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        body.ValueRO.Body.ApplyTorque(torque.ValueRO.Torque);

        // Update duration
        if (torque.ValueRO.Duration > 0) {
          torque.ValueRW.TimeRemaining -= deltaTime;
          if (torque.ValueRW.TimeRemaining <= 0) {
            ecb.RemoveComponent<PhysicsTorque>(entity);
          }
        }
      }
    }

    private void ApplyAngularImpulses(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, angularImpulse, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRO<PhysicsAngularImpulse>>()
          .WithAll<PhysicsDynamicTag>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        body.ValueRO.Body.ApplyAngularImpulse(angularImpulse.ValueRO.Impulse);

        // Remove after applying (one-shot)
        ecb.RemoveComponent<PhysicsAngularImpulse>(entity);
      }
    }

    private void ProcessForceQueues(ref SystemState state, float deltaTime, EntityCommandBuffer ecb)
    {
      var queueEntities = SystemAPI.QueryBuilder()
        .WithAll<PhysicsBodyComponent, PhysicsDynamicTag>()
        .WithAllRW<ForceQueueElement>()
        .Build()
        .ToEntityArray(Allocator.Temp);

      foreach (var entity in queueEntities) {
        var body = SystemAPI.GetComponent<PhysicsBodyComponent>(entity);
        if (!body.IsValid)
          continue;

        var physicsBody = body.Body;
        var forceQueue = SystemAPI.GetBuffer<ForceQueueElement>(entity);

        for (int i = forceQueue.Length - 1; i >= 0; i--) {
          var element = forceQueue[i];
          element.Delay -= deltaTime;

          if (element.Delay <= 0) {
            if (element.UseWorldPoint) {
              physicsBody.ApplyForce(element.Force, element.Point);
            } else {
              physicsBody.ApplyForceToCenter(element.Force);
            }

            forceQueue.RemoveAt(i);
          } else {
            forceQueue[i] = element;
          }
        }
      }

      queueEntities.Dispose();
    }

    private void ProcessImpulseQueues(ref SystemState state, float deltaTime, EntityCommandBuffer ecb)
    {
      var queueEntities = SystemAPI.QueryBuilder()
        .WithAll<PhysicsBodyComponent, PhysicsDynamicTag>()
        .WithAllRW<ImpulseQueueElement>()
        .Build()
        .ToEntityArray(Allocator.Temp);

      foreach (var entity in queueEntities) {
        var body = SystemAPI.GetComponent<PhysicsBodyComponent>(entity);
        if (!body.IsValid)
          continue;

        var physicsBody = body.Body;
        var impulseQueue = SystemAPI.GetBuffer<ImpulseQueueElement>(entity);

        for (int i = impulseQueue.Length - 1; i >= 0; i--) {
          var element = impulseQueue[i];
          element.Delay -= deltaTime;

          if (element.Delay <= 0) {
            if (element.UseWorldPoint) {
              physicsBody.ApplyLinearImpulse(element.Impulse, element.Point);
            } else {
              physicsBody.ApplyLinearImpulseToCenter(element.Impulse);
            }

            impulseQueue.RemoveAt(i);
          } else {
            impulseQueue[i] = element;
          }
        }
      }

      queueEntities.Dispose();
    }

    private void ClearForces(ref SystemState state, EntityCommandBuffer ecb)
    {
      foreach (var (body, entity) in
          SystemAPI.Query<RefRO<PhysicsBodyComponent>>()
          .WithAll<ClearForcesRequest>()
          .WithEntityAccess()) {
        if (!body.ValueRO.IsValid)
          continue;

        // Clear all velocities
        var physicsBody = body.ValueRO.Body;
        physicsBody.linearVelocity = float2.zero;
        physicsBody.angularVelocity = 0f;

        // Remove all force-related components
        ecb.RemoveComponent<PhysicsForce>(entity);
        ecb.RemoveComponent<PhysicsImpulse>(entity);
        ecb.RemoveComponent<PhysicsTorque>(entity);
        ecb.RemoveComponent<PhysicsAngularImpulse>(entity);
        ecb.RemoveComponent<ClearForcesRequest>(entity);
      }
    }
  }
}