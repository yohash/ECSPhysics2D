using Unity.Entities;

namespace ECSPhysics2D
{

  /// <summary>
  /// Tag indicating shapes have been created for this body.
  /// </summary>
  public struct ShapesCreatedTag : IComponentData
  {
    public int ShapeCount;
  }
}