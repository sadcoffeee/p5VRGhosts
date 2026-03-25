using UnityEngine;

public class ConeCollider : MonoBehaviour
{
    public Vacuum vacuum; //the vacuum script

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            vacuum.StartSucking(other.gameObject);
            Debug.Log("You hit something");
        }
    }
   
}
