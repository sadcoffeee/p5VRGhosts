using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class InteractableEvents : MonoBehaviour
{
    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        // triggers when hand interactor hovers over object
        grab.hoverEntered.AddListener(OnHoverEnter);
        grab.hoverExited.AddListener(OnHoverExit);
        
        // triggers when you hold the grip button while in contact with or hovering over object
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        // triggers when you press the trigger while holding the object
        grab.activated.AddListener(OnActivate);
        grab.deactivated.AddListener(OnDeactivate);
    }

    void OnDestroy()
    {
        // Always unsubscribe!
        grab.hoverEntered.RemoveListener(OnHoverEnter);
        grab.hoverExited.RemoveListener(OnHoverExit);
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
        grab.activated.RemoveListener(OnActivate);
        grab.deactivated.RemoveListener(OnDeactivate);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log("Hover start: " + gameObject.name);
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        Debug.Log("Hover end: " + gameObject.name);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Grabbed " + gameObject.name);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log("Released " + gameObject.name);
    }

    private void OnActivate(ActivateEventArgs args)
    {
        Debug.Log("Activated " + gameObject.name);
    }

    private void OnDeactivate(DeactivateEventArgs args)
    {
        Debug.Log("Deactivated " + gameObject.name);
    }
}
