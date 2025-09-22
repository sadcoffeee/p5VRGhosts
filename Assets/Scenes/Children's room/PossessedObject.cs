using UnityEngine;

public class PossessedObject : MonoBehaviour
{
    private Renderer currentObject;
    public Material endMaterial;
    public bool isPossessed = false;
    private GameObject ghostFace;
    private Material startMaterial;
    public GameObject ghostOccupying;


    private void Start()
    {
        currentObject = GetComponent<Renderer>();
        ghostFace = transform.GetChild(0).gameObject;
        ghostFace.SetActive(false);
        startMaterial = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (isPossessed)
        {
            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
        }
    }
    public void SetPossessed(bool value, GameObject ghost = null)
    {
        isPossessed = value;

        if (isPossessed)
        {
            ghostOccupying = ghost;
            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
        }
        else
        {
            ghostFace.SetActive(false);
            currentObject.material = startMaterial;
            //Reactivate ghost and release it from furniture
            ghostOccupying.transform.position = transform.position + transform.forward * 2f;
            ghostOccupying.SetActive(true);
            FlyTowardsGhost ghostScript = ghostOccupying.GetComponent<FlyTowardsGhost>();
            ghostScript.currentState = FlyTowardsGhost.GhostState.Hovering;
            ghostOccupying = null;

        }
    }
}

