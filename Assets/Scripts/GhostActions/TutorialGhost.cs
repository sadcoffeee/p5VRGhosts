using UnityEngine;
using System.Collections;

public class TutorialGhost : MonoBehaviour
{

    public GameObject possessObject;
    private FlyTowardsGhost ghostStates;
    public Transform roomPoint1;
    public Transform roomPoint2;
    public Transform roomPoint3;
    private float flyTime = 3f;
    private float pauseInAir = 1.5f;
    private PossessedObject possessedScript;
    private GhostAnimations anim;
    public GameObject flashlightTutorial;
    public GameObject controllerTutorial;
    public GameObject Hand;
    public GameObject flashlight;
    public GameObject lookAtObject1;
    public GameObject lookAtObject2;
    private float idleFloatTime = 0.8f;

    void Start()
    {
        if (!GameManager.Instance.tutorialDone)
        {
            anim = GetComponent<GhostAnimations>();
            ghostStates = GetComponent<FlyTowardsGhost>();
            possessedScript = possessObject.GetComponent<PossessedObject>();
            StartCoroutine(TutorialSequence());
            flashlight.SetActive(false);
        }
    }

    private IEnumerator TutorialSequence()
    {
        //Fly into the room
        yield return MoveGhost(this.transform.position, roomPoint1.position, flyTime);
        anim.PlayHappyFlying();

        //Fly to first spot
        yield return StartCoroutine(SmoothTransform(roomPoint2.transform, 1.2f)); 
        yield return MoveGhost(this.transform.position, roomPoint2.position, flyTime);

        //Look at first object
        yield return StartCoroutine(SmoothTransform(lookAtObject1.transform, 0.5f));
        anim.PlayExcited();
        yield return new WaitForSeconds(idleFloatTime);
        anim.PlayHappyFlying();

        //Fly to second spot
        yield return StartCoroutine(SmoothTransform(roomPoint3.transform, 0.5f));
        yield return MoveGhost(this.transform.position, roomPoint3.position, flyTime);

        //Look at second object
        yield return StartCoroutine(SmoothTransform(lookAtObject2.transform, 0.5f));
        anim.PlayExcited();
        yield return new WaitForSeconds(idleFloatTime);

        //Flashlight and surprised
        flashlight.SetActive(true);
        yield return StartCoroutine(SmoothTransform(Hand.transform, 0.3f));
        yield return new WaitForSeconds(idleFloatTime);
        anim.Caught();


        //Pause while shock animation plays, fly towards furniture 
        yield return new WaitForSeconds(pauseInAir);
        anim.PlayFlying();
        this.transform.LookAt(possessObject.transform.position);
        yield return MoveGhost(this.transform.position, possessObject.transform.position, flyTime);
        this.ghostStates.currentState = FlyTowardsGhost.GhostState.Possessing;
        possessedScript.SetPossessed(true, this.gameObject);
        HideGhost(false);
        flashlightTutorial.SetActive(true);
        yield return new WaitUntil(() => this.ghostStates.currentState == FlyTowardsGhost.GhostState.Stunned);
        HideGhost(true);
        flashlightTutorial.SetActive(false);
        controllerTutorial.SetActive(true);
        yield return new WaitUntil(() => this.ghostStates.currentState == FlyTowardsGhost.GhostState.Grabbed);
        controllerTutorial.SetActive(false);
        GameManager.Instance.EndTutorial();

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
    private void HideGhost(bool hidden)
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(hidden);
        }
    }

    private IEnumerator SmoothTransform(Transform lookAtTarget, float duration)
    {
        Quaternion startRot = transform.rotation;
        Vector3 direction = (lookAtTarget.position - transform.position).normalized;
        Quaternion endRot = Quaternion.LookRotation(direction);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, timer / duration);
            yield return null;
        }
        //transform.rotation = endRot;
    }
}
