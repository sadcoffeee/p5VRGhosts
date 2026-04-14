using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    public enum GhostState
    {
        Lingering,
        Stunned,
        Grabbed,
        FlyingToSteal,
        ExitingScene,
    }

    [Header("References")]
    [SerializeField] GameObject ghostVisual;
    [SerializeField] GhostAnimations ghostAnimator;
    [SerializeField] ParticleSystem litParticles;
    [SerializeField] OutlineObject outline;

    [Header("Lingering")]
    [SerializeField] float lingerDuration = 8f;
    [SerializeField] float driftRadius = 0.4f;
    [SerializeField] float driftInterval = 2f;
    [SerializeField] float driftSpeed = 0.3f;

    [Header("Flashlight Stun")]
    [SerializeField] float stunChargeRequired = 0.5f;
    [SerializeField] float stunDuration = 5f;

    [Header("Steal & Fly")]
    [SerializeField] float flyToToySpeed = 3.5f;
    [SerializeField] float arriveDistance = 0.15f;

    [Header("Exit Arc")]
    [SerializeField] Vector3 roomCenter = Vector3.zero;
    [SerializeField] float exitRadius = 10f;
    [SerializeField] float arcHeight = 2f;
    [SerializeField] float exitSpeed = 4f;

    // -------------------------------------------------------------------------
    // Runtime
    // -------------------------------------------------------------------------

    [HideInInspector] public GhostState currentState = GhostState.Lingering;
    [HideInInspector] public bool isGrabbable;
    SpatialAudioEmitter audioEmitter;

    // Lingering
    private float lingerTimer;
    private float driftTimer;
    private Vector3 lingerAnchor;
    private Vector3 driftTarget;
    private Transform lookTarget;

    // Stun
    private float stunChargeTimer;
    private float stunTimer;
    private FlashlightController flashlightController;

    // Steal & exit
    private Transform stolenToyTransform;

    // Exit arc
    private Vector3 arcP0;
    private Vector3 arcP1;
    private Vector3 arcP2;
    private float arcT;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    // Bypass initialization - used for when it is spawned by a haunted toy
    bool bypassInitialization = false;

    void Start()
    {
        lookTarget = Camera.main.transform;
        audioEmitter = GetComponent<SpatialAudioEmitter>();
        flashlightController = FindFirstObjectByType<FlashlightController>();

        // If spawned from a toy, skip rest of initialization
        if (bypassInitialization)
            return;

        GazeGameManager.Instance.RegisterGhost(this);
        EnterLingering();
    }

    void Update()
    {
        outline.enabled = isGrabbable;
        switch (currentState)
        {
            case GhostState.Lingering: UpdateLingering(); break;
            case GhostState.Stunned: UpdateStunned(); break;
            case GhostState.FlyingToSteal: UpdateFlyingToSteal(); break;
            case GhostState.ExitingScene: UpdateExitingScene(); break;
            // Grabbed is driven externally by Vacuum
        }
    }

    // -------------------------------------------------------------------------
    // Lingering
    // -------------------------------------------------------------------------

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
        AudioManager.Instance.PlayAudioAtPosition("ghostSpawn", transform.position); 
        // to do: find more sounds for the ghost so you can tell spawning, flying to steal, getting caught etc apart
    }

    void UpdateLingering()
    {
        // Count down to stealing
        lingerTimer += Time.deltaTime;
        if (lingerTimer >= lingerDuration)
        {
            EnterFlyToSteal();
            return;
        }

        // Drift within radius of anchor
        driftTimer += Time.deltaTime;
        if (driftTimer >= driftInterval)
        {
            driftTimer = 0f;
            driftTarget = lingerAnchor + Random.insideUnitSphere * driftRadius;
        }
        transform.position = Vector3.MoveTowards(transform.position, driftTarget, driftSpeed * Time.deltaTime);
        transform.LookAt(lookTarget);
    }

    // -------------------------------------------------------------------------
    // Stunned
    // -------------------------------------------------------------------------

    void EnterStunned(bool fromFlashlight)
    {
        currentState = GhostState.Stunned;
        stunTimer = stunDuration;
        isGrabbable = true;
        SetVisualActive(true);
        if (litParticles.isPlaying)
            litParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        ghostAnimator?.PlayDizzy();

        if (audioEmitter != null)
            audioEmitter.Play(AudioManager.Instance.GetSound("GhostStunned"));

        if (fromFlashlight)
            flashlightController?.TriggerStunReward();
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

        if (audioEmitter != null)
            audioEmitter.Stop();

        AudioManager.Instance.PlayAudioAtPosition("ghostLaughOther", transform.position);
        EnterLingering();
    }

    // -------------------------------------------------------------------------
    // Fly to steal
    // -------------------------------------------------------------------------

    void EnterFlyToSteal()
    {
        Transform toy = GazeGameManager.Instance.ClaimToyForGhost(this);
        if (toy == null)
        {
            // No toys available, just vanish
            Die(false);
            return;
        }

        stolenToyTransform = toy;
        currentState = GhostState.FlyingToSteal;
        SetVisualActive(true);
        ghostAnimator?.PlayFlying();
        AudioManager.Instance.PlayAudioAtPosition("ghostLaugh", transform.position);
    }

    void UpdateFlyingToSteal()
    {
        if (stolenToyTransform == null)
        {
            Die(false);
            return;
        }
        isGrabbable = true; // last chance to catch while flying towards toy

        transform.position = Vector3.MoveTowards(transform.position, stolenToyTransform.position, flyToToySpeed * Time.deltaTime);
        transform.LookAt(stolenToyTransform.position);

        if (Vector3.Distance(transform.position, stolenToyTransform.position) <= arriveDistance)
            EnterExitingScene();
    }

    // -------------------------------------------------------------------------
    // Exiting scene
    //
    // We define an arc with 3 points:
    //   P0 = ghost's current position (beside the toy)
    //   P2 = roomCenter + (outward horizontal direction * exitRadius)
    //        This guarantees the endpoint is always well outside the room
    //        regardless of where the ghost or toy happened to be.
    //   P1 = midpoint of P0 and P2 lifted by arcHeight
    //        This creates a smooth swooping arc upward then outward.
    // -------------------------------------------------------------------------

    void EnterExitingScene()
    {
        isGrabbable = false; // ghost got the toy and is now escaping, no longer catchable
        if (stolenToyTransform != null)
        {
            stolenToyTransform.SetParent(transform);
            stolenToyTransform.localPosition = new Vector3(0f, -0.3f, 0.2f);
            stolenToyTransform.localRotation = Quaternion.identity;

            Rigidbody toyRb = stolenToyTransform.GetComponent<Rigidbody>();
            if (toyRb != null) toyRb.isKinematic = true;
        }

        // Direction outward from room center, horizontal only
        Vector3 outward = transform.position - roomCenter;
        Vector3 outwardXZ = new Vector3(outward.x, 0f, outward.z);

        if (outwardXZ.sqrMagnitude < 0.001f)
        {
            // Ghost is directly above center — pick a random horizontal direction
            float angle = Random.Range(0f, Mathf.PI * 2f);
            outwardXZ = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }
        outwardXZ.Normalize();

        arcP0 = transform.position;
        arcP2 = roomCenter + outwardXZ * exitRadius;
        arcP2.y = Mathf.Max(arcP2.y, transform.position.y + 0.5f); // slight upward tilt on exit

        // Midpoint, raised upward for swoop
        arcP1 = (arcP0 + arcP2) * 0.5f + Vector3.up * arcHeight;

        arcT = 0f;
        currentState = GhostState.ExitingScene;

        ghostAnimator?.PlayFlying();
        AudioManager.Instance.PlayAudioAtPosition("ghostExiting", transform.position);
    }

    void UpdateExitingScene()
    {
        // Approximate arc length for consistent speed feel
        // (chord-of-chords is a good-enough estimate for a gentle arc)
        float approxLength = Vector3.Distance(arcP0, arcP1) + Vector3.Distance(arcP1, arcP2);
        arcT += (exitSpeed / Mathf.Max(approxLength, 0.01f)) * Time.deltaTime;
        arcT = Mathf.Clamp01(arcT);

        // Quadratic Bezier position: B(t) = (1-t)^2P0 + 2(1-t)tP1 + t^2P2
        float u = 1f - arcT;
        transform.position = u * u * arcP0 + 2f * u * arcT * arcP1 + arcT * arcT * arcP2;

        // Face direction of travel (Bezier tangent: B'(t) = 2(1-t)(P1-P0) + 2t(P2-P1))
        Vector3 tangent = 2f * u * (arcP1 - arcP0) + 2f * arcT * (arcP2 - arcP1);
        if (tangent.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(tangent.normalized);

        if (arcT >= 1f)
        {
            if (stolenToyTransform != null && GazeGameManager.Instance != null)
                GazeGameManager.Instance.OnGhostExitedWithToy(this, stolenToyTransform);
            else if (GazeGameManager.Instance == null)
                Debug.LogWarning("GazeGameManager missing on ghost exit!");

            Die(false);
        }
    }

    public void ExpellFromToy()
    {
        EnterStunned(false);
        bypassInitialization = true;
        stunTimer = Mathf.Infinity;
        transform.LookAt(Camera.main.transform.position);
        ghostAnimator.PlayExpelled();
    }

    // -------------------------------------------------------------------------
    // External interface (vacuum and flashlight)
    // -------------------------------------------------------------------------

    // Vacuum
    public void OnGrabbed()
    {
        currentState = GhostState.Grabbed;
        isGrabbable = false;

        if (audioEmitter != null)
            audioEmitter.Stop();
    }

    // When let go by vacuum
    public void ReturnToStunned()
    {
        EnterStunned(false);
    }

    // Flashlight (called every frame ghost is lit)
    public void NotifyFlashlightHit(float deltaTime)
    {
        if (currentState != GhostState.Lingering) return;

        if (!litParticles.isPlaying)
            litParticles.Play();

        stunChargeTimer += deltaTime;
        if (stunChargeTimer >= stunChargeRequired)
            EnterStunned(true);
    }

    public void NotifyFlashlightLost()
    {
        if (currentState == GhostState.Lingering)
            stunChargeTimer = 0f;

        if (litParticles.isPlaying)
            litParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }


    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    void SetVisualActive(bool active)
    {
        if (ghostVisual != null)
            ghostVisual.SetActive(active);
    }

    public void Die(bool byVacuum)
    {
        GazeGameManager.Instance?.OnGhostDefeated(this, Time.time, byVacuum);
        Destroy(gameObject);
    }
}
