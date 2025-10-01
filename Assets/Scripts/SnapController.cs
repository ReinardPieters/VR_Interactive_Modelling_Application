using UnityEngine;
using UnityEngine.InputSystem;

public class SnapController : MonoBehaviour
{
    public InputActionProperty leftTrigger;
    public InputActionProperty rightTrigger;
    public bool snapDefault = true; // snap ON normally

    public bool UseSnap
    {
        get
        {
            bool left = leftTrigger.action.IsPressed();
            bool right = rightTrigger.action.IsPressed();
            // If both triggers held, disable snap temporarily
            if (left && right) return false;
            return snapDefault;
        }
    }

    void OnEnable()
    {
        leftTrigger.action.Enable();
        rightTrigger.action.Enable();
    }

    void OnDisable()
    {
        leftTrigger.action.Disable();
        rightTrigger.action.Disable();
    }
}
