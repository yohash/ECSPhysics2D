using Unity.Mathematics;
namespace ECSPhysics2D.Samples.FallingShapes
{
  public static class MouseOverDetector
  {
    private static int _mouseTracker = 0;
    public static bool IsMouseOverUI => _mouseTracker > 0;
    public static void OnMouseEnter() => _mouseTracker++;
    public static void OnMouseLeave() => _mouseTracker = math.max(0, _mouseTracker - 1);
  }
}
