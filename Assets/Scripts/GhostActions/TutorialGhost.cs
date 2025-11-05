using UnityEngine;
using System.Collections;

public class TutorialGhost : MonoBehaviour
{

    public GameObject possessObject;
    private FlyTowardsGhost ghostStates;
    public Transform roomPoint;
    private float flyTime = 3f;
    private float pauseInAir = 1.5f;
    private PossessedObject possessedScript;
    private GhostAnimations anim;

    void Start()
    {
        if (!GameManager.Instance.tutorialDone)
        {
            anim = GetComponent<GhostAnimations>();
            ghostStates = GetComponent<FlyTowardsGhost>();
            possessedScript = possessObject.GetComponent<PossessedObject>();
            StartCoroutine(TutorialSequence());
        }
    }

    private IEnumerator TutorialSequence()
    {
        //Fly into the room, play shock when arriving
        Vector3 startPos = ghostStates.transform.position;
        Vector3 endPos = roomPoint.position;
        yield return MoveGhost(startPos, endPos, flyTime);
        anim.PlayShocked();

        //Pause while shock animation plays, fly towards furniture 
        yield return new WaitForSeconds(pauseInAir);
        anim.PlayFlying();
        this.transform.LookAt(possessObject.transform.position);
        yield return MoveGhost(this.transform.position, possessObject.transform.position, flyTime);
        possessedScript.SetPossessed(true, this.gameObject);
        ghostStates.currentState = FlyTowardsGhost.GhostState.Possessing;
        this.gameObject.SetActive(false);

    }

    private IEnumerator MoveGhost(Vector3 from, Vector3 to, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
             timer += Time.deltaTime;
            this.transform.position = Vector3.Lerp(from, to, timer / duration);
            yield return null;
        }
    }
}
