namespace ECSPhysics2D
{
  /// <summary>
  /// Holds the debug draw preference that is shared between editor tooling and
  /// runtime world creation. Stored in the Runtime assembly so PhysicsWorldSingleton
  /// can read it without taking an editor dependency.
  ///
  /// The Editor menu writes to this static and persists the value via EditorPrefs.
  /// [InitializeOnLoad] restores it after every domain reload so new worlds created
  /// on play-mode entry always respect the current preference.
  /// </summary>
  public static class PhysicsDebugDraw
  {
    public static bool Enabled = false;
  }
}
