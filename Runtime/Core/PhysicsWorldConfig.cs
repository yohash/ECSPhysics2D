using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace ECSPhysics2D
{
  /// <summary>
  /// Configuration for a single physics world.
  /// Allows per-world customization of gravity, simulation parameters, etc.
  /// </summary>
  public struct PhysicsWorldConfig
  {
    public float2 Gravity;
    public int SimulationSubSteps;
    public float ContactFrequency;
    public float ContactDamping;
    public float ContactHitEventThreshold;
    public bool Enabled;

    public static PhysicsWorldConfig Default => new PhysicsWorldConfig
    {
      Gravity = new float2(0f, -9.81f),
      SimulationSubSteps = 4,
      ContactFrequency = 30f,
      ContactDamping = 1f,
      ContactHitEventThreshold = 0.01f,
      Enabled = true
    };

    /// <summary>
    /// Creates a debris/low-fidelity world configuration with reduced iterations.
    /// </summary>
    public static PhysicsWorldConfig Debris => new PhysicsWorldConfig
    {
      Gravity = new float2(0f, -9.81f),
      SimulationSubSteps = 2,
      ContactFrequency = 15f,
      ContactDamping = 1f,
      ContactHitEventThreshold = 0.1f,
      Enabled = true
    };

    /// <summary>
    /// Creates a disabled world configuration (for static-only worlds).
    /// </summary>
    public static PhysicsWorldConfig Disabled => new PhysicsWorldConfig
    {
      Gravity = float2.zero,
      SimulationSubSteps = 1,
      ContactFrequency = 30f,
      ContactDamping = 1f,
      ContactHitEventThreshold = 0.01f,
      Enabled = false
    };

    public PhysicsWorldDefinition ToDefinition()
    {
      return new PhysicsWorldDefinition
      {
        gravity = Gravity,
        transformPlane = PhysicsWorld.TransformPlane.XY,
        simulateType = PhysicsWorld.SimulationType.Script,
        simulationSubSteps = SimulationSubSteps,
        contactFrequency = ContactFrequency,
        contactDamping = ContactDamping,
        contactHitEventThreshold = ContactHitEventThreshold
      };
    }
  }
}