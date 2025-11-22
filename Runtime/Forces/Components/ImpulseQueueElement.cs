using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Buffer for queuing multiple impulses to apply.
  /// </summary>
  [InternalBufferCapacity(4)]
  public struct ImpulseQueueElement : IBufferElementData
  {
    public float2 Impulse;
    public float2 Point;
    public bool UseWorldPoint;
    public float Delay;           // Apply after this many seconds
  }
}