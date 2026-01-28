using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Spawns falling circles at regular intervals.
  /// Circles are created in World 0 and will explode into debris (World 1) when triggered.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(BuildPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct CircleSpawnerSystem : ISystem
  {
    private Random random;

    public void OnCreate(ref SystemState state)
    {
      random = new Random(12345);
      state.RequireForUpdate<MultiWorldDemoConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<MultiWorldDemoConfig>(out var configRef))
        return;

      ref var config = ref configRef.ValueRW;

      // Update spawn timer
      config.TimeSinceLastSpawn += SystemAPI.Time.DeltaTime;

      // Check if we should spawn
      if (config.TimeSinceLastSpawn < config.SpawnInterval)
        return;

      if (config.CurrentCircleCount >= config.MaxCircles)
        return;

      config.TimeSinceLastSpawn = 0f;

      // Spawn a circle
      var ecb = new EntityCommandBuffer(Allocator.TempJob);
      var circleEntity = ecb.CreateEntity();

      // Random horizontal offset
      float xOffset = random.NextFloat(-config.CircleSpawnArea, config.CircleSpawnArea);
      var spawnPos = config.SpawnPosition + new float3(xOffset, 0f, 0f);

      // Transform
      ecb.AddComponent(circleEntity, LocalTransform.FromPosition(spawnPos));

      // Physics body (World 0 - main gameplay)
      ecb.AddComponent(circleEntity, new PhysicsBodyComponent
      {
        WorldIndex = 0,
        GravityScale = 1f,
        LinearDamping = 0.1f,
        AngularDamping = 0.1f
      });

      // Dynamic body tag
      ecb.AddComponent<PhysicsDynamicTag>(circleEntity);

      // Circle shape
      ecb.AddComponent(circleEntity, new PhysicsShapeCircle
      {
        Radius = config.CircleRadius,
        Center = float2.zero
      });

      // Material
      ecb.AddComponent(circleEntity, new PhysicsMaterial
      {
        Friction = 0.3f,
        Bounciness = 0.4f,
        Density = 1f
      });

      // Collision filter - circle layer
      ecb.AddComponent(circleEntity, CollisionFilter.CreateFromMask(
        MultiWorldDemoLayers.CircleLayer,
        MultiWorldDemoLayers.CircleCollidesWith
      ));

      // Tags and explosion config
      ecb.AddComponent<FallingCircleTag>(circleEntity);
      ecb.AddComponent(circleEntity, new ExplosionOnContact
      {
        DebrisCount = config.DebrisCount,
        DebrisRadius = config.DebrisRadius,
        SpreadSpeed = config.DebrisSpreadSpeed,
        DebrisLifetime = config.DebrisLifetime
      });

      config.CurrentCircleCount++;

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
