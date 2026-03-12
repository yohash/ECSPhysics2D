namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Predefined collision layers for common game objects.
  /// Use these with CollisionFilter.Create() for consistency.
  /// </summary>
  public static class FallingShapesCollisionLayers
  {
    public const int Default = 0;
    public const int Player = 1;
    public const int Enemy = 2;
    public const int PlayerProjectile = 3;
    public const int EnemyProjectile = 4;
    public const int Terrain = 5;
    public const int Trigger = 6;
    public const int Debris = 7;
    public const int Item = 8;
    public const int Platform = 9;
    public const int OneWayPlatform = 10;
    public const int Water = 11;
    public const int Sensor = 12;
    public const int Buildings = 13;

    // Helper methods to create common filters
    public static class Filters
    {
      public static readonly CollisionFilter Player = CollisionFilter.Create(
          FallingShapesCollisionLayers.Player,
          FallingShapesCollisionLayers.Enemy,
          FallingShapesCollisionLayers.EnemyProjectile,
          FallingShapesCollisionLayers.Terrain,
          FallingShapesCollisionLayers.Trigger,
          FallingShapesCollisionLayers.Item,
          FallingShapesCollisionLayers.Platform,
          FallingShapesCollisionLayers.OneWayPlatform,
          FallingShapesCollisionLayers.Water,
          FallingShapesCollisionLayers.Sensor
      );

      public static readonly CollisionFilter Enemy = CollisionFilter.Create(
          FallingShapesCollisionLayers.Enemy,
          FallingShapesCollisionLayers.Player,
          FallingShapesCollisionLayers.PlayerProjectile,
          FallingShapesCollisionLayers.Terrain,
          FallingShapesCollisionLayers.Platform
      );

      public static readonly CollisionFilter Debris = CollisionFilter.Create(
          FallingShapesCollisionLayers.Debris,
          FallingShapesCollisionLayers.Terrain,
          FallingShapesCollisionLayers.Platform
      // Note: Debris doesn't collide with other debris for performance
      );

      public static readonly CollisionFilter ExplosionTargets = CollisionFilter.Create(
          FallingShapesCollisionLayers.Debris,
          FallingShapesCollisionLayers.Terrain,
          FallingShapesCollisionLayers.Player,
          FallingShapesCollisionLayers.Enemy,
          FallingShapesCollisionLayers.Buildings
      // Note: Debris doesn't collide with other debris for performance
      );

      public static readonly CollisionFilter Trigger = CollisionFilter.Create(
          FallingShapesCollisionLayers.Trigger,
          FallingShapesCollisionLayers.Player,
          FallingShapesCollisionLayers.Enemy
      );
    }
  }
}