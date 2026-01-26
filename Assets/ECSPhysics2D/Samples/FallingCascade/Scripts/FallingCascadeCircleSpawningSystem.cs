using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.FallingCascade
{
  /// <summary>
  /// Spawns circles from the top at configured rate.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
  [BurstCompile]
  public partial struct FallingCascadeCircleSpawningSystem : ISystem
  {
    private Random random;

    public void OnCreate(ref SystemState state)
    {
      random = new Random((uint)System.DateTime.Now.Ticks);
      state.RequireForUpdate<CascadeCircleSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<CascadeCircleSpawner>(out var spawner))
        return;

      var deltaTime = SystemAPI.Time.DeltaTime;
      spawner.ValueRW.TimeSinceLastSpawn += deltaTime;

      // Check if we should spawn
      if (spawner.ValueRW.TimeSinceLastSpawn < spawner.ValueRO.SpawnRate)
        return;

      // Check circle count limit
      var circleQuery = SystemAPI.QueryBuilder()
        .WithAll<CascadeCircleTag>()
        .Build();

      if (circleQuery.CalculateEntityCount() >= spawner.ValueRO.MaxCircles)
        return;

      // Reset timer
      spawner.ValueRW.TimeSinceLastSpawn = 0f;

      // Spawn circle
      SpawnCircle(ref state, spawner.ValueRO);
    }

    private void SpawnCircle(ref SystemState state, CascadeCircleSpawner config)
    {
      var circleEntity = state.EntityManager.CreateEntity();

      // Random X position within range
      float x = random.NextFloat(-config.SpawnRangeX, config.SpawnRangeX);

      // Transform
      state.EntityManager.AddComponentData(circleEntity,
        LocalTransform.FromPosition(x, config.SpawnHeight, 0f));

      // Physics body (uninitialized)
      state.EntityManager.AddComponentData(circleEntity, new PhysicsBodyComponent
      {
        WorldIndex = 0,
        GravityScale = 1f
      });

      // Dynamic tag
      state.EntityManager.AddComponent<PhysicsDynamicTag>(circleEntity);

      // Circle shape
      var shape = new PhysicsShapeCircle
      {
        Radius = config.CircleRadius,
        Center = float2.zero
      };
      state.EntityManager.AddComponentData(circleEntity, shape);
      state.EntityManager.AddBuffer<PhysicsShapeReference>(circleEntity).Add(new PhysicsShapeReference
      {
        //Shape = shape
      });

      // Material - bouncy for nice arcs
      state.EntityManager.AddComponentData(circleEntity, new PhysicsMaterial
      {
        Friction = 0.2f,
        Bounciness = 0.5f,
        Density = 1f,
        RollingResistance = 0.05f
      });

      // Collision filter - starts colliding with all terrain layers
      state.EntityManager.AddComponentData(circleEntity,
        FallingCascadeCollisionLayers.Filters.CascadeCircleInitial.WithCollisionEvents());

      // Tag and buffer
      state.EntityManager.AddComponent<CascadeCircleTag>(circleEntity);
      state.EntityManager.AddBuffer<TerrainLayersHit>(circleEntity);
    }
  }
}