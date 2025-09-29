using UnityEngine;

public class Grablinghand : MonoBehaviour
{
    private GameObject touched;

    private void OnCollisionEnter(Collision collision)
    {
        touched = collision.gameObject;
    }

    public GameObject HandTouched()
    {
        return touched;
    }
}
