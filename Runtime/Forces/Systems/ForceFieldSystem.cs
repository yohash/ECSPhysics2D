using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// System that handles force fields - area effects that apply forces to bodies within.
  ///
  /// Bounded fields (finite OuterRadius) find candidate bodies with one Box2D broadphase
  /// circle query per field, filtered by AffectedLayers. Unbounded fields (e.g. wind)
  /// iterate dynamic bodies and test CollisionFilter.CategoryBits against AffectedLayers.
  /// Each body receives a field's force at most once per step, measured from the body's
  /// live world center of mass.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(ExplosionSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct ForceFieldSystem : ISystem
  {
    private ComponentLookup<PhysicsDynamicTag> dynamicLookup;
    private ComponentLookup<AffectedByForceFields> optInLookup;
    private ComponentLookup<CollisionFilter> filterLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
      dynamicLookup = state.GetComponentLookup<PhysicsDynamicTag>(true);
      optInLookup = state.GetComponentLookup<AffectedByForceFields>(true);
      filterLookup = state.GetComponentLookup<CollisionFilter>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorldSingleton))
        return;

      dynamicLookup.Update(ref state);
      optInLookup.Update(ref state);
      filterLookup.Update(ref state);

      var deltaTime = SystemAPI.Time.DeltaTime;
      var visited = new NativeHashSet<Entity>(64, Allocator.Temp);

      // Process each force field
      foreach (var (forceField, fieldTransform) in
          SystemAPI.Query<RefRO<PhysicsForceField>, RefRO<LocalTransform>>()) {
        var field = forceField.ValueRO;

        // Update field center based on entity position
        field.Center = fieldTransform.ValueRO.Position.xy;

        var worldIndex = physicsWorldSingleton.IsValidWorldIndex(field.WorldIndex) ? field.WorldIndex : 0;

        if (field.IsBounded) {
          // Broadphase query: only shapes overlapping the field's circle on the affected layers
          var physicsWorld = physicsWorldSingleton.GetWorld(worldIndex);
          var shapeProxy = new PhysicsShape.ShapeProxy(new CircleGeometry
          {
            center = field.Center,
            radius = field.OuterRadius
          });
          var filter = new PhysicsQuery.QueryFilter
          {
            categories = 0xFFFFFFFF,
            hitCategories = field.AffectedLayers
          };

          var overlaps = physicsWorld.OverlapShapeProxy(shapeProxy, filter);

          // Results are per shape; a multi-shape body must only receive the force once.
          // Iterate in query order to keep force accumulation deterministic.
          visited.Clear();
          for (int i = 0; i < overlaps.Length; i++) {
            var body = overlaps[i].shape.body;
            if (!body.isValid)
              continue;

            var entity = body.GetEntityUserData();
            if (!dynamicLookup.HasComponent(entity))
              continue;
            if (field.RequireOptInTag && !optInLookup.HasComponent(entity))
              continue;
            if (!visited.Add(entity))
              continue;

            ApplyField(body, field, deltaTime);
          }

          overlaps.Dispose();
        } else {
          // Unbounded fields cannot be a circle query; check every dynamic body's layers instead
          foreach (var (bodyComponent, entity) in
              SystemAPI.Query<RefRO<PhysicsBodyComponent>>()
              .WithAll<PhysicsDynamicTag>()
              .WithEntityAccess()) {
            if (!bodyComponent.ValueRO.IsValid || bodyComponent.ValueRO.WorldIndex != worldIndex)
              continue;
            if (field.RequireOptInTag && !optInLookup.HasComponent(entity))
              continue;

            var categories = filterLookup.TryGetComponent(entity, out var collisionFilter)
              ? collisionFilter.CategoryBits
              : CollisionFilter.Default.CategoryBits;
            if ((categories & field.AffectedLayers.bitMask) == 0)
              continue;

            ApplyField(bodyComponent.ValueRO.Body, field, deltaTime);
          }
        }
      }

      visited.Dispose();
    }

    private static void ApplyField(PhysicsBody body, in PhysicsForceField field, float deltaTime)
    {
      // Distance is measured to the live center of mass, where the force is applied
      float2 position = body.worldCenterOfMass;
      if (!field.InRange(position))
        return;

      if (field.Type == PhysicsForceField.FieldType.Damping) {
        var factor = field.DampingFactor(deltaTime);
        body.linearVelocity *= factor;
        body.angularVelocity *= factor;
        return;
      }

      var force = field.CalculateForce(position);
      if (math.any(force != float2.zero)) {
        body.ApplyForceToCenter(force);
      }
    }
  }
}
