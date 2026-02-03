using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ECSPhysics2D.Samples.FallingShapes
{
  /// <summary>
  /// UI Toolkit controller for FallingShapes sample.
  /// Creates UserShapeCreationRequest when spacebar or spawn button is held.
  /// Throttling is handled by FallingShapesInputDetectionSystem.
  /// </summary>
  [RequireComponent(typeof(UIDocument))]
  public class FallingShapesUIController : MonoBehaviour
  {
    [SerializeField] private UIDocument uiDocument;
    private VisualElement spawnButtonRoot;
    private Button spawnButton;

    private const string PressedClass = "pressed";

    private bool isMouseDown = false;

    private void OnEnable()
    {
      if (uiDocument == null) {
        Debug.LogError("No UIDocument assigned to FallingShapesUIController");
        return;
      }

      var root = uiDocument.rootVisualElement;
      spawnButton = root.Q<Button>("spawn-button");
      spawnButtonRoot = root.Q<VisualElement>("spawn-button");

      spawnButtonRoot.RegisterCallback<MouseEnterEvent>(evt => MouseOverDetector.OnMouseEnter());
      spawnButtonRoot.RegisterCallback<MouseLeaveEvent>(evt => MouseOverDetector.OnMouseLeave());
      spawnButtonRoot.RegisterCallback<MouseDownEvent>(evt => isMouseDown = true, TrickleDown.TrickleDown);
      spawnButtonRoot.RegisterCallback<MouseUpEvent>(evt => isMouseDown = false);
    }

    private void Update()
    {
      if (spawnButton == null)
        return;

      bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
      bool shouldSpawn = spacePressed || isMouseDown;

      if (shouldSpawn) {
        CreateUserShapeCreationRequest();
      }

      // Visual feedback
      if (shouldSpawn && !spawnButton.ClassListContains(PressedClass)) {
        spawnButton.AddToClassList(PressedClass);
      } else if (!shouldSpawn && spawnButton.ClassListContains(PressedClass)) {
        spawnButton.RemoveFromClassList(PressedClass);
      }
    }

    private void CreateUserShapeCreationRequest()
    {
      var world = World.DefaultGameObjectInjectionWorld;
      if (world == null || !world.IsCreated)
        return;

      var entityManager = world.EntityManager;
      var requestEntity = entityManager.CreateEntity();
      entityManager.AddComponentData(requestEntity, new UserShapeCreationRequest());
    }
  }
}
