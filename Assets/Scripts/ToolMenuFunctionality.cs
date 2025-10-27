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
	private int currentTool = 0;
	private int previousTool = -1; // Track previous tool to detect changes
	private int lastSelectedTool = 1; // Remember the last selected tool (default to Point)

	// === Controller Transform ===
	public Transform controllerTransform;

	// === XR Locomotion Lock ===
	public GameObject xrOrigin; // Assign your XR Origin (XRRig) in the Inspector
	private ContinuousMoveProviderBase moveProvider;
	private ContinuousTurnProviderBase turnProvider;
	private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportProvider;

	// === Camera Lock ===
	private Camera xrCamera;
	private Vector3 lockedCamPosition;
	private Quaternion lockedCamRotation;
	private bool cameraLocked = false;

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

		// Setup XR providers
		if (xrOrigin)
		{
			moveProvider = xrOrigin.GetComponentInChildren<ContinuousMoveProviderBase>();
			turnProvider = xrOrigin.GetComponentInChildren<ContinuousTurnProviderBase>();
			teleportProvider = xrOrigin.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
		}

		xrCamera = Camera.main;
	}

	void Update()
	{
		// === Toggle Menu Visibility ===
		if (gripAction.action.WasPressedThisFrame())
		{
			isVisible = true;
			canvasToToggle.SetActive(isVisible);
			LockXRMovement(true);
			LockXRCamera(true);
		}

		if (gripAction.action.WasReleasedThisFrame())
		{
			isVisible = false;
			canvasToToggle.SetActive(isVisible);
			ToolMenuDisplay.sprite = PlainToolMenu;
			if (currentTool > 0) lastSelectedTool = currentTool; // Remember the selected tool
			currentTool = 0;
			target.rotation = Quaternion.Euler(0, 0, 0f);
			LockXRMovement(false);
			LockXRCamera(false);
		}

		// === Handle Tool Selection ===
		if (isVisible)
		{
			analogValue = analogAction.action.ReadValue<Vector2>();
			HandleAnalogInput();
		}

		// === Keep Camera Locked ===
		if (cameraLocked && xrCamera != null)
		{
			xrCamera.transform.position = lockedCamPosition;
			xrCamera.transform.rotation = lockedCamRotation;
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
				if (analogValue.y >= 0.5f && analogValue.x <= -0.5f) ToolMenuDisplay.sprite = LinelineSelected;
				if (analogValue.y > 0.7f && Mathf.Abs(analogValue.x) < 0.5f) ToolMenuDisplay.sprite = ParallelLineSelected;
				if (analogValue.y >= 0.5f && analogValue.x >= 0.5f) ToolMenuDisplay.sprite = AngleLineSelected;
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
				if (analogValue.y >= 0.5f && analogValue.x < 0f) ToolMenuDisplay.sprite = FromCenterSelected;
				if (analogValue.y >= 0.5f && analogValue.x > 0f) ToolMenuDisplay.sprite = FromCornerSelected;
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
		}
	}

	// === Public Method to Get Current Tool ===
	public int GetCurrentTool()
	{
		return currentTool > 0 ? currentTool : lastSelectedTool;
	}

	// === XR Locking Methods ===
	private void LockXRMovement(bool lockMovement)
	{
		if (moveProvider) moveProvider.enabled = !lockMovement;
		if (turnProvider) turnProvider.enabled = !lockMovement;
		if (teleportProvider) teleportProvider.enabled = !lockMovement;
	}

	private void LockXRCamera(bool lockCam)
	{
		if (!xrCamera) return;

		if (lockCam)
		{
			lockedCamPosition = xrCamera.transform.position;
			lockedCamRotation = xrCamera.transform.rotation;
			cameraLocked = true;
		}
		else
		{
			cameraLocked = false;
		}
	}
}
