using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Unity.Entities;

namespace ECSPhysics2D.Editor
{
  /// <summary>
  /// Adds "ECS Physics 2D > Debug Draw Enabled" to the Unity top-bar menu.
  ///
  /// The static PhysicsDebugDraw.Enabled (Runtime assembly) is the single source
  /// of truth at runtime. EditorPrefs persists the preference across sessions.
  /// [InitializeOnLoad] restores PhysicsDebugDraw.Enabled from EditorPrefs after
  /// every domain reload so worlds created on play-mode entry inherit the setting.
  /// </summary>
  [InitializeOnLoad]
  public static class PhysicsDebugDrawMenu
  {
    private const string PrefKey = "ECSPhysics2D.DebugDrawEnabled";
    private const string MenuPath = "ECS Physics 2D/Debug Draw Enabled";

    static PhysicsDebugDrawMenu()
    {
      PhysicsDebugDraw.Enabled = EditorPrefs.GetBool(PrefKey, false);
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
      bool next = !PhysicsDebugDraw.Enabled;
      PhysicsDebugDraw.Enabled = next;
      EditorPrefs.SetBool(PrefKey, next);
      ApplyToWorlds(next);
    }

    [MenuItem(MenuPath, validate = true)]
    private static bool ToggleValidate()
    {
      Menu.SetChecked(MenuPath, PhysicsDebugDraw.Enabled);
      return true;
    }

    private static void ApplyToWorlds(bool enabled)
    {
      if (!Application.isPlaying) return;

      var defaultWorld = World.DefaultGameObjectInjectionWorld;
      if (defaultWorld == null || !defaultWorld.IsCreated) return;

      var em = defaultWorld.EntityManager;
      var query = em.CreateEntityQuery(typeof(PhysicsWorldSingleton));

      if (!query.TryGetSingletonEntity<PhysicsWorldSingleton>(out Entity entity)) {
        return;
      }

      var singleton = em.GetComponentData<PhysicsWorldSingleton>(entity);

      var options = enabled ? PhysicsWorld.DrawOptions.DefaultAll : PhysicsWorld.DrawOptions.Off;

      for (int i = 0; i < singleton.WorldCount; i++) {
        var world = singleton.GetWorld(i);
        if (!world.isValid) continue;
        world.drawOptions = options;
      }
    }
  }
}
