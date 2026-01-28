namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Collision layer definitions for MultiWorldDemo sample.
  /// </summary>
  public static class MultiWorldDemoLayers
  {
    // Layer indices
    public const int TerrainLayer = 0;
    public const int CircleLayer = 1;
    public const int DebrisLayer = 2;

    // Layer masks (bit flags)
    public const uint Terrain = 1u << TerrainLayer;
    public const uint Circle = 1u << CircleLayer;
    public const uint Debris = 1u << DebrisLayer;

    // Collision masks
    public const uint CircleCollidesWith = Terrain | Circle;
    public const uint DebrisCollidesWith = Terrain | Debris;
    public const uint TerrainCollidesWith = Circle | Debris;
  }
}
