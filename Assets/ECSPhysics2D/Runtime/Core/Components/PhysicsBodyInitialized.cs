using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Marks that a physics body has been initialized.
  /// Prevents recreation on subsequent frames.
  /// </summary>
  public struct PhysicsBodyInitialized : IComponentData { }
}