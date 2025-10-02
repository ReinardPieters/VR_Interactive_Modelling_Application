using UnityEngine;
using UnityEngine.InputSystem;

public class ResizeMenuPopUp : MonoBehaviour
{

	public GameObject canvasToToggle; // Assign in Inspector
	private bool isVisible = false;

	public InputActionProperty RightSecondaryButton;

	void OnEnable()
	{
		RightSecondaryButton.action.Enable();
	}

	void OnDisable()
	{
		RightSecondaryButton.action.Disable();
	}
	void Start()
	{
		canvasToToggle.SetActive(isVisible);
	}

	void Update()
	{
		if (RightSecondaryButton.action.WasPressedThisFrame())
		{
			isVisible = !isVisible;
			canvasToToggle.SetActive(isVisible);
		}
	}
}
