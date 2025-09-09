using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuDisplay : MonoBehaviour
{
	public GameObject targetCanvas;
	public InputActionProperty toggleAction; // Assign in Inspector

	private bool isVisible = true;

	void OnEnable() => toggleAction.action.Enable();
	void OnDisable() => toggleAction.action.Disable();

	void Update()
	{
		if (toggleAction.action.WasPressedThisFrame())
		{
			isVisible = !isVisible;
			targetCanvas.SetActive(isVisible);
		}
	}
}
