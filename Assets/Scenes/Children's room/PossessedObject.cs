using UnityEngine;

public class PossessedObject : MonoBehaviour
{
    private Renderer currentObject;
    public Material endMaterial;
    private bool isPossessed = false;
    private GameObject ghostFace;

    private void Start()
    {
        currentObject = GetComponent<Renderer>();
        ghostFace = transform.GetChild(0).gameObject;
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
