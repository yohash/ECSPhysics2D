# ECSPhysics2D Common Workflows

1. [World Management](#world-management)
2. [Body Creation](#body-creation)
3. [Shape Creation](#shape-creation)
4. [Joints](#joints)
5. [Forces and Impulses](#forces--impulses)
6. [Queries](#queries)
7. [Events](#events)
8. [Runtime Modification](#runtime-modification)
9. [Cleanup](#cleanup--destruction)

## World Management

### Default World
The `PhysicsWorldInitializationSystem` auto-creates a default world using `PhysicsWorldSingleton.CreateDefault()` if no `MultiWorldConfiguration` singleton is present. Default world is fetched at index 0:

```csharp
// In a system - world is ready to use
if (SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var singleton))
{
    var world = singleton.GetWorld(0); // Default world
}
```

### Multiple Worlds

Create a `MultiWorldConfiguration` singleton before `PhysicsWorldInitializationSystem` runs. Bodies route to worlds via `WorldIndex`.

**Authoring** (MonoBehaviour)**:**
```csharp
public class PhysicsWorldAuthoring : MonoBehaviour
{
    public float2 MainWorldGravity = new float2(0, -9.81f);
    public float2 DebrisWorldGravity = new float2(0, -15f);
    
    class Baker : Baker<PhysicsWorldAuthoring>
    {
        public override void Bake(PhysicsWorldAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            var config = new MultiWorldConfiguration
            {
                WorldCount = 2,
                FixedDeltaTime = 1f / 60f,
                World0 = new PhysicsWorldConfig { Gravity = authoring.MainWorldGravity, Enabled = true },
                World1 = new PhysicsWorldConfig { Gravity = authoring.DebrisWorldGravity, Enabled = true }
            };
            
            AddComponent(entity, config);
        }
    }
}
```

**Usage:**
```csharp
new PhysicsBodyComponent { WorldIndex = 1 } // Routes to debris world on body creation
```

[Top](#ecsphysics2d-common-workflows)

---

## Body Creation

### Required Components

| Component | Purpose | Notes |
|-----------|---------|-------|
| `LocalTransform` | Position/rotation | Unity handles baking from GameObject |
| `PhysicsBodyComponent` | Links entity to Box2D body | Set `WorldIndex`, damping, gravity scale. Leave `Body` as `default` (the `Body` is assigned after creation in `BuildPhysicsWorldSystem`) |
| Type tag | Body behavior | `PhysicsDynamicTag`, `PhysicsKinematicTag`, or `PhysicsStaticTag` |
| Shape | Collision geometry | `PhysicsShapeCircle`, `PhysicsShapeBox`, `PhysicsShapeCapsule`, `PhysicsShapePolygon`, `PhysicsShapeChain`, `CompoundShape`|
| `PhysicsMaterial` | Surface properties | Friction, bounciness, density |
| `CollisionFilter` | Layer filtering | Categories, mask, group index |

### Dynamic Body (fully simulated)

```csharp
var entity = ecb.CreateEntity();

ecb.AddComponent(entity, LocalTransform.FromPosition(spawnPos));
ecb.AddComponent(entity, new PhysicsBodyComponent
{
    WorldIndex = 0,
    LinearDamping = 0.1f,
    AngularDamping = 0.1f,
    GravityScale = 1f
});
ecb.AddComponent<PhysicsDynamicTag>(entity);
ecb.AddComponent(entity, new PhysicsShapeCircle { Radius = 0.5f });
ecb.AddComponent(entity, PhysicsMaterial.Default);
ecb.AddComponent(entity, CollisionFilter.Default);
```

### Kinematic Body (user-controlled, ignores forces)

```csharp
ecb.AddComponent(entity, new PhysicsBodyComponent { WorldIndex = 0 });
ecb.AddComponent<PhysicsKinematicTag>(entity);
// + transform, shape, material, filter
```

### Static Body (immovable)

```csharp
ecb.AddComponent(entity, new PhysicsBodyComponent { WorldIndex = 0 });
ecb.AddComponent<PhysicsStaticTag>(entity);
// + transform, shape, material, filter
```

### Batch Creation (explosions, debris)

```csharp
for (int i = 0; i < debrisCount; i++)
{
    var entity = ecb.CreateEntity();
    ecb.AddComponent(entity, LocalTransform.FromPosition(positions[i]));
    ecb.AddComponent(entity, new PhysicsBodyComponent 
    { 
        WorldIndex = 0,
        InitialLinearVelocity = velocities[i] 
    });
    ecb.AddComponent<PhysicsDynamicTag>(entity);
    ecb.AddComponent(entity, new PhysicsShapeCircle { Radius = 0.2f });
    ecb.AddComponent(entity, PhysicsMaterial.Default);
    ecb.AddComponent(entity, CollisionFilter.Default);
}
```

---

### Minimal Working Example

```csharp
// Spawn a bouncy ball at click position
var entity = ecb.CreateEntity();
ecb.AddComponent(entity, LocalTransform.FromPosition(clickPos));
ecb.AddComponent(entity, new PhysicsBodyComponent { WorldIndex = 0, GravityScale = 1f });
ecb.AddComponent<PhysicsDynamicTag>(entity);
ecb.AddComponent(entity, new PhysicsShapeCircle { Radius = 0.5f });
ecb.AddComponent(entity, new PhysicsMaterial { Friction = 0.3f, Bounciness = 0.8f, Density = 1f });
ecb.AddComponent(entity, CollisionFilter.Default);
```

**Lifecycle:**

1. **Entity created** — Components added via `EntityCommandBuffer`
2. **`BuildPhysicsWorldSystem`** — Detects uninitialized body, creates `PhysicsBody` in Box2D, adds `PhysicsBodyInitialized` tag
3. **`ShapeCreationSystem`** — Reads shape component, attaches collision geometry to body
4. **`PhysicsSimulationSystem`** — Box2D steps the world, resolves collisions
5. **`ExportPhysicsWorldSystem`** — Syncs body position/rotation back to `LocalTransform`
6. **Repeat** — Steps 4-5 run each `FixedStepSimulationSystemGroup` tick

[Top](#ecsphysics2d-common-workflows)

---

## Shape Creation

Shapes define collision geometry. Add one shape component per body, or use `DynamicBuffer<CompoundShape>` for multiple.

### Shape Types

| Component | Use Case | Key Properties |
|-----------|----------|----------------|
| `PhysicsShapeCircle` | Balls, coins, wheels | `Center`, `Radius` |
| `PhysicsShapeBox` | Crates, platforms | `Size`, `Center`, `Rotation` |
| `PhysicsShapeCapsule` | Characters, pills | `Center1`, `Center2`, `Radius` |
| `PhysicsShapePolygon` | Irregular convex (3-8 verts) | `VertexCount`, `Vertex0-7` |
| `PhysicsShapeChain` | Terrain, walls | `ChainBlob`, `IsLoop` |

### Single Shape

```csharp
// Circle
ecb.AddComponent(entity, new PhysicsShapeCircle { Radius = 0.5f, Center = float2.zero });

// Box
ecb.AddComponent(entity, new PhysicsShapeBox { Size = new float2(2f, 1f), Center = float2.zero, Rotation = 0f });

// Capsule (vertical, for characters)
ecb.AddComponent(entity, PhysicsShapeCapsule.CreateVertical(height: 1.8f, radius: 0.3f));

// Regular polygon (hexagon)
ecb.AddComponent(entity, PhysicsShapePolygon.CreateRegular(sides: 6, radius: 0.5f));
```

### Chain Shape (terrain)

```csharp
// Create blob asset with vertices
var vertices = new NativeArray<float2>(pointCount, Allocator.Temp);
// ... fill vertices ...
var chainBlob = ChainBlobData.Create(vertices, Allocator.Persistent);
vertices.Dispose();

ecb.AddComponent(entity, new PhysicsShapeChain { ChainBlob = chainBlob, IsLoop = false });
```

### Compound Shape (multiple shapes per body)

```csharp
var buffer = ecb.AddBuffer<CompoundShape>(entity);

// Add circle at offset
buffer.Add(new CompoundShape
{
    Type = CompoundShape.ShapeType.Circle,
    Param0 = new float2(0.5f, 0f),  // center
    Param1 = new float2(0.3f, 0f)   // radius in x
});

// Add box
buffer.Add(new CompoundShape
{
    Type = CompoundShape.ShapeType.Box,
    Param0 = new float2(1f, 0.5f),  // size
    Param1 = float2.zero,            // center
    Param2 = 0f                      // rotation
});
```

[Top](#ecsphysics2d-common-workflows)

---

## Joints

Joints connect two bodies. Create a separate entity with `PhysicsJointComponent` + type-specific component.

### Joint Types

| Component | Use Case | Key Features |
|-----------|----------|--------------|
| `DistanceJoint` | Ropes, springs | Length limits, spring, motor |
| `HingeJoint` | Doors, wheels, arms | Angle limits, motor, spring |
| `SliderJoint` | Pistons, elevators | Translation limits, motor |
| `WheelJoint` | Vehicle suspension | Spring + motor combo |
| `WeldJoint` | Rigid/breakable connection | Flexible stiffness |

### Required Components

Every joint entity needs:
1. `PhysicsJointComponent` — references BodyA, BodyB, joint type
2. Type-specific component — `HingeJoint`, `DistanceJoint`, etc.

### Unit conventions

The Unity `LowLevelPhysics2D` joint API uses **two different angle units** depending on the field:

| Field type | Unit | Examples |
|---|---|---|
| Joint angle limits / targets / motor speed | **Degrees** | `lowerAngleLimit`, `upperAngleLimit`, `springTargetAngle`, `motorSpeed` on `PhysicsHingeJointDefinition` |
| Anchor frame rotation (`PhysicsTransform.rotation`) | **Radians** | `localAnchorA.rotation`, `localAnchorB.rotation` |

This is why `HingeJoint` (and other joint structs) distinguish between `LowerAngleDegrees`/`UpperAngleDegrees` (passed directly to the limit fields) and `ReferenceAngleDegrees` (converted with `math.radians()` before being baked into the `localAnchorB` frame rotation in `JointCreationSystem`). Both carry "degrees" in their names — but only `ReferenceAngleDegrees` undergoes a radians conversion, because it targets `PhysicsTransform.rotation`, not a joint angle limit field.

### Hinge (revolute)

```csharp
var jointEntity = ecb.CreateEntity();

ecb.AddComponent(jointEntity, new PhysicsJointComponent
{
    BodyA = pivotEntity,
    BodyB = swingingEntity,
    Type = PhysicsJointComponent.JointType.Revolute,
    CollideConnected = false
});

ecb.AddComponent(jointEntity, HingeJoint.CreateHinge(anchor: new float2(0f, 0.5f)));
```

### Distance (rope/spring)

```csharp
// Rope with slight stretch
ecb.AddComponent(jointEntity, DistanceJoint.CreateRope(length: 2f, stretch: 0.1f));

// Spring
ecb.AddComponent(jointEntity, DistanceJoint.CreateSpring(length: 1f, frequency: 4f, damping: 0.5f));
```

### Slider (prismatic)

```csharp
// Piston
ecb.AddComponent(jointEntity, SliderJoint.CreatePiston(
    axis: new float2(0f, 1f),  // vertical
    stroke: 2f,
    speed: 1f
));
```

### Weld (rigid connection)

```csharp
// Rigid weld
ecb.AddComponent(jointEntity, WeldJoint.CreateRigid(anchorA: float2.zero, anchorB: float2.zero));

// Flexible (for breakable objects)
ecb.AddComponent(jointEntity, WeldJoint.CreateFlexible(anchorA: float2.zero, anchorB: float2.zero, flexibility: 0.2f));
```

### Breaking a Joint

Destroy the joint entity. Bodies remain intact.

```csharp
ecb.DestroyEntity(jointEntity);
```

---

### Minimal Working Example

#### Swinging Pendulum

```csharp
// Anchor (static)
var anchor = ecb.CreateEntity();
ecb.AddComponent(anchor, LocalTransform.FromPosition(new float3(0, 5, 0)));
ecb.AddComponent(anchor, new PhysicsBodyComponent { WorldIndex = 0 });
ecb.AddComponent<PhysicsStaticTag>(anchor);
ecb.AddComponent(anchor, new PhysicsShapeCircle { Radius = 0.1f });
ecb.AddComponent(anchor, PhysicsMaterial.Default);
ecb.AddComponent(anchor, CollisionFilter.Default);

// Bob (dynamic)
var bob = ecb.CreateEntity();
ecb.AddComponent(bob, LocalTransform.FromPosition(new float3(2, 3, 0)));
ecb.AddComponent(bob, new PhysicsBodyComponent { WorldIndex = 0, GravityScale = 1f });
ecb.AddComponent<PhysicsDynamicTag>(bob);
ecb.AddComponent(bob, new PhysicsShapeCircle { Radius = 0.3f });
ecb.AddComponent(bob, PhysicsMaterial.Default);
ecb.AddComponent(bob, CollisionFilter.Default);

// Hinge joint
var joint = ecb.CreateEntity();
ecb.AddComponent(joint, new PhysicsJointComponent
{
    BodyA = anchor,
    BodyB = bob,
    Type = PhysicsJointComponent.JointType.Revolute,
    CollideConnected = false
});
ecb.AddComponent(joint, HingeJoint.CreateHinge(anchor: float2.zero));
```

**Lifecycle:**

1. **Bodies created** — `BuildPhysicsWorldSystem` initializes physics bodies
2. **Shapes attached** — `ShapeCreationSystem` adds collision geometry
3. **Joint created** — `JointCreationSystem` links bodies in Box2D
4. **Simulation** — Physics runs, joint constrains motion
5. **Export** — Transforms sync back to ECS

[Top](#ecsphysics2d-common-workflows)

---

## Forces & Impulses

Add force/impulse components to dynamic bodies. `BatchForceApplicationSystem` processes them automatically.

### Force Types

| Component | Behavior | Use Case |
|-----------|----------|----------|
| `PhysicsForce` | Continuous, each frame | Thrust, per-body pushes |
| `PhysicsImpulse` | One-shot, auto-removed | Jumps, explosions, impacts |
| `PhysicsTorque` | Rotational force | Spinning, steering |
| `PhysicsAngularImpulse` | One-shot rotation | Spin kicks, ricochet |
| `PhysicsExplosion` | Radial impulse | Explosions, shockwaves |
| `PhysicsForceField` | Area force, each fixed step | Wind, gravity wells, vortices, slow zones |

### Continuous Force

```csharp
// Thrust (applied at center)
ecb.AddComponent(entity, PhysicsForce.CreateCentralForce(
    force: new float2(0f, 100f),
    duration: 0f  // 0 = infinite
));

// Force at point (generates torque)
ecb.AddComponent(entity, PhysicsForce.CreatePointForce(
    force: new float2(50f, 0f),
    worldPoint: new float2(1f, 0.5f),
    duration: 2f  // seconds
));
```

### One-Shot Impulse

```csharp
// Jump
ecb.AddComponent(entity, PhysicsImpulse.CreateCentralImpulse(new float2(0f, 10f)));

// Impact at point
ecb.AddComponent(entity, PhysicsImpulse.CreatePointImpulse(
    impulse: new float2(5f, 2f),
    worldPoint: hitPoint
));
```

### Torque & Angular Impulse

```csharp
// Continuous spin
ecb.AddComponent(entity, PhysicsTorque.Create(torque: 50f, duration: 0f));

// One-shot spin
ecb.AddComponent(entity, new PhysicsAngularImpulse { Impulse = 10f });
```

### Explosion (radial)

```csharp
// Create explosion entity
var explosion = ecb.CreateEntity();
ecb.AddComponent(explosion, PhysicsExplosion.Create(
    center: impactPoint,
    radius: 5f,
    force: 500f,
    worldIndex: 0
));

// With a falloff band beyond the radius
ecb.AddComponent(explosion, PhysicsExplosion.CreateWithFalloff(
    center: impactPoint,
    radius: 5f,
    force: 500f,
    falloff: 2f
));
```

`ExplosionSystem` calls `PhysicsWorld.Explode()` internally — handles spatial query, falloff, and impulse application in one optimized call.

Every shape within `radius` receives the **same** impulse; there is no fade inside the radius. `falloff` adds a band beyond the radius over which the impulse fades linearly to zero, so the total reach is `radius + falloff`. The impulse per unit length is `force / radius`, so for the same `force`, a larger radius pushes each shape less.

### Force Fields

A force field is an entity with `PhysicsForceField` and `LocalTransform`. `ForceFieldSystem` applies it to bodies every fixed step until the entity is destroyed or the component removed.

```csharp
// Gravity well: pulls bodies on the enemy layers toward the trap, fading toward the edge
var well = ecb.CreateEntity();
ecb.AddComponent(well, LocalTransform.FromPosition(new float3(trapPos, 0f)));
ecb.AddComponent(well, PhysicsForceField.CreateRadial(
    center: trapPos,
    strength: 40f,                       // newtons at full strength; negative pushes away
    radius: 6f,
    affectedLayers: enemyLayers,         // PhysicsMask; omit for all layers
    falloff: PhysicsForceField.FalloffMode.Linear
));

// Wind everywhere (unbounded), all layers
ecb.AddComponent(windEntity, PhysicsForceField.CreateWind(direction: new float2(1f, 0f), strength: 5f));

// Vortex with a spiral angle
ecb.AddComponent(vortexEntity, PhysicsForceField.CreateVortex(
    center: pos, strength: 30f, radius: 5f, angle: math.PI / 3f));
```

Semantics:

- **Units and mass:** `Strength` is a force in newtons applied at each body's center of mass (`ApplyForceToCenter`), so lighter bodies accelerate more than heavy ones under the same field. For `Damping` fields it is instead a rate in 1/s: velocity is scaled by `exp(-Strength * dt)` each step, which never reverses it.
- **Sign:** positive `Radial` strength pulls toward the center; negative pushes away. `Vortex` pulls inward by `Strength * cos(angle)` and spins counter-clockwise by `Strength * sin(angle)`.
- **Position:** the field follows its entity. `Center` is overwritten every step from `LocalTransform`, so move the entity to move the field. Distance is measured to each body's live center of mass.
- **Range:** bodies with their center of mass between `InnerRadius` and `OuterRadius` are affected. `OuterRadius = float.MaxValue` (as in `CreateWind`) makes the field unbounded.
- **Layer filtering:** a body is affected only if its `CollisionFilter.CategoryBits` intersect `AffectedLayers` (default all layers). Only dynamic bodies are affected.
- **Opt-in:** every matching dynamic body is affected. Set `RequireOptInTag = true` to only affect bodies tagged `AffectedByForceFields`.
- **World:** `WorldIndex` (default 0) selects the physics world; invalid indices fall back to 0.
- **Performance:** bounded fields use one Box2D broadphase circle query each, so many small fields stay cheap. Unbounded fields check every dynamic body.

Falloff modes (`Falloff`, bounded fields only; unbounded fields ignore it):

| Mode | Strength at distance `d` from center |
|------|--------------------------------------|
| `None` (default) | `Strength` everywhere in range |
| `Linear` | `Strength` at `InnerRadius`, fading linearly to 0 at `OuterRadius` |
| `InverseSquare` | `Strength` within `r0 = max(InnerRadius, MinDistance)`, then `Strength * (r0 / d)²` |

`InverseSquare` has a hard edge: at `OuterRadius` the force drops from `Strength * (r0 / OuterRadius)²` to zero. Make `OuterRadius` much larger than `r0` to keep that step small, or use `Linear` for a smooth edge. The factories set `MinDistance` to 1; set it on the struct to change `r0`.

### Clear All Forces

```csharp
ecb.AddComponent<ClearForcesRequest>(entity);
```

Removes all force components and zeros velocity.

---

### Minimal Working Example

#### Jumping Character

```csharp
// On jump input
ecb.AddComponent(playerEntity, PhysicsImpulse.CreateCentralImpulse(new float2(0f, 12f)));
```

[Top](#ecsphysics2d-common-workflows)

---

## Queries

Queries use a request/result pattern. Create a request entity → system processes it → read result from result entity.

### Query Types

| Request Component | Result | Use Case |
|-------------------|--------|----------|
| `RaycastRequest` | `RaycastResult` | Line-of-sight, bullets |
| `OverlapShapeRequest` | `DynamicBuffer<OverlapResult>` | Area detection |
| `AABBQueryRequest` | `DynamicBuffer<OverlapResult>` | Fast box query |
| `ClosestPointRequest` | `ClosestPointResult` | Nearest surface |

### Raycast

```csharp
// Create request + result entities
var resultEntity = ecb.CreateEntity();
ecb.AddComponent(resultEntity, new RaycastResult());

var requestEntity = ecb.CreateEntity();
ecb.AddComponent(requestEntity, new RaycastRequest
{
    Origin = startPoint,
    Direction = math.normalize(direction),
    MaxDistance = 100f,
    Filter = PhysicsQuery.QueryFilter.defaultFilter,
    ResultEntity = resultEntity,
    WorldIndex = 0
});

// Later, read result
var result = SystemAPI.GetComponent<RaycastResult>(resultEntity);
if (result.Hit)
{
    // result.Point, result.Normal, result.Distance, result.HitEntity
}
```

### Circle Overlap

```csharp
var resultEntity = ecb.CreateEntity();
ecb.AddBuffer<OverlapResult>(resultEntity);

var requestEntity = ecb.CreateEntity();
ecb.AddComponent(requestEntity, OverlapShapeRequest.CreateCircle(
    position: center,
    radius: 5f,
    resultEntity: resultEntity,
    worldIndex: 0
));

// Later, read results
var overlaps = SystemAPI.GetBuffer<OverlapResult>(resultEntity);
foreach (var overlap in overlaps)
{
    // overlap.Entity, overlap.Body, overlap.Shape
}
```

### Box Overlap

```csharp
ecb.AddComponent(requestEntity, OverlapShapeRequest.CreateBox(
    position: center,
    size: new float2(4f, 2f),
    rotation: 0.5f,  // radians
    resultEntity: resultEntity
));
```

### AABB Query (fastest)

```csharp
var resultEntity = ecb.CreateEntity();
ecb.AddBuffer<OverlapResult>(resultEntity);

var requestEntity = ecb.CreateEntity();
ecb.AddComponent(requestEntity, AABBQueryRequest.Create(
    min: new float2(-5f, -5f),
    max: new float2(5f, 5f),
    resultEntity: resultEntity
));
```

### Immediate Queries (no ECS)

For editor tools or synchronous needs:

```csharp
var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().GetWorld(0);

// Raycast
if (PhysicsQueryUtility.RaycastImmediate(world, origin, direction, maxDist, out var hit))
{
    // hit.Point, hit.Normal, hit.HitEntity
}

// Circle overlap
var results = new NativeList<PhysicsBody>(Allocator.Temp);
int count = PhysicsQueryUtility.OverlapCircleImmediate(world, center, radius, ref results);
results.Dispose();
```

### Layer Filtering

```csharp
// Create filter for specific layers
var filter = PhysicsQueryUtility.CreateLayerFilter(0, 2, 5);  // layers 0, 2, 5

// Use in request
ecb.AddComponent(requestEntity, new RaycastRequest
{
    // ...
    Filter = filter
});
```

---

### Minimal Working Examples

#### Ground Check Raycast

```csharp
var resultEntity = ecb.CreateEntity();
ecb.AddComponent(resultEntity, new RaycastResult());

ecb.AddComponent(ecb.CreateEntity(), new RaycastRequest
{
    Origin = feetPosition,
    Direction = new float2(0f, -1f),
    MaxDistance = 0.1f,
    Filter = PhysicsQueryUtility.CreateLayerFilter(0),  // ground layer
    ResultEntity = resultEntity
});

// Next frame
var result = SystemAPI.GetComponent<RaycastResult>(resultEntity);
bool isGrounded = result.Hit;
```

#### Area Damage

```csharp
// Query
var resultEntity = ecb.CreateEntity();
ecb.AddBuffer<OverlapResult>(resultEntity);
ecb.AddComponent(ecb.CreateEntity(), OverlapShapeRequest.CreateCircle(
    position: explosionCenter,
    radius: damageRadius,
    resultEntity: resultEntity
));

// Next frame — apply damage
var overlaps = SystemAPI.GetBuffer<OverlapResult>(resultEntity);
foreach (var overlap in overlaps)
{
    if (SystemAPI.HasComponent<Health>(overlap.Entity))
    {
        var health = SystemAPI.GetComponentRW<Health>(overlap.Entity);
        health.ValueRW.Current -= damage;
    }
}
```

**Lifecycle:**

1. **Request created** — Add request component to entity
2. **Query system runs** — `RaycastSystem`, `ShapeOverlapSystem`, or `AABBQuerySystem` processes request
3. **Result written** — System populates result entity, removes request component
4. **Read result** — Your system reads from result entity
5. **Cleanup** — Destroy request/result entities when done (or reuse)

[Top](#ecsphysics2d-common-workflows)

---

## Events

Events are gathered after simulation into `PhysicsEventsSingleton`. Read them in systems that run after `PhysicsEventGatheringSystem`.

### Event Types

| Buffer | Fires When | Key Data |
|--------|------------|----------|
| `Collisions` | Bodies touch/separate | `EntityA/B`, `ContactPoint`, `NormalImpulse` |
| `Triggers` | Overlap begin/end (no physics response) | `TriggerEntity`, `OtherEntity`, `EventType` |
| `SleepEvents` | Body sleeps/wakes | `Entity`, `IsSleeping` |
| `JointThresholds` | Joint force/torque exceeds limit | `JointEntity`, `Force`, `Torque` |

### Reading Collision Events

```csharp
[UpdateAfter(typeof(PhysicsEventGatheringSystem))]
public partial struct DamageOnCollisionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var events = SystemAPI.GetSingleton<PhysicsEventsSingleton>();
        
        foreach (var collision in events.Buffers.Collisions)
        {
            if (collision.EventType != CollisionEventType.Begin)
                continue;
                
            // Apply damage based on impact force
            if (SystemAPI.HasComponent<Health>(collision.EntityA))
            {
                var health = SystemAPI.GetComponentRW<Health>(collision.EntityA);
                health.ValueRW.Current -= collision.NormalImpulse * 0.1f;
            }
        }
    }
}
```

### Reading Trigger Events

```csharp
foreach (var trigger in events.Buffers.Triggers)
{
    if (trigger.EventType == TriggerEventType.Enter)
    {
        // Entity entered trigger zone
    }
    else if (trigger.EventType == TriggerEventType.Exit)
    {
        // Entity left trigger zone
    }
}
```

### Enabling Events on Shapes

Events only fire if enabled on the shape's `CollisionFilter`:

```csharp
ecb.AddComponent(entity, CollisionFilter.Default.WithCollisionEvents());       // Collision events

ecb.AddComponent(triggerEntity, CollisionFilter.Default.AsTrigger());          // trigger volume (no physics)
ecb.AddComponent(visitorEntity, CollisionFilter.Default.WithTriggerEvents());  // detectable by triggers
```

`AsTrigger()` and `WithTriggerEvents()` are two different roles:

- **`AsTrigger()`** makes the shape a trigger volume - it stops producing a collision
  response and starts reporting overlaps. It also enables its own trigger events.
- **`WithTriggerEvents()`** leaves the shape solid and merely makes it *visible* to
  trigger volumes.

### Trigger Gotchas

Three behaviours inherited from the underlying physics engine that otherwise read as bugs:

- **Both shapes must opt in.** A trigger only reports a visitor if that visitor also
  sets `WithTriggerEvents()`. Miss either side and `Buffers.Triggers` stays silent -
  this is separate from, and in addition to, category/mask filtering, which must also
  pass reciprocally.
- **Triggers have no continuous collision.** An object that passes clean through a
  trigger within a single step generates no event. For fast movers or small triggers,
  use a ray or shape cast instead.
- **Triggers detect other triggers.** Two `AsTrigger()` shapes will report each other.

Chain shapes cannot be trigger volumes - `IsTrigger` is ignored on them. They can still
set `WithTriggerEvents()` to be detected by other triggers.

### Joint Threshold Events

Enable threshold monitoring when creating joint:

```csharp
ecb.AddComponent(jointEntity, new DistanceJoint
{
    // ... other entries
    ForceThreshold = 100f,
    TorqueThreshold = 1000f
});
```

[Top](#ecsphysics2d-common-workflows)

---

## Runtime Modification

### Direct Body Access

Box2D bodies/joints expose mutable properties. Direct assignment works. For better performance when most bodies don't change frequently, use the `ChangeFilter`:

```csharp
foreach (var bodyComponent in
    SystemAPI.Query<RefRO<PhysicsBodyComponent>>()
      .WithAll<PhysicsBodyInitialized>()
      .WithChangeFilter<PhysicsBodyComponent>())  // Performance optimization
{
    if (!bodyComponent.ValueRO.IsValid)
        continue;

    var body = bodyComponent.ValueRO.Body;

    // Direct property modification - these take effect immediately
    body.gravityScale = 2f;
    body.linearDamping = 0.5f;
    body.angularDamping = 0.2f;

    // Velocity
    body.linearVelocity = new float2(5f, 0f);
    body.angularVelocity = 2f;

    // Position (teleport)
    body.position = newPosition;
    body.rotation = newAngle;

    // State
    body.enabled = false;          // Disable physics
    body.sleepingAllowed = false;
    body.Wake();                   // Force awake
}

```

### Shape Properties

```csharp
var shapeBuffer = SystemAPI.GetBuffer<PhysicsShapeReference>(entity);
var shape = shapeBuffer[0].Shape;

// Material
shape.friction = 0.8f;
shape.bounciness = 0.5f;
shape.rollingResistance = 0.1f;

// Filter (runtime layer change)
shape.contactFilter = new PhysicsShape.ContactFilter
{
    categories = 1 << newLayer,
    contacts = collisionMask
};
```

### Body Type Change

Cannot change type directly. Destroy and recreate:

```csharp
// Make dynamic body static
body.Body.Destroy();
ecb.RemoveComponent<PhysicsDynamicTag>(entity);
ecb.AddComponent<PhysicsStaticTag>(entity);
ecb.RemoveComponent<PhysicsBodyInitialized>(entity);  // Triggers recreation
```

[Top](#ecsphysics2d-common-workflows)

---

## Cleanup & Destruction

### Destroying a Body

**Order matters:** Destroy physics body first, then entity.

```csharp
var body = SystemAPI.GetComponent<PhysicsBodyComponent>(entity);
if (body.IsValid)
{
    body.Body.Destroy();
}
ecb.DestroyEntity(entity);
```

### Destroying a Joint

Destroy joint entity only. Connected bodies remain intact.

```csharp
var joint = SystemAPI.GetComponent<PhysicsJointComponent>(jointEntity);
if (joint.Joint.isValid)
{
    joint.Joint.Destroy();
}
ecb.DestroyEntity(jointEntity);
```

### Cleanup by Position (out-of-bounds)

```csharp
[UpdateAfter(typeof(ExportPhysicsWorldSystem))]
public partial struct OutOfBoundsCleanupSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        foreach (var (transform, body, entity) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<PhysicsBodyComponent>>()
            .WithAll<PhysicsDynamicTag>()
            .WithEntityAccess())
        {
            if (transform.ValueRO.Position.y < -50f)
            {
                if (body.ValueRO.IsValid)
                    body.ValueRO.Body.Destroy();
                ecb.DestroyEntity(entity);
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
```

### Cleanup by Count (oldest first)

```csharp
var query = SystemAPI.QueryBuilder()
    .WithAll<PhysicsDynamicTag, PhysicsBodyComponent>()
    .Build();

int count = query.CalculateEntityCount();
if (count > maxBodies)
{
    var entities = query.ToEntityArray(Allocator.Temp);
    int excess = count - maxBodies;
    
    for (int i = 0; i < excess; i++)
        ecb.DestroyEntity(entities[i]);
    
    entities.Dispose();
}
```

### Cleanup by Lifetime

```csharp
foreach (var (debris, body, entity) in
    SystemAPI.Query<RefRO<DebrisTag>, RefRO<PhysicsBodyComponent>>()
    .WithEntityAccess())
{
    if (debris.ValueRO.IsExpired(currentTime))
    {
        if (body.ValueRO.IsValid)
            body.ValueRO.Body.Destroy();
        ecb.DestroyEntity(entity);
    }
}
```

### Chain Shape Blob Disposal

Chain shapes use blob assets. Dispose before destroying entity:

```csharp
var chainShape = SystemAPI.GetComponent<PhysicsShapeChain>(entity);
if (chainShape.ChainBlob.IsCreated)
{
    chainShape.ChainBlob.Dispose();
}
ecb.DestroyEntity(entity);
```

---

### Important Rules

1. **Timing** — Never destroy bodies between `BuildPhysicsWorldSystem` and `ExportPhysicsWorldSystem`. Use `EntityCommandBuffer` scheduled after export.

2. **Validation** — Always check `body.IsValid` or `joint.isValid` before accessing. Handles invalidate on destruction.

3. **Order** — Destroy physics object → then destroy entity. Never reverse.

4. **Events clear each frame** — Read events before `PhysicsEventGatheringSystem` runs next frame, or they're lost.


[Top](#ecsphysics2d-common-workflows)