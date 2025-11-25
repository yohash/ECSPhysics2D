using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Creates the static ground plane and spawner config on startup.
  /// </summary>
  [UpdateInGroup(typeof(InitializationSystemGroup))]
  public partial struct GroundSetupSystem : ISystem
  {
    private bool initialized;

    public void OnCreate(ref SystemState state)
    {
      initialized = false;
    }

    public void OnUpdate(ref SystemState state)
    {
      if (initialized) {
        state.Enabled = false;
        return;
      }

      // Create spawner config singleton
      var configEntity = state.EntityManager.CreateEntity();
      state.EntityManager.AddComponentData(configEntity, ShapeSpawnerConfig.CreateDefault());

      // Create ground entity
      var groundEntity = state.EntityManager.CreateEntity();

      // Position at origin
      state.EntityManager.AddComponentData(groundEntity, LocalTransform.FromPosition(0f, 0f, 0f));

      // Physics components
      state.EntityManager.AddComponentData(groundEntity, new PhysicsBodyComponent
      {
        WorldIndex = 0
      });

      state.EntityManager.AddComponent<PhysicsStaticTag>(groundEntity);

      // Large box shape for ground (20 units wide, 1 unit tall)
      state.EntityManager.AddComponentData(groundEntity, new PhysicsShapeBox
      {
        Size = new float2(20f, 1f),
        Center = float2.zero,
        Rotation = 0f
      });

      // Material properties
      state.EntityManager.AddComponentData(groundEntity, new PhysicsMaterial
      {
        Friction = 0.6f,
        Bounciness = 0.2f,
        Density = 1f,
        RollingResistance = 0f
      });

      // Collision filter - ground is on Terrain layer
      state.EntityManager.AddComponentData(groundEntity, CollisionFilter.Create(
          CollisionLayers.Terrain,
          CollisionLayers.Default,
          CollisionLayers.Debris
      ));

      initialized = true;
    }
  }
}
