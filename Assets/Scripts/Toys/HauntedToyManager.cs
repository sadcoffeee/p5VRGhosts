using UnityEngine;
using System.Collections.Generic;


public class HauntedToyManager : MonoBehaviour
{
    //Settings
    [SerializeField] float toySpawnDelay = 10f;
    //[SerializeField] Transform[] spawnPoints;
    [SerializeField] HauntedToyWaypoint[] waypoints;

    //Variables
    float toySpawnTimer = 0;
    List<GameObject> stolenToys = new List<GameObject>();

    //Logic
    private void Update()
    {
        if (toySpawnTimer <= 0 && stolenToys.Count > 0)
        {
            SpawnToy();
            StartToySpawnTimer();
        }
        else if (toySpawnTimer > 0)
        {
            toySpawnTimer -= Time.deltaTime;
        }
    }

    //Singleton
    public static HauntedToyManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple HauntedToyManagers present in scene!");
            enabled = false;
        }
    }

    //Methods
    void SpawnToy()
    {
        if (stolenToys.Count > 0)
        {
            //Select toy
            int randomToy = GetRandomToyIndex();


            //Find Waypoint
            HauntedToyWaypoint waypoint = GetAvailableWaypoint();

            if (waypoint != null)
            {
                //Activate and move to waypoint
                stolenToys[randomToy].transform.position = waypoint.transform.position;
                stolenToys[randomToy].transform.rotation = Quaternion.identity;
                stolenToys[randomToy].SetActive(true);
                stolenToys[randomToy].GetComponent<HauntableToy>().SetHaunted(true);

                //Remove from stole toys
                stolenToys.RemoveAt(randomToy);
            }
        }
    }

    void StartToySpawnTimer()
    {
        toySpawnTimer = toySpawnDelay;
    }

    int GetRandomToyIndex()
    {
        return Random.Range(0, stolenToys.Count);
    }


    //Public methods
    public void AddToy(GameObject toy)
    {
        //Set location and hide
        toy.transform.position = transform.position;
        toy.SetActive(false);

        //Add to stolen list
        stolenToys.Add(toy);

        //Start timer if not already started
        if (toySpawnTimer <= 0)
        {
            StartToySpawnTimer();
        }
    }

    public HauntedToyWaypoint GetAvailableWaypoint()
    {
        List<HauntedToyWaypoint> availableWaypoints = new List<HauntedToyWaypoint>();
        foreach (HauntedToyWaypoint waypoint in waypoints)
        {
            if (waypoint.IsAvailable())
            {
                availableWaypoints.Add(waypoint);
            }
        }

        if (availableWaypoints.Count > 0)
        {
            return availableWaypoints[Random.Range(0, availableWaypoints.Count)];
        }
        else
        {
            Debug.LogWarning("No toy waypoints available!");
            return null;
        }
    }
}
