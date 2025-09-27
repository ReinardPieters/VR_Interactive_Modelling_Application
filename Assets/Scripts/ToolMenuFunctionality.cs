using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToolMenuFunctionality : MonoBehaviour
{
	//Vars for images
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

	//Display Image
	public Image ToolMenuDisplay;
	public RectTransform target;

	//Analog Input
	public InputActionProperty analogAction;
	private Vector2 analogValue;
	private bool analogInUse = false;

	//Grip Button Input
	public InputActionProperty gripAction;

	//Bool to track if menu is visible
	private bool isVisible = false;
	public GameObject canvasToToggle;

	//Var to Track Current Tool
	private int currentTool = 0;

	//Controller
	public Transform controllerTransform;

	//Enable /Disable Input Actions
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

	//void for Start
	void Start()
	{
		canvasToToggle.SetActive(isVisible);
		ToolMenuDisplay.sprite = PlainToolMenu;
	}

	//Update function
	void Update()
	{
		//Toggle Menu Visibility
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
			currentTool = 0;
			target.rotation = Quaternion.Euler(0, 0, 0f);
		}
		//If Menu is Visible, Check Analog Input
		if (isVisible)
		{
			analogValue = analogAction.action.ReadValue<Vector2>();

			//Change current tool based on analog input
			if (currentTool != 0 && analogValue.x > 0.7f && analogValue.y < 0.5f && analogValue.y > -0.5f && !analogInUse)
			{
				currentTool += 1;
				if (currentTool > 6)
				{
					currentTool = 1;
				}
				analogInUse = true;
			}
			if (currentTool != 0 && analogValue.x < -0.7f && analogValue.y < 0.5f && analogValue.y > -0.5f && !analogInUse)
			{
				currentTool += -1;
				if (currentTool < 1)
				{
					currentTool = 6;
				}
				analogInUse = true;
			}
			//Reset Analog In Use
			if (analogValue.x < 0.5f && analogValue.x > -0.5f && analogValue.y < 0.5f && analogValue.y > -0.5f)
			{
				analogInUse = false;
			}

			//Determine which tool is selected based on analog input
			//Selection if tool is 0
			if (currentTool == 0 && analogValue.x < -0.7f && analogValue.y < 0.5f && analogValue.y > -0.5f)
			{
				currentTool = 1;
				analogInUse = true;
			}
			if (currentTool == 0 && analogValue.x > 0.7f && analogValue.y < 0.5f && analogValue.y > -0.5f)
			{
				currentTool = 3;
				analogInUse = true;
			}
			if (currentTool == 0 && analogValue.x < 0.5f && analogValue.x > -0.5f && analogValue.y > 0.7f)
			{
				currentTool = 2;
				analogInUse = true;
			}

			//Change display based on current tool
			if (currentTool == 1)
			{
				ToolMenuDisplay.sprite = PointSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, -60f);
			}
			if (currentTool == 2)
			{
				ToolMenuDisplay.sprite = LineSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 0f);

				//check for sub tools
				if (analogValue.y >= 0.5f && analogValue.x <= -0.5f)
				{
					ToolMenuDisplay.sprite = LinelineSelected;
				}
				if (analogValue.y > 0.7f && analogValue.x > -0.5f && analogValue.x < 0.5f)
				{
					ToolMenuDisplay.sprite = ParallelLineSelected;
				}
				if (analogValue.y >= 0.5f && analogValue.x >= 0.5f)
				{
					ToolMenuDisplay.sprite = AngleLineSelected;
				}

			}
			if (currentTool == 3)
			{
				ToolMenuDisplay.sprite = ArcSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 60f);
			}
			if (currentTool == 4)
			{
				ToolMenuDisplay.sprite = CircleSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 120f);
			}
			if (currentTool == 5)
			{
				ToolMenuDisplay.sprite = RectangleSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, 180f);

				//Check for sub tools
				if (analogValue.y >= 0.5f && analogValue.x < 0f)
				{
					ToolMenuDisplay.sprite = FromCenterSelected;
				}
				if (analogValue.y >= 0.5f && analogValue.x > 0f)
				{
					ToolMenuDisplay.sprite = FromCornerSelected;
				}
			}
			if (currentTool == 6)
			{
				ToolMenuDisplay.sprite = PolygonSelected;
				target.rotation = controllerTransform.rotation * Quaternion.Euler(0, 0, -120f);
			}
		}
	}
}
