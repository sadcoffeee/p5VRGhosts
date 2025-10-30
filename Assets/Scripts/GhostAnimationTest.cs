using System.Linq;
using UnityEngine;


public class GhostAnimationTest : MonoBehaviour
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

    // Update is called once per frame
    void Update()
    {
        timer = Time.time;
        if (timer > 3 && timer < 6)
        {
            anim.Play("Shock");
            ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[1];
            ExclamationMarks.SetActive(true);
            Stars.SetActive(false);

        }
        else if (timer > 6 && timer < 9) 
        {
            {
                anim.Play("Dizzy");
                ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[2];
                Stars.SetActive(true);
                ExclamationMarks.SetActive(false);
                hoverGhostScript.enabled = false;
            }
        }
        else if (timer > 9 && timer < 12)
        {
            anim.Play("Flying");
            ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[3];
            Stars.SetActive(false);
            ExclamationMarks.SetActive(false);
            hoverGhostScript.enabled = true;
        }
        else
        {
            anim.Play("Idle");
            ghostFaceRenderer.material.mainTexture = GhostFaceMaterials[0];
            Stars.SetActive(false);
            ExclamationMarks.SetActive(false);

        }

    }
}
