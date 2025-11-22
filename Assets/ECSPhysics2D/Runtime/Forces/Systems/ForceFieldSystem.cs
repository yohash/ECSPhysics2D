using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that handles force fields - area effects that apply forces to bodies within.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExplosionSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ForceFieldSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      // Process each force field
      foreach (var (forceField, fieldTransform) in
          SystemAPI.Query<RefRO<PhysicsForceField>, RefRO<LocalTransform>>()) {
        var field = forceField.ValueRO;
        var fieldPos = fieldTransform.ValueRO.Position.xy;

        // Update field center based on entity position
        field.Center = fieldPos;

        // Apply forces to all affected bodies
        foreach (var (body, transform, entity) in
            SystemAPI.Query<RefRO<PhysicsBodyComponent>, RefRO<LocalTransform>>()
            .WithAll<PhysicsDynamicTag, AffectedByForceFields>()
            .WithEntityAccess()) {
          if (!body.ValueRO.IsValid)
            continue;

          var bodyPos = transform.ValueRO.Position.xy;

          // Check if body is in range and affected by this field's layers
          if (!field.InRange(bodyPos))
            continue;

          // Check layer filtering (simplified - would need actual layer from entity)
          // In production, you'd get the CollisionFilter component

          var force = field.CalculateForce(bodyPos);

          if (math.lengthsq(force) > 0.001f) {
            body.ValueRO.Body.ApplyForceToCenter(force);
          }

          // Apply damping for damping fields
          if (field.Type == PhysicsForceField.FieldType.Damping) {
            var physicsBody = body.ValueRO.Body;
            physicsBody.linearVelocity *= 1f - field.Strength * SystemAPI.Time.DeltaTime;
            physicsBody.angularVelocity *= 1f - field.Strength * SystemAPI.Time.DeltaTime;
          }
        }
      }
    }
  }
}