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
	private double currentTool = 0;
	private int previousTool = -1; // Track previous tool to detect changes
	private int lastSelectedTool = 1; // Remember the last selected tool (default to Point)

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
			if (currentTool > 0) lastSelectedTool = currentTool; // Remember the selected tool
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
		//Change current tool based on analog input
		if (currentTool != 0 && analogValue.x > 0.7f && analogValue.y < 0.5f && analogValue.y > -0.5f && !analogInUse)
		{
			currentTool += 1;
			if (currentTool > 6) currentTool = 1;
			analogInUse = true;
		}
		if (currentTool != 0 && analogValue.x < -0.7f && analogValue.y < 0.5f && analogValue.y > -0.5f && !analogInUse)
		{
			currentTool -= 1;
			if (currentTool < 1) currentTool = 6;
			analogInUse = true;
		}

		//Reset Analog In Use
		if (analogValue.x < 0.5f && analogValue.x > -0.5f && analogValue.y < 0.5f && analogValue.y > -0.5f)
			analogInUse = false;

		//Selection if tool is 0
		if (currentTool == 0 && analogValue.x < -0.7f && Mathf.Abs(analogValue.y) < 0.5f)
		{
			currentTool = 1; analogInUse = true;
		}
		if (currentTool == 0 && analogValue.x > 0.7f && Mathf.Abs(analogValue.y) < 0.5f)
		{
			currentTool = 3; analogInUse = true;
		}
		if (currentTool == 0 && Mathf.Abs(analogValue.x) < 0.5f && analogValue.y > 0.7f)
		{
			currentTool = 2; analogInUse = true;
		}

		// === Tool Display Logic ===
		switch (currentTool)
		{
			case 1:
				ToolMenuDisplay.sprite = PointSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, -60f);
				break;

			case 2:
				ToolMenuDisplay.sprite = LineSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 0f);
				if (analogValue.y >= 0.5f && analogValue.x <= -0.5f)
				{ 
					ToolMenuDisplay.sprite = LinelineSelected;
					currentTool = 2.1;
				}
				if (analogValue.y > 0.7f && Mathf.Abs(analogValue.x)
				{ 
					< 0.5f) ToolMenuDisplay.sprite = ParallelLineSelected;
					currentTool = 2.2;
				}
				if (analogValue.y >= 0.5f && analogValue.x >= 0.5f) 
				{
					ToolMenuDisplay.sprite = AngleLineSelected;
					currentTool = 2.3;
				}
				break;

			case 3:
				ToolMenuDisplay.sprite = ArcSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 60f);
				break;

			case 4:
				ToolMenuDisplay.sprite = CircleSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 120f);
				break;

			case 5:
				ToolMenuDisplay.sprite = RectangleSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 180f);
				if (analogValue.y >= 0.5f && analogValue.x < 0f)
				{
					ToolMenuDisplay.sprite = FromCenterSelected; 
					currentTool = 5.1;
				}
				if (analogValue.y >= 0.5f && analogValue.x > 0f)
				{ 
					ToolMenuDisplay.sprite = FromCornerSelected; 
					currentTool = 5.2;
				}
				break;

			case 6:
				ToolMenuDisplay.sprite = PolygonSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, -120f);
				break;

			default:
				ToolMenuDisplay.sprite = PlainToolMenu;
				break;
		}

		// Log tool changes
		if (currentTool != previousTool)
		{
			string[] toolNames = { "None", "Point", "Line", "Arc", "Circle", "Rectangle", "Polygon" };
			string toolName = currentTool < toolNames.Length ? toolNames[currentTool] : "Unknown";
			Debug.Log("Tool selected: " + toolName);
			previousTool = currentTool;
			previousLineSubTool = -1; // Reset sub-tool tracking when main tool changes
		}
		
		// Log line sub-tool changes
		if (currentTool == 2)
		{
			int currentSubTool = 0;
			if (analogValue.y >= 0.5f && analogValue.x <= -0.5f) currentSubTool = 1; // LinelineSelected
			else if (analogValue.y > 0.7f && Mathf.Abs(analogValue.x) < 0.5f) currentSubTool = 2; // ParallelLineSelected
			else if (analogValue.y >= 0.5f && analogValue.x >= 0.5f) currentSubTool = 3; // AngleLineSelected
			
			if (currentSubTool != previousLineSubTool)
			{
				string[] subToolNames = { "Default Line", "Line-Line", "Parallel Line", "Angle Line" };
				string subToolName = currentSubTool < subToolNames.Length ? subToolNames[currentSubTool] : "Unknown";
				Debug.Log("Line sub-tool: " + subToolName);
				previousLineSubTool = currentSubTool;
			}
		}
	}

	// === Public Method to Get Current Tool ===
	public int GetCurrentTool()
	{
		return currentTool > 0 ? currentTool : lastSelectedTool;
	}
	
	// === Public Method to Get Line Sub-Tool ===
	private int previousLineSubTool = -1;
	
	public int GetLineSubTool()
	{
		if (currentTool == 2) // Only for Line tool
		{
			if (analogValue.y >= 0.5f && analogValue.x <= -0.5f) return 1; // LinelineSelected
			if (analogValue.y > 0.7f && Mathf.Abs(analogValue.x) < 0.5f) return 2; // ParallelLineSelected
			if (analogValue.y >= 0.5f && analogValue.x >= 0.5f) return 3; // AngleLineSelected
		}
		return 0; // Default line
	}

}
