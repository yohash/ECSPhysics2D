# Changelog

All notable changes to this project will be documented in this file.

The format is based on \[Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to \[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

