using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class Grablinghook : MonoBehaviour
{
    private enum GrabblingState
    {
        Idle,
        Shooting,
        Returning
    }

    [Header("Inputs")]
    public InputActionReference trgRight;

    [Header("ObjectParts")]
    [SerializeField] GameObject hand;
    [SerializeField] GameObject forearm;
    [SerializeField] GameObject ghLineRenderer;
    [SerializeField] GameObject handOffset;


    [Header("Variables")]
    [SerializeField] float maxLength = 10;
    [SerializeField] float flySpeed = 1;
    [SerializeField] float returnSpeed = 1;
    [SerializeField] float rotationSpeed = 2;
    [SerializeField] GrabblingState currState = GrabblingState.Idle;

    [Header("Arm Haptics")]
    [SerializeField] VibratorController vController;
    [SerializeField] int shootVibration;
    [SerializeField] int pullWweigthVibration;
    [SerializeField] int pullWOweigthVibration;
    private HapticImpulsePlayer hapticPlayer;


    [HideInInspector] public GameObject grabbed;
    private bool grabbing = false;
    private Vector3 hookTarget = Vector3.zero;
    private Vector3 lastHandPos = Vector3.zero;
    private Vector3 handVelocity;
    [SerializeField] private GrablinghandAnimations handAnim;

    private float originalGrabDistance = 0f; //this

    //Sound
    bool shotSoundPlayed = false;
    bool returnSoundPlayed = false;

    void Start()
    {
        hapticPlayer = GetComponent<HapticImpulsePlayer>();
    }

    void Update()
    {
        float trgRightValue = trgRight.action.ReadValue<float>();

        switch (currState)
        {
            case GrabblingState.Idle:
                if (trgRightValue != 0 && !grabbing)
                {
                    hand.transform.SetParent(null);
                    if (hookTarget == Vector3.zero) hookTarget = SetTarget();
                    currState = GrabblingState.Shooting;
                    handAnim.PlayHandReaching();
                }

                if (grabbing)
                {
                    if (trgRightValue == 0)
                    {
                        grabbed.transform.SetParent(null);

                        if (grabbed.CompareTag("Ghost"))
                        {
                            Destroy(grabbed);
                        }
                        else if (grabbed.CompareTag("Grabbable"))
                        {
                            Rigidbody rb = grabbed.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = false;

                                // Apply throw force based on hand velocity
                                handVelocity = (hand.transform.position - lastHandPos) / Time.deltaTime;
                                rb.AddForce(handVelocity * 0.8f, ForceMode.VelocityChange);
                            }
                        }

                        handAnim.PlayHandIdle();

                        grabbed = null;
                        grabbing = false;
                    }
                }

                //Sound
                returnSoundPlayed = false;
                shotSoundPlayed = false;
                AudioManager.Instance.StopAudio("RopeStretchSound");
                AudioManager.Instance.StopAudio("HandReturnWindSound");

                break;

            case GrabblingState.Shooting:
                if (!shotSoundPlayed) AudioManager.Instance.PlayAudio("ShootSound"); shotSoundPlayed = true;
                float outStep = flySpeed * Time.deltaTime;
                hand.transform.position = Vector3.MoveTowards(hand.transform.position, hookTarget, outStep);

                if (Vector3.Distance(hand.transform.position, hookTarget) < 0.01f)
                {
                    GameObject touched = FindBestOverlapTarget();

                    if (touched != null)
                    {
                        if (touched.CompareTag("Ghost"))
                        {
                            handAnim.PlayHandGrabbed();

                            FlyTowardsGhost ghostScript = touched.GetComponent<FlyTowardsGhost>();
                            if (ghostScript.currentState == FlyTowardsGhost.GhostState.Stunned)
                            {
                                grabbed = touched;
                                grabbed.transform.SetParent(hand.transform);
                                ghostScript.currentState = FlyTowardsGhost.GhostState.Grabbed;
                                grabbing = true;

                                originalGrabDistance = Vector3.Distance(hand.transform.position, this.transform.position); //this
                            } else if (ghostScript.currentState == FlyTowardsGhost.GhostState.Hovering || ghostScript.currentState == FlyTowardsGhost.GhostState.FlyingTowardsObject)
                                AudioManager.Instance.PlayAudio("ghostLaughOther");
                        }
                        else if (touched.CompareTag("Grabbable"))
                        {
                            handAnim.PlayHandGrabbed();

                            grabbed = touched;

                            grabbed.transform.SetParent(hand.transform);

                            var grabComp = grabbed.GetComponent<Grabbable>();
                            if (grabComp != null)
                            {
                                grabbed.transform.localPosition = grabComp.holdOffset;
                                grabbed.transform.localEulerAngles = grabComp.holdRotation;
                            }
                            else
                            {
                                grabbed.transform.localPosition = Vector3.zero;
                                grabbed.transform.localRotation = Quaternion.identity;
                            }

                            Rigidbody rb = grabbed.GetComponent<Rigidbody>();
                            if (rb != null)
                                rb.isKinematic = true;

                            grabbing = true;
                        }
                        else if (touched.CompareTag("Button"))
                        {
                            touched.GetComponent<Button>().onClick.Invoke();
                        }
                    }

                    hand.transform.position = hookTarget;
                    hookTarget = Vector3.zero;
                    currState = GrabblingState.Returning;
                }
                if (trgRightValue == 0)
                {
                    hookTarget = Vector3.zero;
                    currState = GrabblingState.Returning;
                }


                // Haptics
                if (vController != null)
                {
                    if (vController.connectionEstablished)
                    {
                        vController.SendArduinoSignal("PL", shootVibration);
                        //hapticPlayer.SendHapticImpulse(0.1f, 0.1f);
                    }
                }

                break;

            case GrabblingState.Returning:
                float inStep = returnSpeed * Time.deltaTime;
                hand.transform.position = Vector3.MoveTowards(hand.transform.position, this.transform.position, inStep);

                //added this for shrinking when pulled in and grabbed :3
                if (grabbing && grabbed != null && grabbed.CompareTag("Ghost"))
                {
                    float currentDist = Vector3.Distance(hand.transform.position, this.transform.position);
                    float ratio = Mathf.Clamp01(currentDist / Mathf.Max(originalGrabDistance, 0.01f));
                    // shrinks gradually as it gets closer
                    grabbed.transform.localScale = Vector3.one * ratio;

                    grabbed.transform.position = hand.transform.position;

                }

                hand.transform.rotation = Quaternion.Slerp(hand.transform.rotation, handOffset.transform.rotation, rotationSpeed * Time.deltaTime);

                if (Vector3.Distance(hand.transform.position, this.transform.position) < 0.1f)
                {
                    hookTarget = Vector3.zero;
                    hand.transform.SetParent(handOffset.transform);

                    hand.transform.SetLocalPositionAndRotation(new Vector3(-0.0177f, -0.0348f, 0.1774f), Quaternion.identity);
                    currState = GrabblingState.Idle;
                }

                //sound
                if (!returnSoundPlayed)
                {
                    AudioManager.Instance.PlayAudio("HandReturnWindSound");
                    if (grabbing)
                    {
                        AudioManager.Instance.PlayAudio("RopeStretchSound");
                    }
                    returnSoundPlayed = true;
                }

                //Haptics
                if (vController != null)
                {
                    if (vController.connectionEstablished)
                    {
                        if (grabbing)
                        {
                            vController.SendArduinoSignal("PL", pullWweigthVibration);
                            //hapticPlayer.SendHapticImpulse(0.3f, 0.1f);
                        }
                        else
                        {
                            vController.SendArduinoSignal("PL", pullWOweigthVibration);
                            //hapticPlayer.SendHapticImpulse(0.1f, 0.1f);
                        }
                    }
                }

                break;
        }

        handVelocity = (hand.transform.position - lastHandPos) / Time.deltaTime;
        lastHandPos = hand.transform.position;
    }

    private Vector3 SetTarget()
    {
        Vector3 targetPos;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            targetPos = hit.point;
        }
        else
        {
            targetPos = transform.position + transform.forward * maxLength;
        }

        return targetPos;
    }
    private GameObject FindBestOverlapTarget()
    {
        float overlapRadius = 0.1f;
        Collider[] hits = Physics.OverlapSphere(hand.transform.position, overlapRadius);

        GameObject bestTarget = null;

        // Highest priority = ghost, then grabbable
        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            if (hit.CompareTag("Ghost"))
                return hit.gameObject;

            if (bestTarget == null && hit.CompareTag("Grabbable"))
                bestTarget = hit.gameObject;

            if (bestTarget == null && hit.CompareTag("Button"))
                bestTarget = hit.gameObject;
        }

        return bestTarget;
    }
}
