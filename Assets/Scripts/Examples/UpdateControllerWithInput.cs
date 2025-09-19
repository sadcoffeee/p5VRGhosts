using UnityEngine;
using UnityEngine.InputSystem;

public class UpdateControllerWithInput : MonoBehaviour
{
    public InputActionReference triggerPulled;
    float triggerPressed;

    // Update is called once per frame
    void Update()
    {
        triggerPressed = triggerPulled.action.ReadValue<float>();
    }
}
