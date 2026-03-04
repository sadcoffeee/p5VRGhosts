using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    public enum GhostState
    {
        Spawning,
        Lingering,
        Stunned,
        Grabbed,
        FlyingToSteal,
        Defeated,
    }

    [Header("References")]
    [SerializeField] GameObject ghostVisual;
    [SerializeField] Renderer ghostRenderer;
    [SerializeField] GhostAnimations ghostAnimator;
    [SerializeField] ParticleSystem litParticles;

    [Header("Spawning")]
    [SerializeField] float spawnMoveDuration = 0.6f;   // seconds to glide into start position
    private Vector3 spawnStartPosition;
    private float spawnTimer;

    [Header("Lingering")]
    [SerializeField] float lingerDuration = 8f;
    [SerializeField] float driftRadius = 0.4f;
    [SerializeField] float driftInterval = 2f;
    [SerializeField] float driftSpeed = 0.3f;

    [Header("Flashlight Stun")]
    [SerializeField] float stunChargeRequired = 0.5f;
    [SerializeField] float stunDuration = 5f;

    [Header("Flee / Steal")]
    [SerializeField] float flySpeed = 3.5f;
    [SerializeField] float arriveDistance = 0.15f;

    [HideInInspector] public GhostState currentState = GhostState.Spawning;

    // Lingering
    private float lingerTimer;
    private float driftTimer;
    private Vector3 lingerAnchor;
    private Vector3 driftTarget;
    private Transform lookTarget;

    // Stun charge
    private float stunChargeTimer;
    private float stunTimer;

    // Fleeing
    private Transform stealTarget;

    // Grabbing
    [HideInInspector] public bool isGrabbable;

    void Start()
    {
        GazeGameManager.Instance.RegisterGhost(this);

        // Ghost starts invisible
        SetVisualActive(false);
        isGrabbable = false;

        spawnStartPosition = transform.position;
        spawnTimer = 0f;

        lookTarget = Camera.main.transform;
    }

    void Update()
    {
        switch (currentState)
        {
            case GhostState.Spawning:       UpdateSpawning();       break;
            case GhostState.Lingering:      UpdateLingering();      break;
            case GhostState.Stunned:        UpdateStunned();        break;
            case GhostState.FlyingToSteal:  UpdateFlyingToSteal();  break;
            // Grabbed and Defeated are driven externally by Grablinghook
        }
    }

    void UpdateSpawning()
    {
        // Glide from spawn origin to linger anchor, then enter Lingering
        spawnTimer += Time.deltaTime;
        float t = Mathf.Clamp01(spawnTimer / spawnMoveDuration);
        transform.position = Vector3.Lerp(spawnStartPosition, spawnStartPosition, t); // stays put for now

        if (t >= 1f)
            EnterLingering();
    }

    void EnterLingering()
    {
        currentState = GhostState.Lingering;
        lingerAnchor = transform.position;
        driftTarget = transform.position;
        lingerTimer = 0f;
        driftTimer = 0f;
        stunChargeTimer = 0f;
        isGrabbable = false;
        SetVisualActive(false);
        //AudioManager.Instance.PlayAudio("ghostAppear"); // to do: find something for this?
    }

    void UpdateLingering()
    {
        // Count down to fleeing
        lingerTimer += Time.deltaTime;
        if (lingerTimer >= lingerDuration)
        {
            EnterFlyToSteal();
            return;
        }

        // Drift
        driftTimer += Time.deltaTime;
        if (driftTimer >= driftInterval)
        {
            driftTimer = 0f;
            driftTarget = lingerAnchor + Random.insideUnitSphere * driftRadius;
        }
        transform.position = Vector3.MoveTowards(transform.position, driftTarget, driftSpeed * Time.deltaTime);
        transform.LookAt(lookTarget);

    }

    public void NotifyFlashlightHit(float deltaTime)
    {
        if (currentState != GhostState.Lingering) return;

        if (!litParticles.isPlaying) 
            litParticles.Play();

        Debug.Log("Getting hit by flashlight");

        stunChargeTimer += deltaTime;
        if (stunChargeTimer >= stunChargeRequired)
            EnterStunned();
    }

    public void NotifyFlashlightLost()
    {
        if (currentState == GhostState.Lingering)
            stunChargeTimer = 0f;

        if (litParticles.isPlaying)
            litParticles.Stop();
    }

    void EnterStunned()
    {
        currentState = GhostState.Stunned;
        stunTimer = stunDuration;
        isGrabbable = true;
        SetVisualActive(true);


        ghostAnimator?.PlayDizzy();
        AudioManager.Instance.PlayAudio("GhostStunned");
    }

    void UpdateStunned()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
            RecoverFromStun();
    }

    void RecoverFromStun()
    {
        isGrabbable = false;

        AudioManager.Instance.PlayAudio("ghostLaughOther");
        AudioManager.Instance.StopAudio("GhostStunned");
        EnterLingering();
    }

    void EnterFlyToSteal()
    {
        // Ask GameManager for the nearest available toy
        Transform toy = GazeGameManager.Instance.ClaimToyForGhost(this);
        if (toy == null)
        {
            // No toys left; just vanish quietly for now
            Die();
            return;
        }

        stealTarget = toy;
        currentState = GhostState.FlyingToSteal;
        SetVisualActive(true);
        ghostAnimator?.PlayFlying();
        AudioManager.Instance.PlayAudio("ghostLaugh");
    }

    void UpdateFlyingToSteal()
    {
        if (stealTarget == null)
        {
            Die();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, stealTarget.position, flySpeed * Time.deltaTime);
        transform.LookAt(stealTarget.position);

        // Ghost successfully arrives at toy; probably want to change this so it flies to some point out of scene
        if (Vector3.Distance(transform.position, stealTarget.position) <= arriveDistance)
        {
            // Ghost steals the toy and exits
            GazeGameManager.Instance.OnGhostStoleToy(this, stealTarget);
            Die();
        }
    }


    public void OnGrabbed()
    {
        currentState = GhostState.Grabbed;
        isGrabbable = false;
        AudioManager.Instance.StopAudio("GhostStunned");
    }

    void SetVisualActive(bool active)
    {
        if (ghostVisual != null)
            ghostVisual.SetActive(active);
    }

    public void Die()
    {
        GazeGameManager.Instance?.OnGhostDefeated(this, Time.time);
        Destroy(gameObject);
    }
}
