using Unity.Entities;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Tag for falling circles that will explode into debris.
  /// </summary>
  public struct FallingCircleTag : IComponentData { }

  /// <summary>
  /// Configuration for explosion behavior when circle is destroyed.
  /// </summary>
  public struct ExplosionOnContact : IComponentData
  {
    public int DebrisCount;
    public float DebrisRadius;
    public float SpreadSpeed;
    public float DebrisLifetime;

    public static ExplosionOnContact Default => new ExplosionOnContact
    {
      DebrisCount = 12,
      DebrisRadius = 0.15f,
      SpreadSpeed = 5f,
      DebrisLifetime = 4f
    };
  }
}
