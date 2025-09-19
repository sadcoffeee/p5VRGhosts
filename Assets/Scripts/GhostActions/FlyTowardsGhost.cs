using UnityEngine;

public class FlyTowardsGhost : MonoBehaviour
{
    public float speed = 5f;
    public float arriveDistance = 0.1f; //rammer couch
    public float delay = 3f; //test timer før vi flyver mod couch
    public GameObject Ghost;
    int targetChosen;
    private PossessedObject possessed;

    private Transform target;
    private HoverGhost hoverScript; //så vi kan finde hoverghost scriptet de kan snakke sammen

    private bool isFlying = false;
    private float timer = 0f;

    void Start()
    {
        GameObject[] furniture = GameObject.FindGameObjectsWithTag("Furniture");
        targetChosen = Random.Range(0, furniture.Length);
        target = furniture[targetChosen].transform;

        //vi henter hoverscriptet nu
        hoverScript = GetComponent<HoverGhost>();
        possessed = target.GetComponent<PossessedObject>();

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
            possessed.isPossessed = true;
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
