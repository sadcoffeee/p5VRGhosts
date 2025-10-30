using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;

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
    

    [Header("Variables")]
    [SerializeField] float maxLength = 10;
    [SerializeField] float flySpeed = 1;
    [SerializeField] float returnSpeed = 1;
    [SerializeField] float rotationSpeed = 2;
    [SerializeField] GrabblingState currState = GrabblingState.Idle;


    [HideInInspector] public GameObject grabbed;
    private bool grabbing = false;
    private Vector3 hookTarget = Vector3.zero;
    private Vector3 lastHandPos = Vector3.zero;
    private Vector3 handVelocity;



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

                        grabbed = null;
                        grabbing = false;
                    }
                }

                break;

            case GrabblingState.Shooting: 
                float outStep = flySpeed * Time.deltaTime;
                hand.transform.position = Vector3.MoveTowards(hand.transform.position, hookTarget, outStep);

                if (Vector3.Distance(hand.transform.position, hookTarget) < 0.01f)
                {
                    Grablinghand grabHand = hand.GetComponent<Grablinghand>();
                    GameObject touched = grabHand.HandTouched();
                    if (touched != null)
                    {
                        if (touched.CompareTag("Ghost"))
                        {
                            FlyTowardsGhost ghostScript = touched.GetComponent<FlyTowardsGhost>();
                            if (ghostScript.currentState == FlyTowardsGhost.GhostState.Stunned)
                            {
                                grabbed = touched;
                                grabbed.transform.SetParent(hand.transform);
                                grabbing = true;
                            }
                        }
                        else if (touched.CompareTag("Grabbable"))
                        {
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

                break;

            case GrabblingState.Returning:
                float inStep = returnSpeed * Time.deltaTime;
                hand.transform.position = Vector3.MoveTowards(hand.transform.position, this.transform.position, inStep);

                hand.transform.rotation = Quaternion.Slerp(hand.transform.rotation, this.transform.rotation, rotationSpeed * Time.deltaTime);

                if (Vector3.Distance(hand.transform.position, this.transform.position) < 0.1f)
                {
                    hookTarget = Vector3.zero;
                    hand.transform.SetLocalPositionAndRotation(this.transform.position, this.transform.rotation);
                    hand.transform.SetParent(this.transform);
                    currState = GrabblingState.Idle;
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
}
