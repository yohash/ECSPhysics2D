using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Processes DebrisSpawnRequest components and creates debris entities.
  /// Debris is spawned in the specified target world with radial velocity.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateBefore(typeof(BuildPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct DebrisSpawnSystem : ISystem
  {
    private Random random;

    public void OnCreate(ref SystemState state)
    {
      random = new Random(67890);
      state.RequireForUpdate<MultiWorldDemoConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.TempJob);
      float currentTime = (float)SystemAPI.Time.ElapsedTime;

      // Process all spawn requests
      foreach (var (request, requestEntity) in
          SystemAPI.Query<RefRO<DebrisSpawnRequest>>()
          .WithEntityAccess()) {
        SpawnDebris(ref ecb, request.ValueRO, currentTime);
        ecb.DestroyEntity(requestEntity);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void SpawnDebris(ref EntityCommandBuffer ecb, DebrisSpawnRequest request, float currentTime)
    {
      float angleStep = math.PI * 2f / request.Count;

      for (int i = 0; i < request.Count; i++) {
        var debrisEntity = ecb.CreateEntity();

        // Calculate radial direction with slight randomization
        float angle = angleStep * i + random.NextFloat(-0.2f, 0.2f);
        float2 direction = new float2(math.cos(angle), math.sin(angle));

        // Slight random offset from center
        float2 offset = direction * random.NextFloat(0.1f, 0.3f);
        float3 spawnPos = new float3(request.Position + offset, 0f);

        // Radial velocity with randomization
        float speed = request.SpreadSpeed * random.NextFloat(0.7f, 1.3f);
        float2 velocity = direction * speed;

        // Add upward bias so debris arcs
        velocity.y += random.NextFloat(2f, 4f);

        // Transform
        ecb.AddComponent(debrisEntity, LocalTransform.FromPosition(spawnPos));

        // Physics body (target world - typically World 1)
        ecb.AddComponent(debrisEntity, new PhysicsBodyComponent
        {
          WorldIndex = request.TargetWorldIndex,
          GravityScale = 1f,
          LinearDamping = 0.5f,
          AngularDamping = 0.3f,
          InitialLinearVelocity = velocity,
          InitialAngularVelocity = random.NextFloat(-10f, 10f)
        });

        // Dynamic body tag
        ecb.AddComponent<PhysicsDynamicTag>(debrisEntity);

        // Circle shape
        ecb.AddComponent(debrisEntity, new PhysicsShapeCircle
        {
          Radius = request.Radius,
          Center = float2.zero
        });

        // Material - bouncy debris
        ecb.AddComponent(debrisEntity, new PhysicsMaterial
        {
          Friction = 0.5f,
          Bounciness = 0.6f,
          Density = 0.5f
        });

        // Collision filter - debris layer
        ecb.AddComponent(debrisEntity, CollisionFilter.CreateFromMask(
          MultiWorldDemoLayers.DebrisLayer,
          MultiWorldDemoLayers.DebrisCollidesWith
        ));

        // Debris tag for lifetime management
        ecb.AddComponent(debrisEntity, new DebrisTag
        {
          SpawnTime = currentTime,
          Lifetime = request.Lifetime
        });
      }
    }
  }
}
