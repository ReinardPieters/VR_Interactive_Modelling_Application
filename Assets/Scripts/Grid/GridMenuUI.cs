using UnityEngine;
using UnityEngine.InputSystem;

public class GridMenuUI : MonoBehaviour
{
	public GameObject gridMenuCanvas;
	public InputActionProperty menuButton;

	void OnEnable() => menuButton.action.Enable();
	void OnDisable() => menuButton.action.Disable();

	void Start()
	{
		gridMenuCanvas.SetActive(false);
	}

	void Update()
	{
		if (menuButton.action.WasPressedThisFrame())
		{
			gridMenuCanvas.SetActive(!gridMenuCanvas.activeSelf);
		}
	}
}
