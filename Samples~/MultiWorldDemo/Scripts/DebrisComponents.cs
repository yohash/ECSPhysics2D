using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Tag identifying debris entities for lifetime management.
  /// </summary>
  public struct DebrisTag : IComponentData
  {
    public float SpawnTime;
    public float Lifetime;

    public bool IsExpired(float currentTime)
    {
      return (currentTime - SpawnTime) >= Lifetime;
    }
  }

  /// <summary>
  /// Request to spawn debris at a position.
  /// Processed by DebrisSpawnSystem and then removed.
  /// </summary>
  public struct DebrisSpawnRequest : IComponentData
  {
    public float2 Position;
    public int Count;
    public float Radius;
    public float SpreadSpeed;
    public float Lifetime;
    public int TargetWorldIndex;
  }
}
