using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Request component to trigger shape creation.
  /// </summary>
  public struct ShapeCreationRequest : IComponentData
  {
    public enum CreationType : byte
    {
      Single,    // Create from single shape component
      Compound   // Create from CompoundShapeElement buffer
    }

    public CreationType Type;
  }
}