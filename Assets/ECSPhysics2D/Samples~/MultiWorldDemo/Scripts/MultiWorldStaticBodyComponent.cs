using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Component for static bodies that exist in multiple physics worlds.
  /// Each world gets its own independent PhysicsBody handle.
  /// 
  /// Use WorldMask to specify which worlds should contain this body.
  /// Bodies are created during initialization and never updated (static).
  /// </summary>
  public struct MultiWorldStaticBodyComponent : IComponentData
  {
    /// <summary>
    /// Bitmask indicating which worlds contain this body.
    /// Bit 0 = World 0, Bit 1 = World 1, etc.
    /// </summary>
    public byte WorldMask;

    /// <summary>
    /// Body handle for World 0 (if bit 0 set in WorldMask).
    /// </summary>
    public PhysicsBody World0Body;

    /// <summary>
    /// Body handle for World 1 (if bit 1 set in WorldMask).
    /// </summary>
    public PhysicsBody World1Body;

    /// <summary>
    /// Body handle for World 2 (if bit 2 set in WorldMask).
    /// </summary>
    public PhysicsBody World2Body;

    /// <summary>
    /// Body handle for World 3 (if bit 3 set in WorldMask).
    /// </summary>
    public PhysicsBody World3Body;

    public bool ExistsInWorld(int worldIndex)
    {
      return (WorldMask & (1 << worldIndex)) != 0;
    }

    public PhysicsBody GetBodyForWorld(int worldIndex)
    {
      return worldIndex switch
      {
        0 => World0Body,
        1 => World1Body,
        2 => World2Body,
        3 => World3Body,
        _ => default
      };
    }

    public void SetBodyForWorld(int worldIndex, PhysicsBody body)
    {
      switch (worldIndex) {
        case 0:
          World0Body = body;
          break;
        case 1:
          World1Body = body;
          break;
        case 2:
          World2Body = body;
          break;
        case 3:
          World3Body = body;
          break;
      }
    }

    public static MultiWorldStaticBodyComponent CreateForWorlds(params int[] worldIndices)
    {
      byte mask = 0;
      foreach (var idx in worldIndices) {
        if (idx >= 0 && idx < 8) {
          mask |= (byte)(1 << idx);
        }
      }
      return new MultiWorldStaticBodyComponent { WorldMask = mask };
    }
  }

  /// <summary>
  /// Tag indicating MultiWorldStaticBodyComponent has been initialized.
  /// </summary>
  public struct MultiWorldStaticBodyInitialized : IComponentData { }
}
