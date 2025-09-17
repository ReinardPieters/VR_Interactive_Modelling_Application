using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleCanvasXR : MonoBehaviour
{
	public GameObject canvasToToggle; // Assign in Inspector
	private bool isVisible = true;

	public InputActionProperty leftGripAction;

	void OnEnable()
	{
		leftGripAction.action.Enable();
	}

	void OnDisable()
	{
		leftGripAction.action.Disable();
	}

	void Update()
	{
		if (leftGripAction.action.WasPressedThisFrame())
		{
			isVisible = !isVisible;
			canvasToToggle.SetActive(isVisible);
		}
	}
}
