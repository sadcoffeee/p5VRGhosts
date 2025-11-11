using UnityEngine;

public class FlyTowardsGhost : MonoBehaviour
{
    public float speed = 5f;
    public float arriveDistance = 0.1f; //rammer couch
    public float hoverDelay = 3f; //test timer før vi flyver mod couch
    public float stunDelay = 5f;
    public GameObject Ghost;
    [HideInInspector] public bool grabbable;
    [HideInInspector] public float spawnTime;
    int targetChosen;
    private PossessedObject possessed;
    private Transform target;
    private HoverGhost hoverScript; //så vi kan finde hoverghost scriptet de kan snakke sammen
    private Vector3 middleTargetPosition;
    private GhostAnimations anim;
    private PossessedObject lastPossessed;

    public enum GhostState
    {
        Hovering,
        FlyingTowardsObject,
        Possessing,
        FlyToMiddle,
        Stunned,
        Grabbed,
        DestroyedFurniture
    };

    private float hoverTimer = 0f;
    private float stunTimer = 0f;
    public GhostState currentState = GhostState.FlyToMiddle;

    void Start()
    {
        //vi henter hoverscriptet nu
        hoverScript = GetComponent<HoverGhost>();
        stunTimer = stunDelay;
        GameManager.Instance.RegisterGhost(this);
        anim = GetComponent<GhostAnimations>();
    }

    void Update()
    {
        if (!GameManager.Instance.tutorialDone)
            return;

        switch (currentState)
        {
            case GhostState.FlyToMiddle:
                if (middleTargetPosition == null)
                {
                    middleTargetPosition = GetRandomHoverPosition();
                    transform.LookAt(middleTargetPosition);
                }
                FlyToTarget(middleTargetPosition, GhostState.Hovering);
                break;
            
            case GhostState.Hovering:
                anim.PlayFlying();
                hoverScript.enabled = true; // keep hover active
                Ghost.SetActive(true);
                Hovering();
                break;

            case GhostState.FlyingTowardsObject:
                hoverScript.enabled = false; // pause hover while flying
                if (target == null)
                {
                    possessed = GameManager.Instance.GetRandomFreeObject(lastPossessed);
                    lastPossessed = possessed;
                    target = possessed.transform;
                    transform.LookAt(target);
                }
                FlyToTarget(target.position, GhostState.Possessing);
                break;

            case GhostState.Possessing:
                possessed.SetPossessed(true, this.gameObject);
                target = null;
                Ghost.SetActive(false);
                break;

            case GhostState.DestroyedFurniture:
                Ghost.SetActive(true);
                currentState = GhostState.Hovering;
                //Debug.Log("DU KOM HER TIL");
                break;

            case GhostState.Stunned:
                grabbable = true;
                hoverScript.startPosition = transform.position;
                if (stunTimer > 0) stunTimer -= Time.deltaTime;
                else
                {
                    stunTimer = stunDelay;
                    grabbable = false;
                    currentState = GhostState.Hovering;
                }
                break;
            case GhostState.Grabbed:
                break;

        }
    }

    private void Hovering()
    {
        hoverTimer += Time.deltaTime;
        if (hoverTimer >= hoverDelay)
        {
            currentState = GhostState.FlyingTowardsObject;
            hoverTimer = 0;
        }
    }

    public void FlyToTarget(Vector3 target, GhostState nextState)
    {
        anim.PlayFlying();
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            Time.deltaTime * speed
            );
        if (Vector3.Distance(transform.position, target) <= arriveDistance)
        {
            currentState = nextState;
        }
    }

    public Vector3 GetRandomHoverPosition()
    {
        return new Vector3(Random.Range(-2f, 2f), Random.Range(0.2f, 2f), Random.Range(-7f, -3f));
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnGhostDefeated(this, Time.time - spawnTime);
    }
}
    

