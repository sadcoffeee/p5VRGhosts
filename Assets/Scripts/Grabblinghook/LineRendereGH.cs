using UnityEngine;

public class LineRendereGH : MonoBehaviour
{
    private LineRenderer lr;
    [SerializeField] Transform handTransform;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (handTransform != null)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, handTransform.position);
        }
    }
}
