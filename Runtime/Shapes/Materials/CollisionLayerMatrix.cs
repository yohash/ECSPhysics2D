using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Singleton component that defines the global collision matrix.
  /// Alternative to per-entity CollisionFilter for simpler setups.
  /// </summary>
  public struct CollisionLayerMatrix : IComponentData
  {
    // 32x32 bit matrix stored as 32 uint values
    // Each uint represents what layers that layer index collides with
    public uint Layer00, Layer01, Layer02, Layer03;
    public uint Layer04, Layer05, Layer06, Layer07;
    public uint Layer08, Layer09, Layer10, Layer11;
    public uint Layer12, Layer13, Layer14, Layer15;
    public uint Layer16, Layer17, Layer18, Layer19;
    public uint Layer20, Layer21, Layer22, Layer23;
    public uint Layer24, Layer25, Layer26, Layer27;
    public uint Layer28, Layer29, Layer30, Layer31;

    public uint GetLayerMask(int layer)
    {
      return layer switch
      {
        0 => Layer00,
        1 => Layer01,
        2 => Layer02,
        3 => Layer03,
        4 => Layer04,
        5 => Layer05,
        6 => Layer06,
        7 => Layer07,
        8 => Layer08,
        9 => Layer09,
        10 => Layer10,
        11 => Layer11,
        12 => Layer12,
        13 => Layer13,
        14 => Layer14,
        15 => Layer15,
        16 => Layer16,
        17 => Layer17,
        18 => Layer18,
        19 => Layer19,
        20 => Layer20,
        21 => Layer21,
        22 => Layer22,
        23 => Layer23,
        24 => Layer24,
        25 => Layer25,
        26 => Layer26,
        27 => Layer27,
        28 => Layer28,
        29 => Layer29,
        30 => Layer30,
        31 => Layer31,
        _ => 0xFFFFFFFF
      };
    }

    public void SetLayerMask(int layer, uint mask)
    {
      switch (layer) {
        case 0:
          Layer00 = mask;
          break;
        case 1:
          Layer01 = mask;
          break;
        case 2:
          Layer02 = mask;
          break;
        case 3:
          Layer03 = mask;
          break;
        case 4:
          Layer04 = mask;
          break;
        case 5:
          Layer05 = mask;
          break;
        case 6:
          Layer06 = mask;
          break;
        case 7:
          Layer07 = mask;
          break;
        case 8:
          Layer08 = mask;
          break;
        case 9:
          Layer09 = mask;
          break;
        case 10:
          Layer10 = mask;
          break;
        case 11:
          Layer11 = mask;
          break;
        case 12:
          Layer12 = mask;
          break;
        case 13:
          Layer13 = mask;
          break;
        case 14:
          Layer14 = mask;
          break;
        case 15:
          Layer15 = mask;
          break;
        case 16:
          Layer16 = mask;
          break;
        case 17:
          Layer17 = mask;
          break;
        case 18:
          Layer18 = mask;
          break;
        case 19:
          Layer19 = mask;
          break;
        case 20:
          Layer20 = mask;
          break;
        case 21:
          Layer21 = mask;
          break;
        case 22:
          Layer22 = mask;
          break;
        case 23:
          Layer23 = mask;
          break;
        case 24:
          Layer24 = mask;
          break;
        case 25:
          Layer25 = mask;
          break;
        case 26:
          Layer26 = mask;
          break;
        case 27:
          Layer27 = mask;
          break;
        case 28:
          Layer28 = mask;
          break;
        case 29:
          Layer29 = mask;
          break;
        case 30:
          Layer30 = mask;
          break;
        case 31:
          Layer31 = mask;
          break;
      }
    }

    public bool ShouldCollide(int layerA, int layerB)
    {
      uint maskA = GetLayerMask(layerA);
      uint maskB = GetLayerMask(layerB);

      bool aCollidesWithB = (maskA & (1u << layerB)) != 0;
      bool bCollidesWithA = (maskB & (1u << layerA)) != 0;

      return aCollidesWithB && bCollidesWithA;
    }

    public void SetCollision(int layerA, int layerB, bool shouldCollide)
    {
      uint maskA = GetLayerMask(layerA);
      uint maskB = GetLayerMask(layerB);

      if (shouldCollide) {
        maskA |= (1u << layerB);
        maskB |= (1u << layerA);
      } else {
        maskA &= ~(1u << layerB);
        maskB &= ~(1u << layerA);
      }

      SetLayerMask(layerA, maskA);
      SetLayerMask(layerB, maskB);
    }

    /// <summary>
    /// Creates a default matrix where everything collides with everything
    /// </summary>
    public static CollisionLayerMatrix CreateDefault()
    {
      var matrix = new CollisionLayerMatrix();
      uint allLayers = 0xFFFFFFFF;

      for (int i = 0; i < 32; i++) {
        matrix.SetLayerMask(i, allLayers);
      }

      return matrix;
    }

    /// <summary>
    /// Creates a typical game collision matrix
    /// </summary>
    public static CollisionLayerMatrix CreateGameDefault()
    {
      var matrix = new CollisionLayerMatrix();

      // Clear all first
      for (int i = 0; i < 32; i++) {
        matrix.SetLayerMask(i, 0);
      }

      // Set up common collisions
      // Player collides with: enemies, enemy projectiles, terrain, items, platforms
      matrix.SetCollision(CollisionLayers.Player, CollisionLayers.Enemy, true);
      matrix.SetCollision(CollisionLayers.Player, CollisionLayers.EnemyProjectile, true);
      matrix.SetCollision(CollisionLayers.Player, CollisionLayers.Terrain, true);
      matrix.SetCollision(CollisionLayers.Player, CollisionLayers.Item, true);
      matrix.SetCollision(CollisionLayers.Player, CollisionLayers.Platform, true);

      // Enemy collides with: player, player projectiles, terrain, platforms
      matrix.SetCollision(CollisionLayers.Enemy, CollisionLayers.PlayerProjectile, true);
      matrix.SetCollision(CollisionLayers.Enemy, CollisionLayers.Terrain, true);
      matrix.SetCollision(CollisionLayers.Enemy, CollisionLayers.Platform, true);

      // Projectiles hit terrain
      matrix.SetCollision(CollisionLayers.PlayerProjectile, CollisionLayers.Terrain, true);
      matrix.SetCollision(CollisionLayers.EnemyProjectile, CollisionLayers.Terrain, true);

      // Debris only collides with terrain (not with other debris)
      matrix.SetCollision(CollisionLayers.Debris, CollisionLayers.Terrain, true);

      return matrix;
    }
  }
}