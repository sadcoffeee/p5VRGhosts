using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ghost"))
        {
            FlyTowardsGhost collidedGhost = collision.gameObject.GetComponent<FlyTowardsGhost>();
            if (collidedGhost.grabbable) 
            {
                collision.gameObject.transform.parent = this.transform;
            }
        }
    }


}
