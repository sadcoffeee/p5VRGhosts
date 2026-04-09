using System.Linq;
using UnityEngine;


public class GhostAnimations : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private float timer;
    private Renderer ghostFaceRenderer;
    public Texture2D[] GhostFaceMaterials;
    private GameObject Stars;
    private GameObject ExclamationMarks;
    private HoverGhost hoverGhostScript;
    private bool isInitialized = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }

    public void PlayIdle()
    {
        Initialize();

        anim.Play("Idle"); 
        Stars.SetActive(false); 
        ExclamationMarks.SetActive(false);
    }

    public void PlayShocked()
    {
        Initialize();

        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[1]; 
        ExclamationMarks.SetActive(true); 
        Stars.SetActive(false);
        anim.Play("Shock");
    }

    public void PlayDizzy()
    {
        Initialize();

        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[2]; 
        Stars.SetActive(true); 
        ExclamationMarks.SetActive(false);
        anim.Play("Dizzy");
        if (hoverGhostScript != null)
            hoverGhostScript.enabled = false;
    }

    public void PlayExpelled()
    {
        Initialize();

        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[2];
        Stars.SetActive(true);
        ExclamationMarks.SetActive(false);
        anim.Play("Ghost_expelled");
        if (hoverGhostScript != null)
            hoverGhostScript.enabled = false;
    }
    public void PlayFlying()
    {
        Initialize();

        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[3]; 
        Stars.SetActive(false); 
        ExclamationMarks.SetActive(false);
        anim.Play("Flying");
    }
    public void PlayHappyFlying()
    {
        Initialize();

        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[0];
        anim.Play("Flying");
        ExclamationMarks.SetActive(false);
        Stars.SetActive(false);
    }
    public void PlayExcited()
    {
        Initialize();

        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[4];
        anim.Play("Shock");
        ExclamationMarks.SetActive(false);
        Stars.SetActive(false);
    }
    public void Caught()
    {
        Initialize();

        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[1];
        ExclamationMarks.SetActive(true);
        Stars.SetActive(false);
    }

    void Initialize()
    {
        if (!isInitialized)
        {
            hoverGhostScript = GetComponent<HoverGhost>();
            Stars = transform.Find("Ghostbody/Stars")?.gameObject;
            ExclamationMarks = transform.Find("Ghostbody/ExclamationMarks")?.gameObject;
            Stars.SetActive(false);
            ExclamationMarks.SetActive(false);
            anim = GetComponent<Animator>();
            ghostFaceRenderer = GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name == "ghostface");

            isInitialized = true;
        }
    }
}
