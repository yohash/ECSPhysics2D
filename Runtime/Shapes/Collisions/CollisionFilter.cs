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

    /// <summary>
    /// If true, collision events will be generated for this shape
    /// Objects must be kinetic or dynamic to generate events
    /// </summary>
    public bool GenerateCollisionEvents;

    /// <summary>
    /// If true, trigger events will be generated for this shape
    /// </summary>
    public bool GenerateTriggerEvents;

    public PhysicsShape.ContactFilter ToContactFilter()
    {
      return new PhysicsShape.ContactFilter
      {
        categories = CategoryBits,
        contacts = MaskBits,
        groupIndex = GroupIndex
      };
    }

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

    // Family of Create() overloads for common cases to avoid allocations,
    // these are burst-friendly

    // Single layer collision
    public static CollisionFilter Create(int layer, int collidesWith)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith),
        GroupIndex = 0
      };
    }

    // Two layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2),
        GroupIndex = 0
      };
    }

    // Three layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3),
        GroupIndex = 0
      };
    }

    // Four layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3, int collidesWith4)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3) | (uint)(1 << collidesWith4),
        GroupIndex = 0
      };
    }

    // Five layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3, int collidesWith4, int collidesWith5)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3) | (uint)(1 << collidesWith4) | (uint)(1 << collidesWith5),
        GroupIndex = 0
      };
    }

    // Six layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3, int collidesWith4, int collidesWith5, int collidesWith6)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3) | (uint)(1 << collidesWith4) | (uint)(1 << collidesWith5) | (uint)(1 << collidesWith6),
        GroupIndex = 0
      };
    }

    // Seven layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3, int collidesWith4, int collidesWith5, int collidesWith6, int collidesWith7)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3) | (uint)(1 << collidesWith4) | (uint)(1 << collidesWith5) | (uint)(1 << collidesWith6) | (uint)(1 << collidesWith7),
        GroupIndex = 0
      };
    }

    // Eight layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3, int collidesWith4, int collidesWith5, int collidesWith6, int collidesWith7, int collidesWith8)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3) | (uint)(1 << collidesWith4) | (uint)(1 << collidesWith5) | (uint)(1 << collidesWith6) | (uint)(1 << collidesWith7) | (uint)(1 << collidesWith8),
        GroupIndex = 0
      };
    }

    // Nine layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3, int collidesWith4, int collidesWith5, int collidesWith6, int collidesWith7, int collidesWith8, int collidesWith9)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3) | (uint)(1 << collidesWith4) | (uint)(1 << collidesWith5) | (uint)(1 << collidesWith6) | (uint)(1 << collidesWith7) | (uint)(1 << collidesWith8) | (uint)(1 << collidesWith9),
        GroupIndex = 0
      };
    }

    // Ten layers
    public static CollisionFilter Create(int layer, int collidesWith1, int collidesWith2, int collidesWith3, int collidesWith4, int collidesWith5, int collidesWith6, int collidesWith7, int collidesWith8, int collidesWith9, int collidesWith10)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = (uint)(1 << collidesWith1) | (uint)(1 << collidesWith2) | (uint)(1 << collidesWith3) | (uint)(1 << collidesWith4) | (uint)(1 << collidesWith5) | (uint)(1 << collidesWith6) | (uint)(1 << collidesWith7) | (uint)(1 << collidesWith8) | (uint)(1 << collidesWith9) | (uint)(1 << collidesWith10),
        GroupIndex = 0
      };
    }

    // Direct mask for complex cases
    public static CollisionFilter CreateFromMask(int layer, uint collisionMask)
    {
      return new CollisionFilter
      {
        CategoryBits = (uint)(1 << layer),
        MaskBits = collisionMask,
        GroupIndex = 0
      };
    }
  }

  public static class CollisionFilterExtensions
  {
    /// <summary>
    /// Sets the filter to collide with all categories
    /// </summary>
    public static void SetCollideWithAll(this ref CollisionFilter filter)
    {
      filter.MaskBits = 0xFFFFFFFF;
    }
    /// <summary>
    /// Sets the filter to not collide with any categories
    /// </summary>
    public static void SetCollideWithNone(this ref CollisionFilter filter)
    {
      filter.MaskBits = 0x00000000;
    }

    /// <summary>
    /// Sets the filter to generate collision events.
    /// </summary>
    public static CollisionFilter WithCollisionEvents(this CollisionFilter filter, bool enable = true)
    {
      filter.GenerateCollisionEvents = enable;
      return filter;
    }

    /// <summary>
    /// Sets the filter to generate trigger events.
    /// </summary>
    public static CollisionFilter WithTriggerEvents(this CollisionFilter filter, bool enable = true)
    {
      filter.GenerateTriggerEvents = enable;
      return filter;
    }
  }
}