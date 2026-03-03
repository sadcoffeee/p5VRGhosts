using UnityEngine;

public class HarpoonPivot : MonoBehaviour
{
    bool isGrabbed = false;
    
    Vector3 startScale = Vector3.one;

    [SerializeField] Transform PivotPoint;

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
