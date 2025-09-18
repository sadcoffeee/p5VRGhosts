using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TriggerGrabbableEffects : MonoBehaviour
{
    private XRGrabInteractable grab;

    public GameObject effectsHolder;
    private ParticleSystem effectParticles;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        effectParticles = effectsHolder.GetComponent<ParticleSystem>();

        grab.hoverEntered.AddListener(OnHoverEnter);
        grab.hoverExited.AddListener(OnHoverExit);
        grab.selectEntered.RemoveListener(OnGrab);
    }
        void OnDestroy()
    {
        grab.hoverEntered.RemoveListener(OnHoverEnter);
        grab.hoverExited.RemoveListener(OnHoverExit);
        grab.selectEntered.RemoveListener(OnGrab);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (effectsHolder != null) 
        {
            effectsHolder.SetActive(true);
        }
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (effectsHolder != null)
        {
            effectsHolder.SetActive(false);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (effectsHolder != null)
        {
            effectsHolder.SetActive(false);
        }
    }
}
