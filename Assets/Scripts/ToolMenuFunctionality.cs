using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ToolMenuFunctionality : MonoBehaviour
{
	// === Tool Menu Sprites ===
	public Sprite PlainToolMenu;
	public Sprite PointSelected;
	public Sprite LineSelected;
	public Sprite LinelineSelected;
	public Sprite ParallelLineSelected;
	public Sprite AngleLineSelected;
	public Sprite ArcSelected;
	public Sprite CircleSelected;
	public Sprite RectangleSelected;
	public Sprite FromCenterSelected;
	public Sprite FromCornerSelected;
	public Sprite PolygonSelected;

	// === Display Elements ===
	public Image ToolMenuDisplay;
	public RectTransform target;

	// === Input Actions ===
	public InputActionProperty analogAction;
	private Vector2 analogValue;
	private bool analogInUse = false;

	public InputActionProperty gripAction;

	// === Menu State ===
	private bool isVisible = false;
	public GameObject canvasToToggle;

	// === Tool Selection Tracking ===
	private double currentTool = 0;          // Use double now
	private double previousTool = -1;        // Track previous tool to detect changes
	private double lastSelectedTool = 1;     // Remember the last selected tool (default to Point)

	// === Controller Transform ===
	public Transform controllerTransform;

	// === Enable / Disable Input Actions ===
	void OnEnable()
	{
		analogAction.action.Enable();
		gripAction.action.Enable();
	}

	void OnDisable()
	{
		analogAction.action.Disable();
		gripAction.action.Disable();
	}

	void Start()
	{
		canvasToToggle.SetActive(isVisible);
		ToolMenuDisplay.sprite = PlainToolMenu;
	}

	void Update()
	{
		// === Toggle Menu Visibility ===
		if (gripAction.action.WasPressedThisFrame())
		{
			isVisible = true;
			canvasToToggle.SetActive(isVisible);
		}

		if (gripAction.action.WasReleasedThisFrame())
		{
			isVisible = false;
			canvasToToggle.SetActive(isVisible);
			ToolMenuDisplay.sprite = PlainToolMenu;
			if (currentTool > 0) lastSelectedTool = currentTool; // Remember selected tool
			currentTool = 0;
			target.rotation = Quaternion.Euler(0, 0, 0f);
		}

		// === Handle Tool Selection ===
		if (isVisible)
		{
			analogValue = analogAction.action.ReadValue<Vector2>();
			HandleAnalogInput();
		}
	}

	private void HandleAnalogInput()
	{
		// === Change main tool (1, 2, 3, 4, 5, 6) if no sub-tool yet ===
		if ((currentTool == 1 || currentTool == 2 || currentTool == 3 || currentTool == 4 || currentTool == 5 || currentTool == 6) && !analogInUse)
		{
			if (analogValue.x > 0.7f && Mathf.Abs(analogValue.y) < 0.5f) { currentTool += 1; if (currentTool > 6) currentTool = 1; analogInUse = true; }
			if (analogValue.x < -0.7f && Mathf.Abs(analogValue.y) < 0.5f) { currentTool -= 1; if (currentTool < 1) currentTool = 6; analogInUse = true; }
		}

		if (Mathf.Abs(analogValue.x) < 0.5f && Mathf.Abs(analogValue.y) < 0.5f)
			analogInUse = false;

		// === Select main tool if currentTool is 0 ===
		if (currentTool == 0)
		{
			if (analogValue.x < -0.7f && Mathf.Abs(analogValue.y) < 0.5f) { currentTool = 1; analogInUse = true; }
			if (analogValue.y > 0.7f && Mathf.Abs(analogValue.x) < 0.5f) { currentTool = 2; analogInUse = true; } // Must pick Line sub-tool
			if (analogValue.x > 0.7f && Mathf.Abs(analogValue.y) < 0.5f) { currentTool = 3; analogInUse = true; }
			if (analogValue.x > 0.7f && analogValue.y > 0.5f) { currentTool = 5; analogInUse = true; } // Rectangle sub-tool
			if (analogValue.x > 0.7f && analogValue.y < -0.5f) { currentTool = 6; analogInUse = true; }
		}

		// === Tool Display Logic ===
		if (currentTool == 1) // Point
		{
			ToolMenuDisplay.sprite = PointSelected;
			target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, -60f);
		}
		else if (currentTool >= 2 && currentTool < 3) // Line sub-tools
		{
			target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 0f);
			ToolMenuDisplay.sprite = LineSelected;
			if (analogValue.y >= 0.5f && analogValue.x <= -0.5f) { currentTool = 2.1; ToolMenuDisplay.sprite = LinelineSelected; }
			else if (analogValue.y > 0.7f && Mathf.Abs(analogValue.x) < 0.5f) { currentTool = 2.2; ToolMenuDisplay.sprite = ParallelLineSelected; }
			else if (analogValue.y >= 0.5f && analogValue.x >= 0.5f) { currentTool = 2.3; ToolMenuDisplay.sprite = AngleLineSelected; }
		}
		else if (currentTool == 3) // Arc
		{
			ToolMenuDisplay.sprite = ArcSelected;
			target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 60f);
		}
		else if (currentTool == 4) // Circle
		{
			ToolMenuDisplay.sprite = CircleSelected;
			target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 120f);
		}
		else if (currentTool >= 5 && currentTool < 6) // Rectangle sub-tools
		{
			target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 180f);
			ToolMenuDisplay.sprite = RectangleSelected;
			if (analogValue.y >= 0.5f && analogValue.x < 0f) { currentTool = 5.1; ToolMenuDisplay.sprite = FromCenterSelected; }
			else if (analogValue.y >= 0.5f && analogValue.x > 0f) { currentTool = 5.2; ToolMenuDisplay.sprite = FromCornerSelected; }
		}
		else if (currentTool == 6) // Polygon
		{
			ToolMenuDisplay.sprite = PolygonSelected;
			target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, -120f);
		}

		// === Log current tool ===
		if (currentTool != previousTool)
		{
			Debug.Log("Tool selected: " + currentTool.ToString("0.0"));
			previousTool = currentTool;
		}
	}

	// === Public Method to Get Current Tool ===
	public double GetCurrentTool()
	{
		return currentTool > 0 ? currentTool : lastSelectedTool;
	}
}
