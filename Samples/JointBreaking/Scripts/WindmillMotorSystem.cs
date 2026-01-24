using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.JointBreaking
{
  /// <summary>
  /// Applies constant angular velocity to windmill motors.
  /// Maintains rotation speed despite collisions and drag.
  /// </summary>
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  [UpdateAfter(typeof(BuildPhysicsWorldSystem))]
  [UpdateBefore(typeof(PhysicsSimulationSystem))]
  [BurstCompile]
  public partial struct WindmillMotorSystem : ISystem
  {
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      var deltaTime = SystemAPI.Time.DeltaTime;

      foreach (var (motor, bodyComponent, velocity) in
          SystemAPI.Query<
              RefRO<WindmillMotor>,
              RefRO<PhysicsBodyComponent>,
              RefRW<PhysicsVelocity>>()) {
        if (!bodyComponent.ValueRO.IsValid)
          continue;

        var body = bodyComponent.ValueRO.Body;

        // Calculate torque needed to reach target velocity
        float currentAngularVel = body.angularVelocity;
        float targetAngularVel = motor.ValueRO.TargetAngularVelocity;
        float velocityError = targetAngularVel - currentAngularVel;

        // Apply torque proportional to velocity error
        // Simple P-controller for smooth acceleration
        float torque = velocityError * body.rotationalInertia * 10f;  // Proportional gain = 10
        torque = math.clamp(torque, -motor.ValueRO.MaxTorque, motor.ValueRO.MaxTorque);

        body.ApplyTorque(torque);

        // Also directly update velocity component for consistency
        velocity.ValueRW.Angular = currentAngularVel;
      }
    }
  }
}
