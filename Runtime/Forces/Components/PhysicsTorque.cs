using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Torque (rotational force) component.
  /// </summary>
  public struct PhysicsTorque : IComponentData
  {
    public float Torque;           // Torque in Newton-meters
    public float Duration;         // How long to apply (0 = infinite)
    public float TimeRemaining;    // Countdown timer

    public static PhysicsTorque Create(float torque, float duration = 0f)
    {
      return new PhysicsTorque
      {
        Torque = torque,
        Duration = duration,
        TimeRemaining = duration
      };
    }
  }

}