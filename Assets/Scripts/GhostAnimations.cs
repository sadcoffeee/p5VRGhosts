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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hoverGhostScript = GetComponent<HoverGhost>();
        Stars = transform.Find("Ghostbody/Stars")?.gameObject;
        ExclamationMarks = transform.Find("Ghostbody/ExclamationMarks")?.gameObject;
        Stars.SetActive(false);
        ExclamationMarks.SetActive(false);
        anim = GetComponent<Animator>();
        ghostFaceRenderer = GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name == "ghostface");
    }

    public void PlayIdle()
    {
        anim.Play("Idle"); 
        Stars.SetActive(false); 
        ExclamationMarks.SetActive(false);
    }

    public void PlayShocked()
    {
        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[1]; 
        ExclamationMarks.SetActive(true); 
        Stars.SetActive(false);
        anim.Play("Shock");
    }

    public void PlayDizzy()
    {
        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[2]; 
        Stars.SetActive(true); 
        ExclamationMarks.SetActive(false);
        anim.Play("Dizzy");
        hoverGhostScript.enabled = false;
    }

    public void PlayFlying()
    {
        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[3]; 
        Stars.SetActive(false); 
        ExclamationMarks.SetActive(false);
        anim.Play("Flying");
    }
    public void PlayHappyFlying()
    {
        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[0];
        anim.Play("Flying");
        ExclamationMarks.SetActive(false);
        Stars.SetActive(false);
    }
    public void PlayExcited()
    {
        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[4];
        anim.Play("Shock");
        ExclamationMarks.SetActive(false);
        Stars.SetActive(false);
    }
    public void Caught()
    {
        ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[1];
        ExclamationMarks.SetActive(true);
        Stars.SetActive(false);
    }
}
