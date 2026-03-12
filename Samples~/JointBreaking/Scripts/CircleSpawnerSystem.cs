using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Spawns falling circles at regular intervals.
  /// Circles have varying size and mass proportional to area.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [BurstCompile]
  public partial struct CircleSpawnerSystem : ISystem
  {
    private Random random;

    public void OnCreate(ref SystemState state)
    {
      random = new Random((uint)System.DateTime.Now.Ticks);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      var deltaTime = SystemAPI.Time.DeltaTime;
      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      foreach (var spawner in SystemAPI.Query<RefRW<CircleSpawnerConfig>>()) {
        spawner.ValueRW.TimeUntilNextSpawn -= deltaTime;

        if (spawner.ValueRO.TimeUntilNextSpawn <= 0f) {
          // Check current circle count
          var currentCount = SystemAPI.QueryBuilder()
              .WithAll<SpawnedCircleTag>()
              .Build()
              .CalculateEntityCount();

          if (currentCount < spawner.ValueRO.MaxCircles) {
            SpawnCircle(ref state, ecb, ref spawner.ValueRW);
            spawner.ValueRW.TimeUntilNextSpawn = spawner.ValueRO.SpawnInterval;
          }
        }
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void SpawnCircle(ref SystemState state, EntityCommandBuffer ecb, ref CircleSpawnerConfig config)
    {
      // Random spawn position
      float randomX = random.NextFloat(-config.SpawnRangeX, config.SpawnRangeX);
      float3 spawnPosition = new float3(randomX, config.SpawnHeight, 0f);

      // Random radius
      float radius = random.NextFloat(config.RadiusRange.x, config.RadiusRange.y);

      // Calculate mass from area: mass = density * πr²
      float area = math.PI * radius * radius;
      float mass = config.Density * area;

      // Create entity
      var entity = ecb.CreateEntity();

      // Transform
      ecb.AddComponent(entity, new LocalTransform
      {
        Position = spawnPosition,
        Rotation = quaternion.identity,
        Scale = 1f
      });

      // Physics body
      ecb.AddComponent(entity, new PhysicsBodyComponent
      {
        Body = default,
        WorldIndex = 0,
        GravityScale = 1f
      });
      ecb.AddComponent<PhysicsDynamicTag>(entity);

      // Circle shape
      ecb.AddComponent(entity, new PhysicsShapeCircle
      {
        Center = float2.zero,
        Radius = radius
      });

      // Material
      ecb.AddComponent(entity, new PhysicsMaterial
      {
        Friction = 0.4f,
        Bounciness = 0.3f,
        Density = config.Density,
        RollingResistance = 0.01f
      });

      // Collision filter (collide with everything)
      ecb.AddComponent(entity, CollisionFilter.Create(1, 0, 1).WithCollisionEvents());

      // Tag
      ecb.AddComponent<SpawnedCircleTag>(entity);
    }
  }
}
