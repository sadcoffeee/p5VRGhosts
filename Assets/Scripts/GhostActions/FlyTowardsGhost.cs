using UnityEngine;

public class FlyTowardsGhost : MonoBehaviour
{
    public string targetTag = "Couch";
    public float speed = 5f;
    public float arriveDistance = 0.1f; //rammer couch
    public float delay = 3f; //test timer før vi flyver mod couch
    public GameObject Ghost;

    private Transform target;
    private HoverGhost hoverScript; //så vi kan finde hoverghost scriptet de kan snakke sammen

    private bool isFlying = false;
    private float timer = 0f;

    void Start()
    {
        GameObject couch = GameObject.FindGameObjectWithTag(targetTag);
        if (couch != null)
        {
            target = couch.transform;
        }

        //vi henter hoverscriptet nu
        hoverScript = GetComponent<HoverGhost>();
    }
    void Update()

    {
        if (!isFlying)
        {
            timer += Time.deltaTime;
            if (timer >= delay)
            {
                StartFlying();
            }
            return;
        }

        if (target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            Time.deltaTime * speed
            );

        //laver inactive når arrived så vi kan active igen bagefter
        if (Vector3.Distance( transform.position, target.position ) <= arriveDistance)
        {
            Ghost.SetActive(false); 
            //hoverScript.enabled = true; //genaktiver hover
            isFlying = false;
        }
        
    }

    
    public void StartFlying()
    {
        isFlying = true;
        if (hoverScript != null)
        {
            hoverScript.enabled = false; //pause hover mens flyver
        }
    }
}
