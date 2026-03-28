using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECSPhysics2D
{
  /// <summary>
  /// Each frame:
  ///   1. Reads the result from the previous query and draws a debug normal indicator.
  ///   2. Issues a new ClosestPointRequest for the current mouse world position.
  ///
  /// Runs after FixedStepSimulationSystemGroup so the result drawn this frame was
  /// computed from the request issued last frame — at most one fixed-step of latency.
  ///
  /// Debug visualization:
  ///   White line  — tick mark perpendicular to the normal at the closest surface point.
  ///   Green ray   — outward normal extending from that point.
  ///   Yellow cross — current mouse world position.
  /// </summary>
  [UpdateInGroup(typeof(SimulationSystemGroup))]
  [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
  public partial struct ClosestPointDemoSystem : ISystem
  {
    private Entity _resultEntity;

    private const float SearchRadius = 20f;
    private const float NormalLength = 1.5f;
    private const float TickHalfWidth = 0.25f;
    private const float MouseMarkerSize = 0.15f;

    public void OnCreate(ref SystemState state)
    {
      _resultEntity = state.EntityManager.CreateEntity();
      state.EntityManager.AddComponentData(_resultEntity, new ClosestPointResult());
    }

    public void OnDestroy(ref SystemState state)
    {
      if (state.EntityManager.Exists(_resultEntity))
        state.EntityManager.DestroyEntity(_resultEntity);
    }

    public void OnUpdate(ref SystemState state)
    {
      float dt = SystemAPI.Time.DeltaTime;

      // Draw result from the previous fixed step.
      var result = SystemAPI.GetComponent<ClosestPointResult>(_resultEntity);
      if (result.Found)
        DrawNormalIndicator(result.ClosestPoint, result.Normal, dt);

      // Issue a new request for the next fixed step.
      var mousePos = GetMouseWorldPosition();
      DrawMouseMarker(mousePos, dt);

      var requestEntity = state.EntityManager.CreateEntity();
      state.EntityManager.AddComponentData(requestEntity,
        ClosestPointRequest.Create(mousePos, SearchRadius, _resultEntity));
    }

    // -------------------------------------------------------------------------
    // Debug drawing
    // -------------------------------------------------------------------------

    private static void DrawNormalIndicator(float2 point, float2 normal, float duration)
    {
      var p = new Vector3(point.x, point.y, 0f);
      var n = new Vector3(normal.x, normal.y, 0f);

      // Tick mark perpendicular to the normal.
      var tangent = new Vector3(-normal.y, normal.x, 0f) * TickHalfWidth;
      Debug.DrawLine(p - tangent, p + tangent, Color.white, duration);

      // Outward normal ray.
      Debug.DrawRay(p, n * NormalLength, Color.green, duration);
    }

    private static void DrawMouseMarker(float2 pos, float duration)
    {
      var p = new Vector3(pos.x, pos.y, 0f);
      var h = new Vector3(MouseMarkerSize, 0f, 0f);
      var v = new Vector3(0f, MouseMarkerSize, 0f);
      Debug.DrawLine(p - h, p + h, Color.yellow, duration);
      Debug.DrawLine(p - v, p + v, Color.yellow, duration);
    }

    // -------------------------------------------------------------------------
    // Mouse helpers
    // -------------------------------------------------------------------------

    private static float2 GetMouseWorldPosition()
    {
      if (Mouse.current == null) return float2.zero;

      var camera = Camera.main;
      if (camera == null) return float2.zero;

      var screenPos = Mouse.current.position.ReadValue();
      var worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
      return new float2(worldPos.x, worldPos.y);
    }
  }
}
