using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GazeGameManager : MonoBehaviour
{
    public static GazeGameManager Instance;
    
    [Header("Spawning Settings")]
    [SerializeField] GameObject ghostPrefab;
    [SerializeField] List<Transform> ghostSpawnPoints;
    [SerializeField] float defaultSpawnDelay;

    [Header("Difficulty Settings")]
    [SerializeField] int defaultAllowedGhosts;
    [SerializeField] int maxAllowedGhosts;
    [SerializeField] float difficultyIncreaseDelay;


    List<GhostBehavior> allGhosts;
    List<Transform> allToys;
    
    float timer;
    float ghostSpawnTimer;
    float difficultyIncreaseTimer;
    int allowedGhosts;
    float ghostSpawnDelay;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        allGhosts = new List<GhostBehavior>();
        allToys = new List<Transform>();

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
                // If so, spawn new ghost and register in allGhosts
                Transform spawnPoint = ghostSpawnPoints[UnityEngine.Random.Range(0, ghostSpawnPoints.Count)];

                GhostBehavior newGhost = Instantiate(ghostPrefab, spawnPoint).GetComponent<GhostBehavior>();
                allGhosts.Add(newGhost);

                ghostSpawnTimer = 0;
            }
        }

        // (to do:) If a toy has been stolen, prepare to return as a possessed toy 

        // Check if we're at max difficulty
        if (allowedGhosts < maxAllowedGhosts)
        {
            // If not, check if enough time passed to increase possible amount of ghosts
            if (difficultyIncreaseTimer >= difficultyIncreaseDelay)
            {
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
    public void OnGhostDefeated(GhostBehavior defeatedGhost, float defeatTime)
    {
        allGhosts.Remove(defeatedGhost);
    }
    public void RegisterGhost(GhostBehavior newGhost)
    {
        allGhosts.Add(newGhost);
    }
    //placeholder while finishing toy stuff
    public Transform ClaimToyForGhost(GhostBehavior claimingGhost)
    {
        return this.transform;
    }
    public void OnGhostStoleToy(GhostBehavior stealingGhost, Transform stolenToy) 
    {
        stolenToy.GetComponent<Grabbable>().enabled = false;
        allToys.Remove(stolenToy);
    }
}
