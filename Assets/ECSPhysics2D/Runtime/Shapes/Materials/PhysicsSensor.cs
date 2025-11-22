using Unity.Entities;

namespace ECSPhysics2D
{
  /// <summary>
  /// Component to mark shapes as sensors (triggers).
  /// Sensors detect overlaps but don't cause physical collision response.
  /// </summary>
  public struct PhysicsSensor : IComponentData
  {
    public bool IsSensor;
    public bool ReportEnter;
    public bool ReportStay;
    public bool ReportExit;

    public static PhysicsSensor Default => new PhysicsSensor
    {
      IsSensor = true,
      ReportEnter = true,
      ReportStay = false,
      ReportExit = true
    };
  }
}