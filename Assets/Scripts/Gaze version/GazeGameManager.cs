using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Conditions
{
    NoVibration,
    HandVibration,
    ArmVibration,
    AllVibration
}

public class GazeGameManager : MonoBehaviour
{
    public static GazeGameManager Instance;

    [Header("Session settings")]
    public int participantNumber;
    public Conditions condition;

    [Header("Game Starting")]
    public InputActionReference startGameButton;
    public GameObject startGameUI;

    [Header("Spawning Settings")]
    [SerializeField] GameObject ghostPrefab;
    [SerializeField] List<Transform> ghostSpawnPoints;

    [Header("Difficulty: Ghost Limits")]
    [SerializeField] int maxHiddenGhosts = 2;

    [Header("Difficulty: Spawn Delay")]
    [SerializeField] float spawnDelayMax = 20f;
    [SerializeField] float spawnDelayDefault = 10f;
    [SerializeField] float spawnDelayMin = 2f;
    [SerializeField] float spawnDelayStep = 2f;

    [Header("Difficulty: Pressure Timers")]
    [SerializeField] float maxSecondsWithNoGhost = 10f;
    [SerializeField] float secondsAtMaxToSlowDown = 15f;
    [SerializeField] float secondsAtNoneToSpeedUp = 8f;

    [Header("Difficulty: Haunted Toy Pressure")]
    [SerializeField] float forceHauntDelay = 30f;
    [SerializeField] int hauntedToyUnhauntThreshold = 6;

    [Header("References")]
    [SerializeField] GhostCapsuleManager ghostContainer;


    // -------------------------------------------------------------------------
    // Internal state
    // -------------------------------------------------------------------------

    public List<GhostBehavior> allGhosts;
    List<Transform> allToys;
    List<Transform> unPossessedToys;
    List<Transform> hauntedToys;

    float ghostSpawnTimer;
    float ghostSpawnDelay;

    // Pressure accumulators for DDA
    float timeAtMaxHiddenGhosts;    // player is struggling, too many ghosts building up
    float timeAtNoHiddenGhosts;     // player is excelling, clears ghosts fast
    float noHauntTimer;             // time since last toy was haunted

    Transform lastUsedSpawnPoint;

    bool gameStarted;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        gameStarted = false;
        allGhosts = new List<GhostBehavior>();
        allToys = new List<Transform>();
        unPossessedToys = new List<Transform>();
        hauntedToys = new List<Transform>();

        ghostSpawnTimer = 0f;
        ghostSpawnDelay = spawnDelayDefault;

        timeAtMaxHiddenGhosts = 0f;
        timeAtNoHiddenGhosts = 0f;
        noHauntTimer = 0f;

        lastUsedSpawnPoint = null;

        // Collect all toys in the scene
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Toy"))
        {
            allToys.Add(go.transform);
            unPossessedToys.Add(go.transform);
        }
    }

    void Update()
    {
        float trgRightValue = startGameButton.action.ReadValue<float>();
        if (!gameStarted && trgRightValue > 0)
        {
            StartGame();
            gameStarted = true;
            startGameUI.SetActive(false);
        }
    }

    // -------------------------------------------------------------------------
    // Game loop
    // -------------------------------------------------------------------------

    public void StartGame()
    {
        SessionLogger.Instance.SetParticipantID(participantNumber);
        SessionLogger.Instance.SetCondition(condition.ToString());
        SessionLogger.Instance.Log("Started Game");

        StartCoroutine(GameplayLoop());
    }

    IEnumerator GameplayLoop()
    {
        const float tick = 0.1f;

        TrySpawnGhost();

        while (true)
        {
            ghostSpawnTimer += tick;

            int hiddenCount = CountHiddenGhosts();

            // ------------------------------------------------------------------
            // 1. DDA: pressure accumulators
            // ------------------------------------------------------------------

            if (hiddenCount >= maxHiddenGhosts)
            {
                // Player is not keeping up: ghosts are piling up
                timeAtMaxHiddenGhosts += tick;
                timeAtNoHiddenGhosts = 0f;

                if (timeAtMaxHiddenGhosts >= secondsAtMaxToSlowDown)
                {
                    ghostSpawnDelay = Mathf.Min(ghostSpawnDelay + spawnDelayStep, spawnDelayMax);
                    timeAtMaxHiddenGhosts = 0f;
                    Debug.Log($"[DDA] Player struggling´, spawn delay increased to {ghostSpawnDelay:F1}s");
                }
            }
            else if (hiddenCount == 0)
            {
                // Player is excelling: no hidden ghosts at all
                timeAtNoHiddenGhosts += tick;
                timeAtMaxHiddenGhosts = 0f;

                if (timeAtNoHiddenGhosts >= secondsAtNoneToSpeedUp)
                {
                    ghostSpawnDelay = Mathf.Max(ghostSpawnDelay - spawnDelayStep, spawnDelayMin);
                    timeAtNoHiddenGhosts = 0f;
                    Debug.Log($"[DDA] Player excelling, spawn delay decreased to {ghostSpawnDelay:F1}s");
                }
            }
            else
            {
                // Somewhere in between: will start by not resetting the accumulators to see how that feels

                //timeAtMaxHiddenGhosts = 0f;
                //timeAtNoHiddenGhosts = 0f;
            }

            // ------------------------------------------------------------------
            // 2. Force-haunt check (eager player with no haunted toys)
            // ------------------------------------------------------------------

            noHauntTimer += tick;
            if (noHauntTimer >= forceHauntDelay && hauntedToys.Count == 0)
            {
                ForceHauntRandomToy();
                noHauntTimer = 0f;
            }

            // ------------------------------------------------------------------
            // 3. Regular ghost spawn
            // ------------------------------------------------------------------

            if (hiddenCount < maxHiddenGhosts && ghostSpawnTimer >= ghostSpawnDelay)
            {
                TrySpawnGhost();
                ghostSpawnTimer = 0f;
            }

            // ------------------------------------------------------------------
            // 4. Emergency spawn, never go longer than maxSecondsWithNoGhost without at least one hidden ghost
            // ------------------------------------------------------------------

            if (hiddenCount == 0 && timeAtNoHiddenGhosts >= maxSecondsWithNoGhost)
            {
                TrySpawnGhost();
                ghostSpawnTimer = 0f;
                timeAtNoHiddenGhosts = 0f;
                Debug.Log("[DDA] Emergency ghost spawn - scene was ghost-free too long");
            }

            yield return new WaitForSeconds(tick);
        }
    }

    // -------------------------------------------------------------------------
    // Spawning helpers
    // -------------------------------------------------------------------------

    void TrySpawnGhost()
    {
        // If too many haunted toys, silently un-haunt one before spawning
        if (hauntedToys.Count >= hauntedToyUnhauntThreshold)
            SilentUnhauntRandomToy();

        Transform spawnPoint = PickSpawnPoint();
        if (spawnPoint == null) return;

        Instantiate(ghostPrefab, spawnPoint.position, spawnPoint.rotation);
        lastUsedSpawnPoint = spawnPoint;
        Debug.Log($"[Spawn] Ghost spawned at {spawnPoint.name}");
    }

    Transform PickSpawnPoint()
    {
        if (ghostSpawnPoints.Count == 0) return null;

        // Build candidate list, excluding the last used point
        List<Transform> candidates = new List<Transform>(ghostSpawnPoints.Count);
        foreach (Transform sp in ghostSpawnPoints)
        {
            if (sp != lastUsedSpawnPoint)
                candidates.Add(sp);
        }

        // Fall back to full list if all points were excluded (only one point exists, this should never be the case)
        if (candidates.Count == 0)
            candidates = ghostSpawnPoints;

        return candidates[Random.Range(0, candidates.Count)];
    }

    void ForceHauntRandomToy()
    {
        if (unPossessedToys.Count == 0) return;

        Transform toy = unPossessedToys[Random.Range(0, unPossessedToys.Count)];
        HauntableToy hauntable = toy.GetComponent<HauntableToy>();
        if (hauntable == null) return;

        hauntable.SetHaunted(true);
        hauntable.SetWaypoint(HauntedToyManager.instance.GetAvailableWaypoint());
        unPossessedToys.Remove(toy);
        hauntedToys.Add(toy);
        noHauntTimer = 0f;
        Debug.Log($"[DDA] Force-haunted toy: {toy.name}");
    }

    void SilentUnhauntRandomToy()
    {
        if (hauntedToys.Count == 0) return;

        Transform toy = hauntedToys[Random.Range(0, hauntedToys.Count)];
        HauntableToy hauntable = toy.GetComponent<HauntableToy>();
        if (hauntable == null) return;

        hauntable.SetHaunted(false, silent: true);
        hauntedToys.Remove(toy);
        unPossessedToys.Add(toy);
        Debug.Log($"[DDA] Silently un-haunted toy: {toy.name}");
    }

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    int CountHiddenGhosts()
    {
        int count = 0;
        foreach (GhostBehavior g in allGhosts)
        {
            if (g != null)
                count++;
        }
        return count;
    }

    public GhostBehavior[] GetAllGhosts() => allGhosts.ToArray();
    public Transform[] GetAllToys() => allToys.ToArray();

    // -------------------------------------------------------------------------
    // Public interface, called by ghost / toy scripts
    // -------------------------------------------------------------------------

    public void RegisterGhost(GhostBehavior newGhost)
    {
        allGhosts.Add(newGhost);
    }

    public void OnGhostDefeated(GhostBehavior defeatedGhost, float defeatTime, bool shouldCount)
    {
        allGhosts.Remove(defeatedGhost);

        if (shouldCount)
            ghostContainer.OnGhostCaught();
    }

    public Transform ClaimToyForGhost(GhostBehavior claimingGhost)
    {
        if (unPossessedToys.Count == 0) return null;

        Transform claimed = unPossessedToys[Random.Range(0, unPossessedToys.Count)];
        return claimed;
    }

    public void OnGhostExitedWithToy(GhostBehavior stealingGhost, Transform stolenToy)
    {
        if (stolenToy == null) return;

        if (unPossessedToys.Contains(stolenToy))
            unPossessedToys.Remove(stolenToy);

        Grabbable grabbable = stolenToy.GetComponent<Grabbable>();
        if (grabbable != null) grabbable.enabled = false;

        stolenToy.GetComponent<HauntableToy>()?.StealToy();

        // Track as haunted so DDA can account for it
        if (!hauntedToys.Contains(stolenToy))
            hauntedToys.Add(stolenToy);

        // Reset the no-haunt timer since something was just haunted
        noHauntTimer = 0f;
    }


    public void ExpelGhostFromToy(Transform freedToy)
    {
        unPossessedToys.Add(freedToy);
        hauntedToys.Remove(freedToy);

        Grabbable grabbable = freedToy.GetComponent<Grabbable>();
        if (grabbable != null) grabbable.enabled = true;
    }
}
