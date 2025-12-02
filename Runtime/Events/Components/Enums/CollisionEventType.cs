namespace ECSPhysics2D
{
  /// <summary>
  /// Type of collision event.
  /// </summary>
  public enum CollisionEventType : byte
  {
    Begin,  // Contact started this frame
    End     // Contact ended this frame
  }
}