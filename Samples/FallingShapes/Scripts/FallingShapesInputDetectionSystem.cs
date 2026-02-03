using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Processes UserShapeCreationRequest components, applies throttling via config, and creates SpawnShapeRequest.
  /// Runs before FixedStep so requests are ready for spawning system.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
  public partial struct FallingShapesInputDetectionSystem : ISystem
  {
    private Unity.Mathematics.Random random;

    public void OnCreate(ref SystemState state)
    {
      random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
      state.RequireForUpdate<FallingShapesSampleConfig>();
    }

    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingletonRW<FallingShapesSampleConfig>(out var config))
        return;

      var deltaTime = SystemAPI.Time.DeltaTime;

      var ecb = new EntityCommandBuffer(Allocator.TempJob);

      // Check if any UserShapeCreationRequest exists
      bool hasUserRequest = false;
      foreach (var (_, entity) in SystemAPI.Query<RefRO<UserShapeCreationRequest>>().WithEntityAccess()) {
        hasUserRequest = true;
        ecb.DestroyEntity(entity);
      }

      bool shouldSpawnContinuous = false;

      if (hasUserRequest) {
        config.ValueRW.TimeSinceLastSpawn += deltaTime;

        if (config.ValueRW.TimeSinceLastSpawn >= config.ValueRO.SpawnCooldown) {
          shouldSpawnContinuous = true;
          config.ValueRW.TimeSinceLastSpawn = 0f;
        }
      } else {
        config.ValueRW.TimeSinceLastSpawn = 0f;
      }

      var spawnConfig = config.ValueRO;

      if (shouldSpawnContinuous) {
        SpawnRandomShape(ref state, spawnConfig);
      }

      bool shouldExplodeMouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
      shouldExplodeMouse &= !MouseOverDetector.IsMouseOverUI;

      if (shouldExplodeMouse) {
        var mousePos = GetMouseWorldPosition();
        CreateExplosion(ref state, mousePos);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }

    private void SpawnRandomShape(ref SystemState state, FallingShapesSampleConfig config)
    {
      var angle = random.NextFloat(0f, math.PI * 2f);
      var radius = random.NextFloat(0f, config.SpawnAreaRadius);
      var position = new float2(
          math.cos(angle) * radius,
          config.SpawnHeight
      );

      SpawnShapeAtPosition(ref state, position, config);
    }

    private void SpawnShapeAtPosition(ref SystemState state, float2 position, FallingShapesSampleConfig config)
    {
      var requestEntity = state.EntityManager.CreateEntity();
      var size = random.NextFloat(config.MinSize, config.MaxSize);
      state.EntityManager.AddComponentData(requestEntity, SpawnShapeRequest.CreateRandom(position, size));
    }

    private void CreateExplosion(ref SystemState state, float2 position)
    {
      var explosionEntity = state.EntityManager.CreateEntity();

      state.EntityManager.AddComponentData(explosionEntity, new PhysicsExplosion
      {
        Center = position,
        Radius = 1.5f,
        Force = 10f,
        Falloff = 1f,
        AffectedLayers = FallingShapesCollisionLayers.Filters.ExplosionTargets.CategoryBits
      });
    }

    private float2 GetMouseWorldPosition()
    {
      var camera = Camera.main;
      if (camera == null) {
        camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null) {
          return float2.zero;
        }
      }

      var mousePos = Mouse.current.position.ReadValue();
      var worldPos = camera.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

      return new float2(worldPos.x, worldPos.y);
    }
  }
}
