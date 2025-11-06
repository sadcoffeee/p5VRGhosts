using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spawning Settings")]
    public GameObject ghostPrefab;
    public int maxGhosts = 6;
    private int currentMaxGhosts = 1;
    public float spawnInterval = 5f;
    public float diffCheckInterval = 10f;
    public int killsToIncrease = 3;
    public bool tutorialDone = false;

    [Header("Jar Ghost Settings")] //added this
    public GameObject jarGhostPrefab; //the small ghost prefab i made
    public Transform[] jarCorners;    // the 8 corner transforms defining the jar box
    public int maxJarGhosts = 10;
    public int currentJarGhosts = 0;


    [Header("References")]
    public List<Transform> ghostSpawnPoints;
    public List<PossessedObject> allObjects = new List<PossessedObject>();
    public List<FlyTowardsGhost> activeGhosts = new List<FlyTowardsGhost>();

    private int ghostsDefeated = 0;
    private int ghostsDefeatedLastCheck = 0;

    //public int GhostsDefeated => ghostsDefeated; //added


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Furniture");
        foreach (GameObject go in obj)
        {
            PossessedObject theObjectInQuestion = go.GetComponent<PossessedObject>();
            allObjects.Add(theObjectInQuestion);
            EndTutorial();
        }


    }

    #region ghost management
    public void RegisterGhost(FlyTowardsGhost ghost)
    {
        if (!activeGhosts.Contains(ghost))
            activeGhosts.Add(ghost);
    }

    public void UnregisterGhost(FlyTowardsGhost ghost)
    {
        if (activeGhosts.Contains(ghost))
            activeGhosts.Remove(ghost);
    }

    public PossessedObject GetRandomFreeObject()
    {
        List<PossessedObject> freeObjects = allObjects.FindAll(o => !o.isPossessed);
        if (freeObjects.Count == 0) return null;
        return freeObjects[Random.Range(0, freeObjects.Count)];
    }

    public void OnGhostDefeated(FlyTowardsGhost ghost)
    {
        ghostsDefeated++;
        UnregisterGhost(ghost);
        Debug.Log("defeated ghost");

        SpawnJarGhost(); //spawn jar ghost everytime ghost defeated
    }

    public bool CanSpawnMoreGhosts()
    {
        return activeGhosts.Count < currentMaxGhosts;
    }
    #endregion

    #region spawning logic
    private IEnumerator AdjustDifficultyLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(diffCheckInterval);

            int killsThisInterval = ghostsDefeated - ghostsDefeatedLastCheck;
            ghostsDefeatedLastCheck = ghostsDefeated;

            if (killsThisInterval >= killsToIncrease && currentMaxGhosts < maxGhosts)
            {
                currentMaxGhosts++;
                Debug.Log($"now allowing {currentMaxGhosts} ghosts at once");
            }
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (CanSpawnMoreGhosts())
            {
                SpawnGhost();
            }
        }
    }

    private void EndTutorial()
    {
        tutorialDone = true;
        StartCoroutine(AdjustDifficultyLoop());
        StartCoroutine(SpawnLoop());
    }

    private void SpawnGhost()
    {
        if (ghostPrefab == null || ghostSpawnPoints.Count == 0) return;

        Transform spawnPoint = ghostSpawnPoints[Random.Range(0, ghostSpawnPoints.Count)];

        GameObject newGhost = Instantiate(ghostPrefab, spawnPoint.position, spawnPoint.rotation);

        FlyTowardsGhost ghostComponent = newGhost.GetComponent<FlyTowardsGhost>();
        if (ghostComponent != null)
        {
            RegisterGhost(ghostComponent);
        }
    }
    #endregion

    //added this
    #region Jar Ghost Logic 
    private Vector3 GetRandomJarPosition()
    {
        if (jarCorners == null || jarCorners.Length < 8)
        {
            Debug.Log("Jar corner fail");
            return Vector3.zero;
        }

        Vector3 min = jarCorners[0].position;
        Vector3 max = jarCorners[0].position;

        foreach (Transform corner in jarCorners)
        {
            Vector3 pos = corner.position;
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }

        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        float z = Random.Range(min.z, max.z);

        return new Vector3(x, y, z);

    }

    public void SpawnJarGhost()
    {

        //LIMIT
        if (currentJarGhosts >= maxJarGhosts) { return; }

        if (jarGhostPrefab == null) return;

        Vector3 spawnPos = GetRandomJarPosition();
        GameObject newJarGhost = Instantiate(jarGhostPrefab, spawnPos, Quaternion.identity);

        currentJarGhosts++; // count visually spawned

        FlyTowardsGhost ghostComponent = newJarGhost.GetComponent<FlyTowardsGhost>();
        if (ghostComponent != null)
        {
            RegisterGhost(ghostComponent);
        }
    }
    #endregion

}
