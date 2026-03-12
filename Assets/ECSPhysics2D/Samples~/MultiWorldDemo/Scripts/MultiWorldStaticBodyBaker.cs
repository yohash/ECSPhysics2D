using Unity.Entities;
using Unity.Mathematics;

namespace ECSPhysics2D.Samples.MultiWorldDemo
{
  /// <summary>
  /// Bakes MultiWorldStaticBodyAuthoring into ECS components.
  /// </summary>
  public class MultiWorldStaticBodyBaker : Baker<MultiWorldStaticBodyAuthoring>
  {
    public override void Bake(MultiWorldStaticBodyAuthoring authoring)
    {
      var entity = GetEntity(TransformUsageFlags.Dynamic);

      // Multi-world static body component
      AddComponent(entity, new MultiWorldStaticBodyComponent
      {
        WorldMask = authoring.GetWorldMask()
      });

      // Static tag (though this body is managed by MultiWorldStaticBodyInitializationSystem)
      AddComponent<PhysicsStaticTag>(entity);

      // Add shape component based on type
      switch (authoring.Shape) {
        case MultiWorldStaticBodyAuthoring.ShapeType.Box:
          AddComponent(entity, new PhysicsShapeBox
          {
            Size = authoring.BoxSize,
            Center = float2.zero,
            Rotation = 0f
          });
          break;

        case MultiWorldStaticBodyAuthoring.ShapeType.Circle:
          AddComponent(entity, new PhysicsShapeCircle
          {
            Radius = authoring.CircleRadius,
            Center = float2.zero
          });
          break;
      }

      // Default material for static bodies
      AddComponent(entity, new PhysicsMaterial
      {
        Friction = 0.5f,
        Bounciness = 0.3f,
        Density = 0f  // Static bodies don't need density
      });
    }
  }
}
