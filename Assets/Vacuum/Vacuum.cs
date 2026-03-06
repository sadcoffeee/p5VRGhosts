using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;


public class Vacuum : MonoBehaviour
{ 
    public enum VacuumState //defines 3 distinct behaviors
    {
        Idle, //not interacting with any object
        Sucking, //oull the object towards you
        Holding //object is locked to the vacuum, waiting to be shoot
    }

    [Header("Input")]
    public InputActionProperty activeVacuumTrigger;

    [Header("Vacuum Settings")]
    public float suckSpeed = 5f;
    public float spinSpeed = 200f;
    public float lockDistance = 0.05f;
    public float shootForce = 0.1f;

    private float suckBlockTimer = 0f;
    public float suckBlockDuration = 1f;

    [Header("References")]
    public HapticImpulsePlayer hapticPlayerR;
    public GameObject effect;

    private VacuumState state = VacuumState.Idle; //current state

    private GameObject currentGhost;
    private Rigidbody ghostRb;

    private Vector3 originalLocalPosition;

    private bool lastTriggerState;
    //public GameObject vacuum;
    
    public VibratorController controller;


    private void Start()
    {
        originalLocalPosition = transform.localPosition;
        hapticPlayerR = GetComponent<HapticImpulsePlayer>();
    }

    private void OnEnable()
    {
        activeVacuumTrigger.action.Enable();
    }

    private void OnDisable()
    {
        activeVacuumTrigger.action.Disable();
    }


    private void Update()
    {
                
        float triggerValue = activeVacuumTrigger.action.ReadValue<float>(); //return either 0 or 1, can be changed to a value
        bool triggerPressed = triggerValue > 0.1f; //convert to a bool if it is pressed or not

        switch (state)
        {
            case VacuumState.Idle:
                IdleState(triggerPressed);
                break;

            case VacuumState.Sucking:
                SuckingState(triggerPressed);
                break;

            case VacuumState.Holding:
                HoldingState(triggerPressed);
                break;
        }

        //saves the trigger state for next frame so you can detect if trigger pressed this frame but NOT pressed last frame
        lastTriggerState = triggerPressed;


        if (suckBlockTimer > 0)
            suckBlockTimer -= Time.deltaTime;

    }

    // ------------------ STATES ------------------

    void IdleState(bool triggerPressed)
    {
        //if trigger is pressed and ghost is in range -> begin sucking
        if (triggerPressed && currentGhost != null)
        {
            state = VacuumState.Sucking;
        }
    }

    void SuckingState(bool triggerPressed)
    {
        //if trigger is released -> stop sucking
        if (!triggerPressed)
        {
            StopSucking();
            return;
        }

        //apply effects
        ApplyVacuumEffects();

        //move ghost towards vacuum
        MoveGhostToVacuum();

        //if close enough -> lock ghost
        if (Vector3.Distance(transform.position, currentGhost.transform.position) < lockDistance)
        {
            state = VacuumState.Holding;
        }
    }

    void HoldingState(bool triggerPressed)
    {
        HoldGhost();

        //turn of shake and particle
        ResetVacuumEffects();

        // if trigger transitions from unpressed -> pressed... shoot object
        if (triggerPressed && !lastTriggerState)
        {
            ShootGhost();
        }
    }

    // ------------------ COLLISION ENTRY ------------------
    //called when cone trigger detects an object
    public void StartSucking(GameObject ghost)
    {

        if (suckBlockTimer > 0f) return;   // <--- IMPORTANT

        //ignores new objects if we already have one
        if (currentGhost != null)
            return;

        currentGhost = ghost; //reference to ghost we are interacting with
        ghostRb = ghost.GetComponent<Rigidbody>();// reference to the objects rigidbody

        if (ghostRb != null)
            ghostRb.isKinematic = true; //freeze the object


        state = VacuumState.Sucking;
    }

    // ------------------ SUCK LOGIC ------------------

    void MoveGhostToVacuum()
    {
        if (currentGhost == null) return;

        //gives a vector pointing from object to vacuum
        Vector3 dir = (transform.position - currentGhost.transform.position).normalized;
        //add a small step each frame
        currentGhost.transform.position += dir * suckSpeed * Time.deltaTime;
        //Spin the object for visual effect
        currentGhost.transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
    }


    void HoldGhost()
    {
        if (currentGhost == null) return;

        //locks the object to the tip
        currentGhost.transform.position = transform.position + transform.up*0.4f;
        currentGhost.transform.rotation = transform.rotation;
    }

    void StopSucking()
    {
        //turn of shake and particle
        ResetVacuumEffects();

        //unfreeze physics
        if (ghostRb != null)
            ghostRb.isKinematic = false;

        //prevent accidental modification
        currentGhost = null;
        ghostRb = null;

        //sets state to idle
        state = VacuumState.Idle;
    }

    // ------------------ SHOOT ------------------

    void ShootGhost()
    {
        if (ghostRb == null) return;

        ghostRb.isKinematic = false;

        ghostRb.AddForce(transform.up * shootForce, ForceMode.Impulse);

        if (hapticPlayerR != null)
            hapticPlayerR.SendHapticImpulse(1f, 0.2f);

        //controller.SendArduinoSignal("PC", 165);
        //controller.SendArduinoSignal("PL", 165);

        //block the ability to suck objects for a short time
        suckBlockTimer = suckBlockDuration;
        //clear reference
        currentGhost = null;
        ghostRb = null;


        //return to idle
        state = VacuumState.Idle;
    }




    // ------------------ EFFECTS ------------------

    void ApplyVacuumEffects()
    {
        //apply shaking to vacuum
        Vector3 randomOffset = Random.insideUnitSphere * 0.01f;
        transform.localPosition = originalLocalPosition + randomOffset;

        //controller vibration
        if (hapticPlayerR != null)
            hapticPlayerR.SendHapticImpulse(0.9f, 0.1f);

        //controller.SendArduinoSignal("PC", 165);
        //controller.SendArduinoSignal("PL", 165);

        //particle system
        if (effect != null)
            effect.SetActive(true);
    }

    void ResetVacuumEffects()
    {
        //return controller to orginal position
        transform.localPosition = originalLocalPosition;

        //turn off particle system
        if (effect != null)
            effect.SetActive(false);
    }
}
