using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    public int maxGhosts = 6;

    public List<PossessedObject> allObjects = new List<PossessedObject>();
    public List<FlyTowardsGhost> activeGhosts = new List<FlyTowardsGhost>();


    private int ghostsDefeated = 0;


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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
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
    }

}
