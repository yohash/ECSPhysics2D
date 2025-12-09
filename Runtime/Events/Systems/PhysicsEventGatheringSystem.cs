using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Gathers physics events from Box2D after simulation completes.
  /// Populates the singleton event buffers for gameplay systems to consume.
  /// 
  /// Pipeline position: After simulation, before gameplay systems.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(PhysicsSimulationSystem))]
  [UpdateBefore(typeof(ExportPhysicsWorldSystem))]
  [BurstCompile]
  public partial struct PhysicsEventGatheringSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var worldSingleton))
        return;

      if (!SystemAPI.TryGetSingletonRW<PhysicsEventsSingleton>(out var eventsSingleton))
        return;

      var physicsWorld = worldSingleton.World;
      ref var buffers = ref eventsSingleton.ValueRW.Buffers;

      // Gather contact events
      GatherContactEvents(physicsWorld, ref buffers);

      // Gather trigger events
      GatherTriggerEvents(physicsWorld, ref buffers);

      // Update statistics
      eventsSingleton.ValueRW.LastFrameStats = buffers.GetStats();
    }

    private void GatherContactEvents(PhysicsWorld world, ref PhysicsEventBuffers buffers)
    {
      // =====================================================================
      // NOTE: The actual Box2D v3 API for retrieving contact events may differ.
      // Adjust these calls based on the real UnityEngine.LowLevelPhysics2D API.
      // 
      // Expected API patterns:
      //   var beginEvents = world.contactBeginEvents;
      //   var endEvents = world.contactEndEvents;
      // Or:
      //   var events = world.GetContactEvents(Allocator.Temp);
      // =====================================================================

      // Get contact begin events
      var beginEvents = world.contactBeginEvents;
      for (int i = 0; i < beginEvents.Length; i++) {
        var rawEvent = beginEvents[i];

        // ensure collision contact validity before accessing manifold data
        var valid = rawEvent.contactId.isValid;
        if (!valid) { continue; }

        // ensure there is at least one contact point
        var manifold = rawEvent.contactId.contact.manifold;
        if (manifold.pointCount == 0) { continue; }

        // compute the average contact point, in case of multiple contact points
        var point = Vector2.zero;
        for (int n = 0; n < manifold.pointCount; n++) {
          point += manifold[n].point;
        }
        point /= manifold.pointCount;

        var evt = new CollisionEvent
        {
          EntityA = rawEvent.shapeA.body.GetEntityUserData(),
          EntityB = rawEvent.shapeB.body.GetEntityUserData(),
          BodyA = rawEvent.shapeA.body,
          BodyB = rawEvent.shapeB.body,
          ShapeA = rawEvent.shapeA,
          ShapeB = rawEvent.shapeB,
          ContactPoint = point,
          ContactNormal = rawEvent.contactId.contact.manifold.normal,
          NormalImpulse = 0f,  // Begin events don't have impulse yet
          TangentImpulse = 0f,
          EventType = CollisionEventType.Begin
        };

        buffers.Collisions.Add(evt);
      }

      // Get contact end events
      var endEvents = world.contactEndEvents;
      for (int i = 0; i < endEvents.Length; i++) {
        var rawEvent = endEvents[i];

        var evt = new CollisionEvent
        {
          EntityA = rawEvent.shapeA.body.GetEntityUserData(),
          EntityB = rawEvent.shapeB.body.GetEntityUserData(),
          BodyA = rawEvent.shapeA.body,
          BodyB = rawEvent.shapeB.body,
          ShapeA = rawEvent.shapeA,
          ShapeB = rawEvent.shapeB,
          ContactPoint = float2.zero,  // End events may not have contact point
          ContactNormal = float2.zero,
          NormalImpulse = 0f,
          TangentImpulse = 0f,
          EventType = CollisionEventType.End
        };

        buffers.Collisions.Add(evt);
      }

      // Get contact hit events (for impulse data)
      var hitEvents = world.contactHitEvents;
      for (int i = 0; i < hitEvents.Length; i++) {
        var rawEvent = hitEvents[i];

        // Get full velocities at contact point
        float2 velA = GetVelocityAtPoint(rawEvent.shapeA.body, rawEvent.point);
        float2 velB = GetVelocityAtPoint(rawEvent.shapeB.body, rawEvent.point);
        float2 relativeVel = velA - velB;

        // Decompose into normal and tangent
        float2 normal = rawEvent.normal;
        float normalSpeed = math.dot(relativeVel, normal);
        float2 tangentVel = relativeVel - (normalSpeed * normal);
        float tangentSpeed = math.length(tangentVel);

        // Calculate reduced mass
        float massA = rawEvent.shapeA.body.mass;
        float massB = rawEvent.shapeB.body.mass;
        float reducedMass = (massA * massB) / (massA + massB);

        // Normal impulse (separation force)
        float normalImpulse = math.abs(normalSpeed) * reducedMass;

        // Tangent impulse (friction, clamped by Coulomb's law)
        float friction = math.max(rawEvent.shapeA.friction, rawEvent.shapeB.friction);
        float maxTangentImpulse = friction * normalImpulse;
        float uncappedTangent = tangentSpeed * reducedMass;
        float tangentImpulse = math.min(uncappedTangent, maxTangentImpulse);

        var evt = new CollisionEvent
        {
          EntityA = rawEvent.shapeA.body.GetEntityUserData(),
          EntityB = rawEvent.shapeB.body.GetEntityUserData(),
          BodyA = rawEvent.shapeA.body,
          BodyB = rawEvent.shapeB.body,
          ShapeA = rawEvent.shapeA,
          ShapeB = rawEvent.shapeB,
          ContactPoint = rawEvent.point,
          ContactNormal = rawEvent.normal,
          NormalImpulse = normalImpulse,
          TangentImpulse = tangentImpulse,
          EventType = CollisionEventType.Begin  // Hit events are impacts
        };

        buffers.Collisions.Add(evt);
      }
    }

    private float2 GetVelocityAtPoint(PhysicsBody body, float2 worldPoint)
    {
      // Linear velocity + rotational contribution
      float2 position = new float2(body.position.x, body.position.y);
      float2 r = worldPoint - position;

      float2 rotationalVel = new float2(
        -body.angularVelocity * r.y,
        body.angularVelocity * r.x);

      float2 linearVelocity = new float2(body.linearVelocity.x, body.linearVelocity.y);
      return linearVelocity + rotationalVel;
    }


    private void GatherTriggerEvents(PhysicsWorld world, ref PhysicsEventBuffers buffers)
    {
      // =====================================================================
      // NOTE: Similar API adjustment may be needed here.
      // Expected API patterns:
      //   var sensorBeginEvents = world.sensorBeginEvents;
      //   var sensorEndEvents = world.sensorEndEvents;
      // =====================================================================

      // Get trigger enter events
      var enterEvents = world.triggerBeginEvents;
      for (int i = 0; i < enterEvents.Length; i++) {
        var rawEvent = enterEvents[i];

        var evt = new TriggerEvent
        {
          TriggerEntity = rawEvent.triggerShape.body.GetEntityUserData(),
          OtherEntity = rawEvent.visitorShape.body.GetEntityUserData(),
          TriggerBody = rawEvent.triggerShape.body,
          OtherBody = rawEvent.visitorShape.body,
          TriggerShape = rawEvent.triggerShape,
          OtherShape = rawEvent.visitorShape,
          EventType = TriggerEventType.Enter
        };

        buffers.Triggers.Add(evt);
      }

      // Get trigger exit events
      var exitEvents = world.triggerEndEvents;
      for (int i = 0; i < exitEvents.Length; i++) {
        var rawEvent = exitEvents[i];

        var evt = new TriggerEvent
        {
          TriggerEntity = rawEvent.triggerShape.body.GetEntityUserData(),
          OtherEntity = rawEvent.visitorShape.body.GetEntityUserData(),
          TriggerBody = rawEvent.triggerShape.body,
          OtherBody = rawEvent.visitorShape.body,
          TriggerShape = rawEvent.triggerShape,
          OtherShape = rawEvent.visitorShape,
          EventType = TriggerEventType.Exit
        };

        buffers.Triggers.Add(evt);
      }
    }
  }
}