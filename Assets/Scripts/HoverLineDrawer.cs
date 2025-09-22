using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(LineRenderer))]
public class HoverLineDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private IXRHoverInteractable currentTarget;

    private XRBaseInteractor interactor;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        interactor = GetComponent<XRBaseInteractor>();
        interactor.hoverEntered.AddListener(OnHoverEnter);
        interactor.hoverExited.AddListener(OnHoverExit);
    }

    void OnDestroy()
    {
        interactor.hoverEntered.RemoveListener(OnHoverEnter);
        interactor.hoverExited.RemoveListener(OnHoverExit);
    }

    void Update()
    {
        if (currentTarget != null)
        {
            lineRenderer.SetPosition(0, transform.position); // interactor position
            lineRenderer.SetPosition(1, currentTarget.transform.position); // interactable position
        }
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        currentTarget = args.interactableObject;
        lineRenderer.enabled = true;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (args.interactableObject == currentTarget)
        {
            currentTarget = null;
            lineRenderer.enabled = false;
        }
    }
}
