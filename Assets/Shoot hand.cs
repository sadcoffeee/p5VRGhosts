using UnityEngine;
using UnityEngine.InputSystem;

public class Shoothand : MonoBehaviour
{
    public InputActionReference triggerPulled;
    
    public GameObject shootingHand; //obejctb i want to shoot
    public Transform spawnPoint; // Where to spawn prefab
    private float shootForce = 200f;
    
    
    public float FireCooldown = 1f;
    private float CurrentCooldown = 0f;

    float triggerPressed;

    void Update()
    {
        //update timer cooldown
        if (CurrentCooldown > 0f)
        { CurrentCooldown -= Time.deltaTime; }


        triggerPressed = triggerPulled.action.ReadValue<float>();

        if (triggerPressed != 0 && CurrentCooldown <= 0f)
        {

            Shoot();

            CurrentCooldown = FireCooldown;

            Debug.Log("Jeg bliver trykket på");
          
        }

    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(shootingHand, spawnPoint.position, spawnPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = spawnPoint.forward * shootForce;

        AttachSpringJoint(bullet);
    }

    private void AttachSpringJoint(GameObject bullet)
    {
        //make sure the bullet has a rigid body
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null) return;

        
        //Connect the join to the spawnpoint
        Rigidbody spawnRb = spawnPoint.GetComponent<Rigidbody>();
        if (spawnRb == null)
        {
            // If the spawnPoint do not have a Rigidbody, add one (kinematic so it do not fall)
            spawnRb = spawnPoint.gameObject.AddComponent<Rigidbody>();
            spawnRb.isKinematic = true;
        }

        //add a spring joint to the bullet
        SpringJoint joint = bullet.AddComponent<SpringJoint>();
        joint.connectedBody = spawnRb;

        //spring properties
        joint.spring = 1000f;       // how strong it pulls back, Higher values = bullet gets pulled back faster and harder
        joint.damper = 1f;         // How much the spring slows down movement. Higher values = less bouncing and wobbling.
        joint.minDistance = 0f;    // The shortest distance allowed between bullet and spawn point.
        joint.maxDistance = 2f;    // The farthest distance the bullet can stretch away from the spawn point. Once bullet reaches this distance, the spring starts pulling it back.
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = Vector3.zero; // attach to center of spawnPoint
        joint.anchor = Vector3.zero;          // attach to center of bullet
    }
}
