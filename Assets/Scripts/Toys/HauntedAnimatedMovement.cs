using UnityEngine;

[RequireComponent(typeof(HauntableToy), typeof(Rigidbody))]
public class HauntedAnimatedMovement : MonoBehaviour
{
    //Settigns
    [SerializeField] float movementSpeed = 1.0f;
    [SerializeField] float stoppingDistance = 0.1f;

    //Variables
    HauntedToyWaypoint currentWaypoint;

    //Refrences
    [Header("Refrences")]
    HauntableToy toy;
    Rigidbody rb;
    [SerializeField] Animator animator;

    //Logic
    private void Awake()
    {
        toy = GetComponent<HauntableToy>();
        toy.onWaypointSet += OnNewWaypoint;
        toy.onHaunted += LockRigidbody;

        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (currentWaypoint == null)
            return;

        if (Vector3.Distance(transform.position, toy.GetProjectedWaypoint(currentWaypoint)) > stoppingDistance)
        {
            Move();
        }
        else
        {
            PlayAnimation(false);
        }
    }

    //Methods
    public virtual void OnNewWaypoint(HauntedToyWaypoint newWaypoint)
    {
        currentWaypoint = newWaypoint;
    }

    void Move()
    {
        if (currentWaypoint == null)
            return;

        Vector3 direction = (toy.GetProjectedWaypoint(currentWaypoint) - transform.position).normalized;
        transform.forward = direction;
        rb.Move(transform.position + direction * movementSpeed * Time.fixedDeltaTime, transform.rotation);
        PlayAnimation(true);
    }

    void PlayAnimation(bool state)
    {
        if( animator!= null)
        {
            animator.SetBool("isMoving", state);
        }
    }

    void LockRigidbody(bool state)
    {
        if (state)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
            currentWaypoint = null;
            PlayAnimation(false);
        }
    }
}
