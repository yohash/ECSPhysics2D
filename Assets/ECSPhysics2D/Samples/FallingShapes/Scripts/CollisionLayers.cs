namespace ECSPhysics2D
{
  /// <summary>
  /// Predefined collision layers for common game objects.
  /// Use these with CollisionFilter.Create() for consistency.
  /// </summary>
  public static class CollisionLayers
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
          CollisionLayers.Player,
          CollisionLayers.Enemy,
          CollisionLayers.EnemyProjectile,
          CollisionLayers.Terrain,
          CollisionLayers.Trigger,
          CollisionLayers.Item,
          CollisionLayers.Platform,
          CollisionLayers.OneWayPlatform,
          CollisionLayers.Water,
          CollisionLayers.Sensor
      );

      public static readonly CollisionFilter Enemy = CollisionFilter.Create(
          CollisionLayers.Enemy,
          CollisionLayers.Player,
          CollisionLayers.PlayerProjectile,
          CollisionLayers.Terrain,
          CollisionLayers.Platform
      );

      public static readonly CollisionFilter Debris = CollisionFilter.Create(
          CollisionLayers.Debris,
          CollisionLayers.Terrain,
          CollisionLayers.Platform
      // Note: Debris doesn't collide with other debris for performance
      );

      public static readonly CollisionFilter ExplosionTargets = CollisionFilter.Create(
          CollisionLayers.Debris,
          CollisionLayers.Terrain,
          CollisionLayers.Player,
          CollisionLayers.Enemy,
          CollisionLayers.Buildings
      // Note: Debris doesn't collide with other debris for performance
      );

      public static readonly CollisionFilter Trigger = CollisionFilter.Create(
          CollisionLayers.Trigger,
          CollisionLayers.Player,
          CollisionLayers.Enemy
      );
    }
  }
}