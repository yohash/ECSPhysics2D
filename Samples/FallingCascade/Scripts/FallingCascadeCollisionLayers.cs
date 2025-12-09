namespace ECSPhysics2D.Samples.FallingCascade
{
  /// <summary>
  /// Predefined collision layers for common game objects.
  /// Use these with CollisionFilter.Create() for consistency.
  /// </summary>
  public static class FallingCascadeCollisionLayers
  {
    public const int CascadeCircle = 0;
    public const int TerrainLayer1 = 1;
    public const int TerrainLayer2 = 2;
    public const int TerrainLayer3 = 3;

    // Helper methods to create common filters
    public static class Filters
    {
      // Add to Filters class (after ExplosionTargets):
      public static readonly CollisionFilter TerrainLayer1Filter = CollisionFilter.Create(
        TerrainLayer1,
        CascadeCircle
      );

      public static readonly CollisionFilter TerrainLayer2Filter = CollisionFilter.Create(
        TerrainLayer2,
        CascadeCircle
      );

      public static readonly CollisionFilter TerrainLayer3Filter = CollisionFilter.Create(
        TerrainLayer3,
        CascadeCircle
      );

      public static readonly CollisionFilter CascadeCircleInitial = CollisionFilter.Create(
        CascadeCircle,
        CascadeCircle,
        TerrainLayer1,
        TerrainLayer2,
        TerrainLayer3
      );
    }
  }
}