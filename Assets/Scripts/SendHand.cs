using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SendHand : MonoBehaviour
{
    public InputActionReference triggerPulled; //reference to input action
    
    public GameObject bulletPrefab;
    public Transform spawnPoint;
    private float shootSpeed = 10f;
    public float FireCooldown = 1f;

    private float cooldownTimer = 0f;

    void Update()
    {
        //Reduce the cooldown timer each update
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        //reads trigger input
        float triggerPressed = triggerPulled.action.ReadValue<float>();
        
        //if trigger is pressed and cooldown finished, then shoot
        if (triggerPressed > 0f && cooldownTimer <= 0f)
        {
            ShootRay(); //launch ray bullet
            cooldownTimer = FireCooldown; //reset cooldown
        }
    }

    void ShootRay()
    {
        //create a ray from spawnpoint and forward
        Ray ray = new Ray(spawnPoint.position, spawnPoint.forward);
        RaycastHit hit;

        Vector3 targetPoint;

        //check of the ray hits any collider
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point; // hit something, set target
        }
        else
        {
            targetPoint = spawnPoint.position + spawnPoint.forward * 20f; //no hit, set target infront
        }

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

        // Start coroutine to move it
        StartCoroutine(MoveBullet(bullet, targetPoint));
    }

    IEnumerator MoveBullet(GameObject bullet, Vector3 target)
    {
        // Track start position (hand) and end position (target)
        Vector3 startPos = bullet.transform.position;
        Vector3 endPos = target;

        bool returning = false; //tracks is the bullet is returning

        while (bullet != null)
        {
            //decide where the bullet should move, forward or back
            Vector3 destination = returning ? spawnPoint.position : endPos;

            //move the bullet a small step forward
            float step = shootSpeed * Time.deltaTime;
            bullet.transform.position = Vector3.MoveTowards(bullet.transform.position, destination, step);

            // Check if reached destination
            if (Vector3.Distance(bullet.transform.position, destination) < 0.01f)
            {
                if (!returning)
                {
                    returning = true; // reverse direction
                }
                else
                {
                    Destroy(bullet); // reached hand
                    yield break; //exit coroutine
                }
            }

            yield return null;
        }
    }
}

