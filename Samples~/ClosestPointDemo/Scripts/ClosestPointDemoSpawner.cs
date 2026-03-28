using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECSPhysics2D
{
  /// <summary>
  /// Creates four static physics shapes for the ClosestPoint demo:
  /// a circle, a capsule, a square polygon, and a sine-wave chain segment.
  ///
  /// Add this MonoBehaviour to a GameObject in the scene. The shapes are created
  /// directly via EntityManager on Start and remain static throughout the session.
  /// </summary>
  public class ClosestPointDemoSpawner : MonoBehaviour
  {
    private BlobAssetReference<ChainBlobData> _chainBlob;

    private void Start()
    {
      var world = World.DefaultGameObjectInjectionWorld;
      if (world == null) {
        Debug.LogError("ClosestPointDemoSpawner: no ECS world found.");
        return;
      }

      var em = world.EntityManager;

      CreateCircle(em, new float2(-4f, 1f), 1.2f);
      CreateCapsule(em, new float2(0f, 0.5f), 0.5f, 2.5f);
      CreateBox(em, new float2(4f, 1f), new float2(2f, 2f));
      CreateSineChain(em, new float2(-6f, -2.5f), width: 12f, segments: 32);
    }

    private void OnDestroy()
    {
      if (_chainBlob.IsCreated)
        _chainBlob.Dispose();
    }

    // -------------------------------------------------------------------------
    // Shape creators
    // -------------------------------------------------------------------------

    private static void CreateCircle(EntityManager em, float2 position, float radius)
    {
      var e = CreateStaticBase(em, position);
      em.AddComponentData(e, new PhysicsShapeCircle { Center = float2.zero, Radius = radius });
    }

    private static void CreateCapsule(EntityManager em, float2 position, float radius, float height)
    {
      var e = CreateStaticBase(em, position);
      em.AddComponentData(e, PhysicsShapeCapsule.CreateVertical(height, radius));
    }

    private static void CreateBox(EntityManager em, float2 position, float2 size)
    {
      var e = CreateStaticBase(em, position);
      em.AddComponentData(e, new PhysicsShapeBox { Center = float2.zero, Size = size, Rotation = 0f });
    }

    private void CreateSineChain(EntityManager em, float2 origin, float width, int segments)
    {
      var verts = new NativeArray<float2>(segments + 1, Allocator.Temp);
      for (int i = 0; i <= segments; i++) {
        float t = i / (float)segments;
        verts[i] = new float2(
          origin.x + t * width,
          origin.y + math.sin(t * 2f * math.PI) * 0.8f
        );
      }
      _chainBlob = ChainBlobData.Create(verts, Allocator.Persistent);
      verts.Dispose();

      var e = CreateStaticBase(em, float2.zero);
      em.AddComponentData(e, new PhysicsShapeChain { ChainBlob = _chainBlob, IsLoop = false });
    }

    // -------------------------------------------------------------------------
    // Shared entity setup
    // -------------------------------------------------------------------------

    private static Entity CreateStaticBase(EntityManager em, float2 position)
    {
      var e = em.CreateEntity();
      em.AddComponentData(e, LocalTransform.FromPosition(new float3(position.x, position.y, 0f)));
      em.AddComponentData(e, new PhysicsBodyComponent { WorldIndex = 0, GravityScale = 0f });
      em.AddComponentData(e, new PhysicsMaterial { Friction = 0.3f, Bounciness = 0.1f, Density = 0f });
      em.AddComponentData(e, CollisionFilter.Default);
      em.AddComponent<PhysicsStaticTag>(e);
      return e;
    }
  }
}
