using UnityEngine;

public class PossessedObject : MonoBehaviour
{
    private Renderer currentObject;
    public Material endMaterial;
    public bool isPossessed = false;
    private GameObject ghostFace;


    private void Start()
    {
        currentObject = GetComponent<Renderer>();
        ghostFace = transform.GetChild(0).gameObject;
        ghostFace.SetActive(false);
    }

    private void Update()
    {
        if (isPossessed)
        {
            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
        }
    }
}
