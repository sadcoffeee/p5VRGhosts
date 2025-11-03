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
        anim.Play("Idle");
    }
}
