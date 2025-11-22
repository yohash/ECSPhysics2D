using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component to request shape updates at runtime.
  /// </summary>
  public struct ShapeUpdateRequest : IComponentData
  {
    public enum UpdateType : byte
    {
      Recreate,      // Destroy and recreate all shapes
      ModifyMaterial // Just update material properties
    }

    public UpdateType Type;
  }
}