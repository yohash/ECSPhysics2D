using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Unity.Entities;

namespace ECSPhysics2D.Editor
{
  public static class PhysicsDebugDrawMenu
  {
    private const string PrefKey  = "ECSPhysics2D.DebugDrawEnabled";
    private const string MenuPath = "ECS Physics 2D/Debug Draw Enabled";

    private static bool IsEnabled => EditorPrefs.GetBool(PrefKey, false);

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
      bool next = !IsEnabled;
      EditorPrefs.SetBool(PrefKey, next);
      ApplyToWorlds(next);
    }

    [MenuItem(MenuPath, validate = true)]
    private static bool ToggleValidate()
    {
      Menu.SetChecked(MenuPath, IsEnabled);
      return Application.isPlaying;
    }

    private static void ApplyToWorlds(bool enabled)
    {
      var defaultWorld = World.DefaultGameObjectInjectionWorld;
      if (defaultWorld == null || !defaultWorld.IsCreated) return;

      var em = defaultWorld.EntityManager;
      if (!em.HasSingleton<PhysicsWorldSingleton>()) return;

      var singleton = em.GetSingleton<PhysicsWorldSingleton>();
      for (int i = 0; i < singleton.WorldCount; i++) {
        var world = singleton.GetWorld(i);
        if (!world.isValid) continue;

        var dd = world.debugDraw;
        dd.drawShapes   = enabled;
        dd.drawJoints   = enabled;
        dd.drawAABBs    = enabled;
        dd.drawContacts = enabled;
        dd.drawMass     = enabled;
        world.debugDraw = dd;
      }
    }
  }
}
