using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GazeGameManager : MonoBehaviour
{
    public static GazeGameManager Instance;

    [Header("Game Starting")]
    public InputActionReference startGameButton;
    public GameObject startGameUI;

    [Header("Spawning Settings")]
    [SerializeField] GameObject ghostPrefab;
    [SerializeField] List<Transform> ghostSpawnPoints;
    [SerializeField] float defaultSpawnDelay;

    [Header("Difficulty Settings")]
    [SerializeField] int defaultAllowedGhosts;
    [SerializeField] int maxAllowedGhosts;
    [SerializeField] float difficultyIncreaseDelay;

    [Header("References")]
    [SerializeField] GhostCapsuleManager ghostContainer;


    public List<GhostBehavior> allGhosts;
    List<Transform> allToys;
    List<Transform> unPosessedToys;
    
    float timer;
    float ghostSpawnTimer;
    float difficultyIncreaseTimer;
    int allowedGhosts;
    float ghostSpawnDelay;
    bool gameStarted;

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
        unPosessedToys = new List<Transform>();

        timer = 0;
        ghostSpawnTimer = 0;
        difficultyIncreaseTimer = 0;
        allowedGhosts = defaultAllowedGhosts;
        ghostSpawnDelay = defaultSpawnDelay;

        // get list of available toys
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Toy");
        foreach (GameObject go in obj)
        {
            allToys.Add(go.transform);
            unPosessedToys.Add(go.transform);
        }
    }
    private void Update()
    {
        float trgRightValue = startGameButton.action.ReadValue<float>();

        if (!gameStarted && trgRightValue > 0)
        {
            StartGame();
            gameStarted = true;
            startGameUI.SetActive(false);
        }

    }
    public void StartGame()
    {
        StartCoroutine(GameplayLoop());
    }
    IEnumerator GameplayLoop()
    {
        // Increment timer by the amount you wait at the end
        timer += 0.1f;
        ghostSpawnTimer += 0.1f;
        difficultyIncreaseTimer += 0.1f;

        // Check for amount of ghosts
        if (allGhosts.Count < allowedGhosts) 
        {
            // If low enough, check if enough time passed to spawn a new ghost
            if (ghostSpawnTimer >= ghostSpawnDelay)
            {
                Debug.Log("Spawned new ghost");

                // If so, spawn new ghost
                Transform spawnPoint = ghostSpawnPoints[UnityEngine.Random.Range(0, ghostSpawnPoints.Count)];

                GhostBehavior newGhost = Instantiate(ghostPrefab, spawnPoint).GetComponent<GhostBehavior>();

                ghostSpawnTimer = 0;
            }
        }

        // Check if we're at max difficulty
        if (allowedGhosts < maxAllowedGhosts)
        {
            // If not, check if enough time passed to increase possible amount of ghosts
            if (difficultyIncreaseTimer >= difficultyIncreaseDelay)
            {
                Debug.Log("Increased ghost limit");
                allowedGhosts++;
   
                difficultyIncreaseTimer = 0;
            }
        }
        
        // Wait for a while before running checks again
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(GameplayLoop());
    }


    public GhostBehavior[] GetAllGhosts()
    {
        return allGhosts.ToArray();
    }
    public Transform[] GetAllToys()
    {
        return allToys.ToArray();
    }
    public void OnGhostDefeated(GhostBehavior defeatedGhost, float defeatTime)
    {
        allGhosts.Remove(defeatedGhost);
        ghostContainer.OnGhostCaught();
    }
    public void RegisterGhost(GhostBehavior newGhost)
    {
        allGhosts.Add(newGhost);
    }
    public Transform ClaimToyForGhost(GhostBehavior claimingGhost)
    {
        if (unPosessedToys.Count == 0) 
            return null;
        
        Transform claimedToy = unPosessedToys[UnityEngine.Random.Range(0, unPosessedToys.Count)];
        return claimedToy;
    }
    public void ExpelGhostFromToy(Transform freedToy)
    {
        unPosessedToys.Add(freedToy);
    }
    public void OnGhostExitedWithToy(GhostBehavior stealingGhost, Transform stolenToy) 
    {
        stolenToy.GetComponent<Grabbable>().enabled = false;
        stolenToy.GetComponent<HauntableToy>().StealToy();
        unPosessedToys.Remove(stolenToy);
    }
}
