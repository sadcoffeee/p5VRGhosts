using UnityEngine;

[RequireComponent(typeof(HauntableToy), typeof(Rigidbody))]
public class HauntedToyColliderSwapMovement : MonoBehaviour
{
    //Settigns
    [SerializeField] float rollingForce = 1.0f;
    [SerializeField] float stoppingDistance = 0.1f;

    [Header("Collders")]
    [SerializeField] Collider defaultColldier;
    [SerializeField] Collider moveColldier;

    //Sounds
    [Header("Sounds")]
    [SerializeField] string onWaypointSound = "";
    [SerializeField] string walkingSound = "";
    [SerializeField] float walkingSoundInterval = 1f;

    //Variables
    HauntedToyWaypoint currentWaypoint;
    float walkSoundTimer = 1f;

    //Refrences
    HauntableToy toy;
    Rigidbody rb;

    //Logic
    private void Awake()
    {
        toy = GetComponent<HauntableToy>();
        toy.onWaypointSet += OnNewWaypoint;
        toy.onHaunted += EnableMoveCollider;

        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (currentWaypoint == null)
            return;

        if (toy.DistanceToWaypoint() > stoppingDistance && toy.IsHaunted())
        {
            Move();

            walkSoundTimer -= Time.fixedDeltaTime;
            if (walkSoundTimer <= 0)
            {
                PlayWalkSound();
            }
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

        Vector3 direction = (currentWaypoint.transform.position - transform.position).normalized;
        rb.AddForce(direction * rollingForce);
    }

    void EnableMoveCollider(bool state)
    {
        moveColldier.enabled = state;
        defaultColldier.enabled = !state;
    }

    void PlayWalkSound()
    {
        walkSoundTimer = walkingSoundInterval;

        if (walkingSound == "") return;
        AudioManager.Instance.PlayAudioAtPosition(walkingSound, transform.position);
    }
}
