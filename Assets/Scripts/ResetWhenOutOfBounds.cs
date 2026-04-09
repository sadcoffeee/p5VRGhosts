using UnityEngine;

public class ResetWhenOutOfBounds : MonoBehaviour
{
    [SerializeField] float maxDistanceFromStartingPosition = 100f;

    Vector3 startPosition;
    Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) > maxDistanceFromStartingPosition)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}
