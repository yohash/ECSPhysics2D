using System;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Collision filter determines what can collide with what.
  /// Uses category bits and mask bits for efficient filtering.
  /// </summary>
  public struct CollisionFilter : IComponentData, IEquatable<CollisionFilter>
  {
    /// <summary>
    /// The collision categories this shape belongs to (up to 32 categories)
    /// </summary>
    public uint CategoryBits;

    /// <summary>
    /// The categories this shape can collide with
    /// </summary>
    public uint MaskBits;

    /// <summary>
    /// Group index for special filtering:
    /// - Negative: Never collide with same group
    /// - Positive: Always collide with same group
    /// - Zero: Use category/mask filtering
    /// </summary>
    public short GroupIndex;

    public PhysicsMask Categories()
    {
      return new PhysicsMask
      {
        bitMask = CategoryBits
      };
    }

    public PhysicsMask Mask()
    {
      return new PhysicsMask
      {
        bitMask = MaskBits
      };
    }

    /// <summary>
    /// Checks if two filters should collide
    /// </summary>
    public bool ShouldCollide(CollisionFilter other)
    {
      // Group index rules take precedence
      if (GroupIndex != 0 && GroupIndex == other.GroupIndex) {
        return GroupIndex > 0; // Positive = always collide, negative = never
      }

      // Category/mask filtering
      bool collideAB = (CategoryBits & other.MaskBits) != 0;
      bool collideBA = (other.CategoryBits & MaskBits) != 0;
      return collideAB && collideBA;
    }

    public bool Equals(CollisionFilter other)
    {
      return CategoryBits == other.CategoryBits &&
             MaskBits == other.MaskBits &&
             GroupIndex == other.GroupIndex;
    }

    /// <summary>
    /// Default filter that collides with everything
    /// </summary>
    public static CollisionFilter Default => new CollisionFilter
    {
      CategoryBits = 0x00000001,
      MaskBits = 0xFFFFFFFF,
      GroupIndex = 0
    };

    /// <summary>
    /// Creates a filter for a specific layer that collides with specified layers
    /// </summary>
    public static CollisionFilter Create(int layer, params int[] collidesWithLayers)
    {
      uint categoryBits = (uint)(1 << layer);
      uint maskBits = 0;

      foreach (var otherLayer in collidesWithLayers) {
        maskBits |= (uint)(1 << otherLayer);
      }

      return new CollisionFilter
      {
        CategoryBits = categoryBits,
        MaskBits = maskBits,
        GroupIndex = 0
      };
    }
  }
}