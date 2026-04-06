using UnityEngine;

[RequireComponent(typeof(HauntableToy))]
public class HauntedToyBasicMovement : MonoBehaviour
{
    //Settigns
    [SerializeField] float movementSpeed = 1.0f;
    [SerializeField] float stoppingDistance = 0.1f;

    //Variables
    HauntedToyWaypoint currentWaypoint;

    //Refrences
    HauntableToy toy;

    //Logic
    private void Awake()
    {
        toy = GetComponent<HauntableToy>();
        toy.onWaypointSet += OnNewWaypoint;
    }

    private void Update()
    {
        if (currentWaypoint == null)
            return;

        if (Vector3.Distance(transform.position, currentWaypoint.transform.position) > stoppingDistance)
        {
            Move();
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

        Vector3 direction = (currentWaypoint.transform.position - transform.position).normalized;
        transform.Translate(direction * movementSpeed * Time.deltaTime);
    }
}
