using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRLineDrawerOpenXR : MonoBehaviour
{
    public InputActionProperty triggerAction; 
    public Transform rightController;

    private bool isDrawing = false;

    void OnEnable()
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
        triggerAction.action.canceled += OnTriggerReleased;
    }

    void OnDisable()
    {
        triggerAction.action.Disable();
        triggerAction.action.performed -= OnTriggerPressed;
        triggerAction.action.canceled -= OnTriggerReleased;
    }

    private void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        isDrawing = true;
        Debug.Log("Trigger Pressed!");
        // Raycast and store startPoint
    }

    private void OnTriggerReleased(InputAction.CallbackContext ctx)
    {
        if (!isDrawing) return;
        isDrawing = false;
        Debug.Log("Trigger Released!");
        // Finalize line
    }
}
