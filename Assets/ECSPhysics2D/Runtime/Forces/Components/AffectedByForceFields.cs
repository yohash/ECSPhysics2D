using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Tag to mark entities that should be affected by force fields.
  /// Opt-in for performance - not all bodies need force field checks.
  /// </summary>
  public struct AffectedByForceFields : IComponentData { }

}