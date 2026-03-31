using UnityEngine;
using System.Collections.Generic;

public class HauntedToyWaypointManager : MonoBehaviour
{
    //Settings

    //Logic

    //Singleton
    public static HauntedToyWaypointManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Two or more HauntedWaypointManagers present in scene!");
            enabled = false;
        }
    }

    //Public methods
    
}
