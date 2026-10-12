# Changelog

All notable changes to this project will be documented in this file.

The format is based on \[Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to \[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## \[0.3.0] - 2026-10-12

### Added

- `PhysicsForceField.Falloff` (`FalloffMode.None`, `Linear`, `InverseSquare`) with `MinDistance`. `None` is the default and matches previous output. `Linear` fades from full strength at `InnerRadius` to zero at `OuterRadius`. `InverseSquare` is full strength within `max(InnerRadius, MinDistance)` and falls off as `1/d²` beyond, with a hard edge at `OuterRadius`; `MinDistance` is clamped to a small epsilon so it can never divide by zero. Falloff is ignored by unbounded fields (#21)
- `PhysicsForceField.WorldIndex`: the physics world a field acts on; invalid indices fall back to world 0, matching `PhysicsExplosion` (#21)
- `PhysicsForceField.RequireOptInTag`: when true, only bodies tagged `AffectedByForceFields` are affected (the previous behavior) (#21)
- `CreateRadial` and `CreateVortex` take an optional `FalloffMode`, and new overloads take a `PhysicsMask affectedLayers`; `CreateWind` gains a `PhysicsMask` overload. Existing calls keep their meaning (#21)

### Changed

- **Behavior change:** force fields no longer require `AffectedByForceFields`. Every dynamic body on a layer in `AffectedLayers` is affected by default; set `RequireOptInTag` to restore opt-in (#21)
- Bounded fields (finite `OuterRadius`) find bodies with one Box2D broadphase circle query per field instead of scanning every body. A multi-shape body receives a field's force once per step. Box2D's overlap query, not the entity list, now decides candidates, so: a body whose only shapes are triggers may not be returned, a shape whose collision mask is 0 is never returned, and a body with `CategoryBits == 0` is never affected (#21)
- Distance to a field is measured from the body's live world center of mass instead of its `LocalTransform` (#21)
- `Damping` fields scale velocity by `exp(-Strength * dt)` instead of `1 - Strength * dt`. Slightly weaker for small `Strength * dt`, but independent of step length and never negative (#21)

### Fixed

- `PhysicsForceField.AffectedLayers` was never read; fields affected every layer. It is now honored for every field type, including `Damping` (#21)
- Forces with squared magnitude under 0.001 (about 0.0316 N) were silently dropped; any non-zero force is now applied (#21)
- A `Damping` field with `Strength * dt > 1` reversed body velocity (#21)
- Fields applied to bodies in every physics world; they now act only on `WorldIndex` (#21)

## \[0.2.0] - 2026-10-12

### Changed

- **Breaking:** removed `PhysicsExplosion.Create(center, radius, falloff, force, worldIndex)`. It took `force` and `falloff` in the opposite order from the other `Create` overloads, and since both are `float`, positional calls compiled and silently swapped the values (#19). Use `PhysicsExplosion.CreateWithFalloff(center, radius, force, falloff, worldIndex)` instead; its distinct name means old positional calls fail to compile rather than silently reordering. Callers that used named arguments only need to rename the method

### Fixed

- `ForceUtility.CalculateExplosionImpulse` ignored its `falloff` argument (an early return at `distance >= radius` made the falloff branch unreachable) and faded the impulse linearly inside the radius. It now matches Box2D's `Explode()` profile: full magnitude within the radius, fading linearly to zero over `falloff` beyond it
- `PhysicsExplosion` documentation described a distance falloff inside the radius and called `Force` a force; Box2D applies a uniform impulse within `Radius` (impulse per unit length = `Force / Radius`), and `Falloff` only fades it over a band beyond the radius

## \[0.1.8] - 2026-07-25

### Fixed

- Trigger events never fired for circle, box, capsule, polygon, or compound shapes; these paths set the Unity `isTrigger` flag but left `triggerEvents` false, producing a shape that Box2D treats as a sensor but never reports overlaps for, so `PhysicsEventsSingleton.Buffers.Triggers` stayed empty for the shape's entire lifetime (#17). Chain shapes were unaffected - `PhysicsChainDefinition` has no `isTrigger`, so that path already set `triggerEvents`

### Changed

- `CollisionFilter.GenerateTriggerEvents` split into two fields: `IsTrigger` (Unity's `isTrigger` - this shape is a trigger volume) and `GenerateTriggerEvents` (Unity's `triggerEvents` - this shape participates in trigger events). One flag could not express both concepts; `triggerEvents` is false by default even for triggers, and must be set on **both** the trigger volume and the visiting shape for an overlap to be reported
- `CollisionFilterExtensions.AsTrigger()` added; sets both flags, and is the replacement for the previous `WithTriggerEvents()` behaviour when creating a trigger volume
- `CollisionFilterExtensions.WithTriggerEvents()` narrowed to the visitor side; it now makes a solid shape detectable by triggers and no longer turns the shape into a trigger

## \[0.1.7] - 2026-05-19

### Fixed

- `PhysicsEventGatheringSystem` now guards against invalidated shape handles before accessing `.body` in the `contactEndEvents`, `contactHitEvents`, and `triggerEndEvents` loops; prevents a Unity warning when a body is destroyed mid-frame and Box2D emits end events for its now-invalid shapes

## \[0.1.6] - 2026-04-23

### Added

- `ReferenceAngleDegrees` field on `HingeJoint`, `WeldJoint`, `WheelJoint`, and `RelativeJoint`; baked into `localAnchorB`'s rotation frame at joint creation, exposing Box2D v3's anchor-frame reference-angle mechanism that was previously unreachable from the ECS API
- Joint angle unit conventions documented in `CommonWorkflows.md` — `PhysicsHingeJointDefinition` limit/target/motor fields use **degrees**; `PhysicsTransform.rotation` uses **radians**

### Changed

- `HingeJoint.LowerAngle`/`UpperAngle` renamed to `LowerAngleDegrees`/`UpperAngleDegrees`; angle limit values pass through to the Unity API unchanged (Unity's `PhysicsHingeJointDefinition` limit fields take degrees, not radians)
- `SliderJoint.TargetAngle` renamed to `SpringTargetTranslation`; the field wires to `springTargetTranslation` (meters along the slide axis), not an angle
- `WeldJoint.ReferenceAngle` renamed to `ReferenceAngleDegrees` for unit-in-the-name consistency

### Fixed

- `WeldJoint.ReferenceAngleDegrees` was defined on the struct and set by factory methods but silently dropped in `JointCreationSystem`; now correctly wired into the anchor frame rotation at joint creation

### Removed

- `RelativeJoint.TuningFrequency` and `TuningDamping`; these fields had no corresponding parameter in Unity's `PhysicsRelativeJointDefinition` and were never applied

## \[0.1.5] - 2026-04-09

### Removed

###### Reverted the changes introduced in v0.1.3. The fix to accommodate parent->child relationships introduced additional problems. The correct design is for all physics entities to be root entities (no `Parent`), using Box2D joints to express physical relationships instead of ECS transform hierarchy

## \[0.1.4] - 2026-03-31

### Added

###### Add a computed `Normal` variable to the `ClosestPointResult`. Normal is computed by geometry in `ClosestPointQuerySystem`.

## \[0.1.3] - 2026-03-18

### Fixed

###### Fix for proper ECS parent->child relationship recognition, and appropriate `LocalToWorld` position computation

## \[0.1.2] - 2026-03-12

### Fixed

###### Fix for `ClosestPointQuerySystem` only writes result if a point is found

## \[0.1.1] - 2026-03-08

### Added

###### Added a debug drawings toggle

## \[0.1.0] - 2026-02-04

### Added

###### Created project

