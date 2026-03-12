using Unity.Entities;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// Request component indicating the user wants to spawn a shape.
  /// Created by input sources (keyboard, UI buttons).
  /// Consumed by FallingShapesInputDetectionSystem which applies throttling.
  /// </summary>
  public struct UserShapeCreationRequest : IComponentData
  {
  }
}
