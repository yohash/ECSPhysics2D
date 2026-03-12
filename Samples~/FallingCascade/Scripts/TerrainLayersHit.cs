using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingCascade
{
  /// <summary>
  /// Buffer tracking which terrain layers this circle has collided with.
  /// </summary>
  [InternalBufferCapacity(3)]
  public struct TerrainLayersHit : IBufferElementData
  {
    public int LayerIndex;  // 14, 15, or 16
  }
}