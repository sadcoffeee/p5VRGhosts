using UnityEngine;
using System.Collections.Generic;

public class HauntedToyWaypoint : MonoBehaviour
{
    //Settings
    [SerializeField] HauntedToyWaypoint[] connectedWaypoints;

    //Variables
    bool isOccupied = false;

    //Public methods
    public HauntedToyWaypoint GetWaypoint()
    {
        List<HauntedToyWaypoint> availableWaypoints = new List<HauntedToyWaypoint>();
        foreach(HauntedToyWaypoint waypoint in connectedWaypoints)
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
            return null;
        }
    }

    public bool IsAvailable()
    {
        return !isOccupied;
    }

    public bool Occupy(bool state)
    {
        if (isOccupied && state == true)
        {
            return false;
        }

        isOccupied = state;
        return true;
    }
}
