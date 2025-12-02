using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using ECSPhysics2D;
using Unity.Transforms;

namespace ECSPhysics2D
{
  /// <summary>
  /// Helper for creating complex joint arrangements.
  /// </summary>
  public static class JointChainBuilder
  {
    /// <summary>
    /// Creates a chain of bodies connected by joints.
    /// </summary>
    public static NativeArray<Entity> CreateChain(
      EntityManager em,
      int linkCount,
      float linkLength,
      float linkMass,
      PhysicsJointComponent.JointType jointType,
      Entity anchorBody = default)
    {
      var entities = new NativeArray<Entity>(linkCount, Allocator.Temp);
      var archetype = em.CreateArchetype(
          typeof(PhysicsBodyComponent),
          typeof(LocalTransform),
          typeof(PhysicsVelocity),
          typeof(PhysicsDynamicTag),
          typeof(PhysicsMass),
          typeof(PhysicsGravityScale),
          typeof(PhysicsDamping)
      );

      // Create chain links
      for (int i = 0; i < linkCount; i++) {
        entities[i] = em.CreateEntity(archetype);

        var position = new float3(0, -i * linkLength, 0);
        em.SetComponentData(entities[i], LocalTransform.FromPosition(position));
        em.SetComponentData(entities[i], PhysicsMass.CreateDefault(linkMass));

        // Create joint to previous link
        if (i > 0) {
          CreateJointBetween(em, entities[i - 1], entities[i], jointType, linkLength);
        } else if (anchorBody != Entity.Null) {
          // Connect first link to anchor
          CreateJointBetween(em, anchorBody, entities[0], jointType, linkLength);
        }
      }

      return entities;
    }

    private static Entity CreateJointBetween(
      EntityManager em,
      Entity bodyA,
      Entity bodyB,
      PhysicsJointComponent.JointType jointType,
      float distance)
    {
      var jointEntity = em.CreateEntity();

      em.AddComponentData(jointEntity, new PhysicsJointComponent
      {
        BodyA = bodyA,
        BodyB = bodyB,
        Type = jointType,
        CollideConnected = false
      });

      switch (jointType) {
        case PhysicsJointComponent.JointType.Distance:
          em.AddComponentData(jointEntity, DistanceJoint.CreateRope(distance));
          break;

        case PhysicsJointComponent.JointType.Revolute:
          em.AddComponentData(jointEntity, HingeJoint.CreateHinge(
            new float2(0, -distance * 0.5f)));
          break;

        case PhysicsJointComponent.JointType.Weld:
          em.AddComponentData(jointEntity, WeldJoint.CreateRigid(
            new float2(0, -distance * 0.5f),
            new float2(0, distance * 0.5f)));
          break;
      }

      return jointEntity;
    }

    /// <summary>
    /// Creates a grid of bodies connected by joints.
    /// </summary>
    public static void CreateGrid(
      EntityManager em,
      int width,
      int height,
      float spacing,
      PhysicsJointComponent.JointType jointType)
    {
      var bodies = new NativeArray<Entity>(width * height, Allocator.Temp);
      var archetype = em.CreateArchetype(
        typeof(PhysicsBodyComponent),
        typeof(LocalTransform),
        typeof(PhysicsVelocity),
        typeof(PhysicsDynamicTag),
        typeof(PhysicsMass)
      );

      // Create grid of bodies
      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          int index = y * width + x;
          bodies[index] = em.CreateEntity(archetype);

          var position = new float3(x * spacing, y * spacing, 0);
          em.SetComponentData(bodies[index], LocalTransform.FromPosition(position));
          em.SetComponentData(bodies[index], PhysicsMass.CreateDefault(1f));

          // Create horizontal joint
          if (x > 0) {
            CreateJointBetween(em, bodies[index - 1], bodies[index],
              jointType, spacing);
          }

          // Create vertical joint
          if (y > 0) {
            CreateJointBetween(em, bodies[index - width], bodies[index],
              jointType, spacing);
          }
        }
      }

      bodies.Dispose();
    }
  }
}