using UnityEngine;

public class GazeConeCollider : MonoBehaviour
{
    public GazeVacuum vacuum;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost") || other.CompareTag("Toy"))
            vacuum.RegisterCandidate(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ghost") || other.CompareTag("Toy"))
            vacuum.UnregisterCandidate(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        // Safety: ensure objects spawned inside still get registered
        if (other.CompareTag("Ghost") || other.CompareTag("Toy"))
            vacuum.RegisterCandidate(other.gameObject);
    }
}