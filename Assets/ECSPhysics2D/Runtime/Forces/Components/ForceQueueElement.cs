using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Buffer for queuing multiple forces to apply.
  /// Useful for complex multi-force scenarios.
  /// </summary>
  [InternalBufferCapacity(4)]
  public struct ForceQueueElement : IBufferElementData
  {
    public float2 Force;
    public float2 Point;
    public bool UseWorldPoint;
    public float Delay;           // Apply after this many seconds
  }
}