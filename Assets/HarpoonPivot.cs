using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HarpoonPivot : MonoBehaviour
{
    bool isGrabbed = false;
    
    Vector3 startScale = Vector3.one;

    [SerializeField] Transform PivotPoint;
    [SerializeField] float hapticIntensity;
    [SerializeField] float impulseDistance = 0.1f;
    Vector3 lastPivotDirection = Vector3.zero;

    void Awake()
    {
        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbed && PivotPoint != null)
        {
            Vector3 direction = PivotPoint.position - transform.position;
            PivotPoint.forward = direction.normalized;
            if (Vector3.Distance(lastPivotDirection, PivotPoint.forward) > impulseDistance)
            {
                HapticsUtility.SendHapticImpulse(hapticIntensity, 0.05f, HapticsUtility.Controller.Right);
                lastPivotDirection = PivotPoint.forward;
            }
        }
    }

    public void startGrab()
    {
        isGrabbed = true;
    }

    public void stopGrab()
    {
        isGrabbed = false;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = startScale;
    }
}
