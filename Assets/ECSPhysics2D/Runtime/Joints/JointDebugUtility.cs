using Unity.Mathematics;
using Unity.Burst;

namespace ECSPhysics2D
{
  /// <summary>
  /// Debug utilities for joint visualization.
  /// </summary>
  [BurstCompile]
  public static class JointDebugUtility
  {
    public static void DrawJoint(PhysicsJointComponent joint, float3 posA, float3 posB, UnityEngine.Color color)
    {
      switch (joint.Type) {
        case PhysicsJointComponent.JointType.Distance:
          UnityEngine.Debug.DrawLine(posA, posB, color);
          break;

        case PhysicsJointComponent.JointType.Revolute:
          UnityEngine.Debug.DrawLine(posA, posB, color);
          DrawCircle(posA, 0.1f, color);
          break;

        case PhysicsJointComponent.JointType.Prismatic:
          UnityEngine.Debug.DrawLine(posA, posB, color);
          DrawBox(posA, 0.1f, color);
          DrawBox(posB, 0.1f, color);
          break;

        case PhysicsJointComponent.JointType.Wheel:
          UnityEngine.Debug.DrawLine(posA, posB, color);
          DrawCircle(posB, 0.2f, color);
          break;
      }
    }


    private static void DrawCircle(float3 center, float radius, UnityEngine.Color color)
    {
      int segments = 16;
      for (int i = 0; i < segments; i++) {
        float angle1 = (i * 2f * math.PI) / segments;
        float angle2 = ((i + 1) * 2f * math.PI) / segments;

        var p1 = center + new float3(math.cos(angle1) * radius, math.sin(angle1) * radius, 0);
        var p2 = center + new float3(math.cos(angle2) * radius, math.sin(angle2) * radius, 0);

        UnityEngine.Debug.DrawLine(p1, p2, color);
      }
    }

    private static void DrawBox(float3 center, float size, UnityEngine.Color color)
    {
      float half = size * 0.5f;
      var corners = new float3[]
      {
        center + new float3(-half, -half, 0),
        center + new float3(half, -half, 0),
        center + new float3(half, half, 0),
        center + new float3(-half, half, 0)
      };

      for (int i = 0; i < 4; i++) {
        UnityEngine.Debug.DrawLine(corners[i], corners[(i + 1) % 4], color);
      }
    }
  }
}