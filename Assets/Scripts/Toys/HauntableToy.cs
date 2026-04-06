using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

public class HauntableToy : MonoBehaviour
{
    //Settings
    [SerializeField] bool isHaunted = false;
    [SerializeField] GameObject spawnEffect;
    [SerializeField] GameObject freedEffect;
    [SerializeField] Material hauntedMaterial;
    [SerializeField] GameObject ExpelledGhost;

    [Header("Movement")]
    [SerializeField] float waypointIdleTime = 5f;
    [SerializeField] float waypointStoppingDistance = 0.2f;

    //References
    [Header("Refrences")]
    [SerializeField] MeshRenderer meshRenderer;

    //Variables
    Material defaultMaterial;
    HauntedToyWaypoint waypoint;
    float idleTimer = 0f;

    //Delegates
    public delegate void WaypointSet(HauntedToyWaypoint waypoint);
    public WaypointSet onWaypointSet;

    public delegate void Haunted(bool state);
    public Haunted onHaunted;

    //Logic
    private void Start()
    {
        ResetIdleTimer();

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (!isHaunted)
        {
            defaultMaterial = meshRenderer.material;
        }
    }

    private void Update()
    {
        if (waypoint == null)
            return;

        if (DistanceToWaypoint() <= waypointStoppingDistance)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer < 0f)
            {
                FindNewWaypoint();
                ResetIdleTimer();
            }
        }
    }

    //Methods
    void UpdateMaterial()
    {
        if (isHaunted)
        {
            meshRenderer.material = hauntedMaterial;
        }
        else
        {
            meshRenderer.material = defaultMaterial;
        }
    }

    void ResetIdleTimer()
    {
        idleTimer = waypointIdleTime;
    }

    //Public methods
    public void StealToy()
    {
        HauntedToyManager.instance.AddToy(gameObject);
    }

    public void SetHaunted(bool state)
    {
        isHaunted = state;

        UpdateMaterial();

        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, transform.position, Quaternion.identity);
        }

        onHaunted?.Invoke(state);
    }

    public void ReleaseGhost()
    {
        SetHaunted(false);

        //Spawn ghost
        GameObject ghost = Instantiate(ExpelledGhost, transform.transform.position, Quaternion.identity);
        GhostBehavior ghostBehavior = ghost.GetComponent<GhostBehavior>();
        ghostBehavior.ExpellFromToy();

        //Spawn effect
        if (freedEffect != null)
        {
            Instantiate(freedEffect, transform.position, Quaternion.identity);
        }

        if (waypoint != null)
            waypoint.Occupy(false);
    }

    public void SetWaypoint(HauntedToyWaypoint waypoint)
    {
        //If it already has a waypoint, release it
        if (this.waypoint != null)
        {
            this.waypoint.Occupy(false);
        }

        //Update waypoint
        this.waypoint = waypoint;
        this.waypoint.Occupy(true);

        //Call delegate
        onWaypointSet?.Invoke(this.waypoint);
    }

    public void FindNewWaypoint()
    {
        if (waypoint == null)
        {
            Debug.Log("No old waypoint set");
            return;
        }

        HauntedToyWaypoint newWaypoint =  waypoint.GetWaypoint();
        if (newWaypoint != null)
        {
            SetWaypoint(newWaypoint);
        }
    }

    // Returns the distance to the current waypoint projected to the horizontal plane. Effectivly negates distance in height
    public float DistanceToWaypoint()
    {
        if (waypoint == null)
        {
            Debug.Log("No waypoint!");
            return 0;
        }

        return Vector3.Distance(transform.position, GetProjectedWaypoint(waypoint));
    }

    public bool IsHaunted()
    {
        return isHaunted;
    }

    public Vector3 GetProjectedWaypoint(HauntedToyWaypoint waypoint)
    {
        Vector3 projectedWaypointPosition = waypoint.transform.position;
        projectedWaypointPosition.y = transform.position.y;
        return projectedWaypointPosition;
    }
}


//Custom Editor
[CustomEditor(typeof(HauntableToy))]
[CanEditMultipleObjects]
public class HauntableToyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("Debug tools");

        HauntableToy hauntableToy = (HauntableToy)target;
        if (GUILayout.Button("Steal Toy"))
        {
            hauntableToy.StealToy();
        }

        if (GUILayout.Button("Release Ghost"))
        {
            hauntableToy.ReleaseGhost();
        }

        if (GUILayout.Button("Haunt"))
        {
            hauntableToy.SetHaunted(true);
        }
    }
}