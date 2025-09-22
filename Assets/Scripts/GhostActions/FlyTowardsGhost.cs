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
    public enum GhostState
    {
        Hovering,
        FlyingTowardsObject,
        Possessing

    };

    private float timer = 0f;
    public GhostState currentState = GhostState.Hovering;

    void Start()
    {
        //vi henter hoverscriptet nu
        hoverScript = GetComponent<HoverGhost>();
    }

    void Update()
    {
        switch (currentState)
        {
            case GhostState.Hovering:
                hoverScript.enabled = true; // keep hover active
                Ghost.SetActive(true);
                Hovering();
                break;

            case GhostState.FlyingTowardsObject:
                hoverScript.enabled = false; // pause hover while flying
                if (target == null)
                {
                    GameObject[] furniture = GameObject.FindGameObjectsWithTag("Furniture");
                    targetChosen = Random.Range(0, furniture.Length);
                    target = furniture[targetChosen].transform;
                    possessed = target.GetComponent<PossessedObject>();
                }
                StartFlying();
                break;

            case GhostState.Possessing:
                possessed.SetPossessed(true, this.gameObject);
                target = null;
                Ghost.SetActive(false);
                break;
        }
        Debug.Log(currentState);
    }

    private void Hovering()
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            currentState = GhostState.FlyingTowardsObject;
            timer = 0;
        }
    }

    public void StartFlying()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            Time.deltaTime * speed
            );
        if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
        {
            currentState = GhostState.Possessing;
        }
    }
}
    

