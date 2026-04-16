using UnityEngine;

[RequireComponent(typeof(HauntableToy), typeof(Rigidbody))]
public class HauntedToyCarMovement : MonoBehaviour
{
    //Settigns
    [SerializeField] float drivingForce = 1.0f;
    [SerializeField] float stoppingDistance = 0.1f;
    [SerializeField] float rotationSpeed = 0.5f;

    [Header("Sounds")]
    [SerializeField] string onWaypointSound = "";

    //Variables
    HauntedToyWaypoint currentWaypoint;

    //Refrences
    HauntableToy toy;
    Rigidbody rb;

    //Logic
    private void Awake()
    {
        toy = GetComponent<HauntableToy>();
        toy.onWaypointSet += OnNewWaypoint;

        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (currentWaypoint == null)
            return;

        if (toy.DistanceToWaypoint() > stoppingDistance && toy.IsHaunted())
        {
            Move();
        }
    }

    //Methods
    public virtual void OnNewWaypoint(HauntedToyWaypoint newWaypoint)
    {
        currentWaypoint = newWaypoint;

        if (onWaypointSound != "") AudioManager.Instance.PlayAudioAtPosition(onWaypointSound, transform.position);
    }

    void Move()
    {
        if (currentWaypoint == null)
            return;

        Vector3 direction = (toy.GetProjectedWaypoint(currentWaypoint) - transform.position).normalized;

        transform.forward = Vector3.Slerp(transform.forward, direction, rotationSpeed);
        rb.AddForce(transform.forward * drivingForce);
    }
}
