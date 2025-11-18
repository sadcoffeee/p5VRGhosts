using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spawning Settings")]
    public GameObject ghostPrefab;
    public bool tutorialDone = false;

    [Header("Difficulty Settings")]
    public float minBreakTime = 5f; // lower bound for difficulty
    public float maxBreakTime = 8f; // upper bound
    public float adjustStep = 0.5f; // how much to increase/decrease per good/bad catch

    [Header("Jar Ghost Settings")] //added this
    public GameObject jarGhostPrefab; //the small ghost prefab i made
    public Transform[] jarCorners;    // the 8 corner transforms defining the jar box
    public int maxJarGhosts = 10;
    public int currentJarGhosts = 0;


    [Header("References")]
    public List<Transform> ghostSpawnPoints;
    public List<PossessedObject> allObjects = new List<PossessedObject>();
    public List<FlyTowardsGhost> activeGhosts = new List<FlyTowardsGhost>();

    [Header("Turtorial")]
    [SerializeField] bool endTurtorial = true;

    private int ghostsDefeated = 0;
    [HideInInspector] public float objectBreakTime = 7f;

    public int GhostsDefeated => ghostsDefeated; //added


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
        }
        
        //Temp line to skip tutorial
        if (endTurtorial) EndTutorial();
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
    public void UnregisterFurniture(PossessedObject furniture)
    {
        if (allObjects.Contains(furniture))
            allObjects.Remove(furniture);
    }

    public PossessedObject GetRandomFreeObject(PossessedObject exclude = null)
    {
        List<PossessedObject> freeObjects = allObjects.FindAll(o => !o.isPossessed && o != exclude);
        if (freeObjects.Count == 0) return null;
        return freeObjects[Random.Range(0, freeObjects.Count)];
    }

    public void OnGhostDefeated(FlyTowardsGhost ghost, float catchDuration)
    {
        ghostsDefeated++;
        UnregisterGhost(ghost);
        SpawnJarGhost();

        AdjustDifficulty(catchDuration);

        StartCoroutine(SpawnNextGhost());
    }

    #endregion

    #region spawning logic
    public void EndTutorial()
    {
        tutorialDone = true;

    }

    private IEnumerator SpawnNextGhost()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

        if (ghostPrefab == null || ghostSpawnPoints.Count == 0)
            yield break;

        // Only one ghost at a time
        if (activeGhosts.Count > 0)
            yield break;

        Transform spawnPoint = ghostSpawnPoints[Random.Range(0, ghostSpawnPoints.Count)];
        GameObject newGhost = Instantiate(ghostPrefab, spawnPoint.position, spawnPoint.rotation);

        FlyTowardsGhost ghostComponent = newGhost.GetComponent<FlyTowardsGhost>();
        if (ghostComponent != null)
        {
            RegisterGhost(ghostComponent);
            // Let the ghost know when it spawned so it can report catch duration
            ghostComponent.spawnTime = Time.time;
        }
    }
    private void AdjustDifficulty(float catchDuration)
    {
        float lowerThreshold = 1.2f * objectBreakTime;
        float upperThreshold = 2.5f * objectBreakTime;

        if (catchDuration < lowerThreshold)
        {
            // Player caught quickly = make it harder
            objectBreakTime = Mathf.Max(minBreakTime, objectBreakTime - adjustStep);
            Debug.Log($"Ghost caught FAST ({catchDuration:F1}s). Increasing difficulty {objectBreakTime:F1}s");
        }
        else if (catchDuration > upperThreshold)
        {
            // Player was slow = make it easier
            objectBreakTime = Mathf.Min(maxBreakTime, objectBreakTime + adjustStep);
            Debug.Log($"Ghost caught SLOW ({catchDuration:F1}s). Decreasing difficulty {objectBreakTime:F1}s");
        }
        else
        {
            Debug.Log($"Ghost caught average ({catchDuration:F1}s). No change to difficulty ({objectBreakTime:F1}s)");
        }

        // Apply new break time to all furniture
        foreach (var obj in allObjects)
        {
            obj.UpdateBreakTime(objectBreakTime);
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
