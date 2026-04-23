using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class GazeVacuum : MonoBehaviour
{
    public enum VacuumState
    {
        Idle,
        Sucking,
        Holding
    }

    // What kind of object is currently being processed
    private enum ObjectType { None, Ghost, Toy }

    [Header("Input")]
    public InputActionReference activeVacuumTrigger;

    [Header("Vacuum Settings")]
    public float suckSpeed = 5f;
    public float spinSpeed = 200f;
    public float lockDistance = 0.05f;
    public float shootForce = 9f;

    private float suckBlockTimer = 0f;
    public  float suckBlockDuration = 1f;

    [Header("Feedback Ramp Durations")]
    public float audioRampDuration = 0.15f;
    public float audioCrossfadeDuration = 0.2f;

    [Header("References")]
    public HapticController hapticController;
    public GameObject suckEffect;
    public LineRenderer selectedObjectLine;

    // -------------------------------------------------------------------------
    // Runtime state
    // -------------------------------------------------------------------------
    private VacuumState state = VacuumState.Idle;
    private ObjectType  objectType = ObjectType.None;

    private List<GameObject> candidates = new List<GameObject>();
    private GameObject currentObject;
    private GameObject currentTarget;
    private Rigidbody objectRb;
    private SpatialAudioEmitter audioEmitter;
    private string currentSoundKey = "";

    // Ghost shrink: record the scale at the moment the ghost enters the vacuum
    private Vector3 ghostOriginalScale;
    private float ghostSuckDistance;   // distance when sucking started, for ratio

    private Vector3 originalLocalPosition;
    private bool lastTriggerState;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------
    private void Start()
    {
        originalLocalPosition = transform.localPosition;
        audioEmitter = GetComponent<SpatialAudioEmitter>();
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
        float triggerValue = activeVacuumTrigger.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.1f;

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

        lastTriggerState = triggerPressed;

        if (suckBlockTimer > 0f)
            suckBlockTimer -= Time.deltaTime;
    }

    // -------------------------------------------------------------------------
    // States
    // -------------------------------------------------------------------------

    void IdleState(bool triggerPressed)
    {
        OutlineCurrentObject();

        if (triggerPressed)
        {
            ApplyIdleEffect();

            if (currentObject == null)
                TryStartSucking();
        }
        else
        {
            ResetEffects();
        }
    }

    void SuckingState(bool triggerPressed)
    {
        if (!triggerPressed)
        {
            ReleaseObject();
            return;
        }

        ApplySuckEffect();
        MoveObjectToVacuum();

        if (currentObject == null) return; // if destroyed mid-frame (like ghost absorbed)

        float dist = Vector3.Distance(transform.position, currentObject.transform.position);

        if (objectType == ObjectType.Ghost)
        {
            // Shrink linearly from original scale down to zero as it closes in
            float ratio = Mathf.Clamp01(dist / Mathf.Max(ghostSuckDistance, 0.01f));
            currentObject.transform.localScale = ghostOriginalScale * ratio;

            // Absorbed — destroy and return to idle
            if (dist < lockDistance)
            {
                AbsorbGhost();
                return;
            }
        }
        else if (objectType == ObjectType.Toy)
        {
            if (dist < lockDistance)
                EnterHolding();
        }
    }

    void HoldingState(bool triggerPressed)
    {
        if (!triggerPressed)
        {
            ShootToy();
            return;
        }

        HoldToy();
        ApplyHoldEffect();
    }
    // -------------------------------------------------------------------------
    // Selecting candidate - used for outline
    // -------------------------------------------------------------------------
    GameObject SelectCandidate()
    {
        List<GameObject> toys = new List<GameObject>();

        foreach (GameObject _object in candidates)
        {
            if (_object == null) continue;

            if (_object.CompareTag("Ghost"))
            {
                return _object;
            }
            else if (_object.CompareTag("Toy"))
            {
                toys.Add(_object);
            }
        }

        if (toys.Count > 0) return toys[0];
        return null;
    }

    void OutlineCurrentObject()
    {

        currentTarget = SelectCandidate();

        if (currentTarget == null)
        {
            OutlineObject.Deselect();
            RemoveLineToObject();
            return;
        }

        OutlineObject outlineObject = currentTarget.GetComponent<OutlineObject>(); //does tge object support outlining

        if (outlineObject != null)
        {
            // Check if outlineObject is a hiding ghost before highlighting it
            GhostBehavior ghostBehavior = outlineObject.GetComponent<GhostBehavior>();
            if (ghostBehavior != null && !ghostBehavior.isGrabbable)
                return;

            outlineObject.Select();
            DrawLineToObject(outlineObject);
        }
        else
        {
            OutlineObject.Deselect();
            RemoveLineToObject();
        }
    }

    void DrawLineToObject(OutlineObject obj)
    {
        selectedObjectLine.SetPosition(0, selectedObjectLine.transform.position);
        selectedObjectLine.SetPosition(1, obj.transform.position);
    }
    void RemoveLineToObject()
    {
        selectedObjectLine.SetPosition(0, selectedObjectLine.transform.position);
        selectedObjectLine.SetPosition(1, selectedObjectLine.transform.position);
    }

    // -------------------------------------------------------------------------
    // Sucking - no longer driven by ConeCollider, instead driven directly in update
    // -------------------------------------------------------------------------
    void TryStartSucking()
    {
        // To give ghosts priority, loop through the whole list for ghosts first, then do it again for toys
        // Inelegant, but we have relatively few candidates and its a simple loop, so whatever
        foreach (var obj in candidates)
        {
            if (obj == null) continue;

            if (obj.CompareTag("Ghost"))
            {
                StartSuckingGhost(obj);
                RemoveLineToObject();
                return;
            }
        }
        foreach (var obj in candidates) //loops through every GameObject currently inside the vacuum’s detection area.
        {
            if (obj == null) continue; //if it does not exists, ignore

            else if (obj.CompareTag("Toy")) //if the object is a toy
            {
                HauntableToy hauntable = obj.GetComponent<HauntableToy>(); //looks for HauntableToy script on the toy, if there is one store in hauntable

                if (hauntable.IsHaunted()) //is the toy haunted, 
                {
                    hauntable.OnVacuumAttempt(); // tells the toy that it tried to vacuumed
                    suckBlockTimer = suckBlockDuration; //Temporarily disables vacuum sucking.

                    return;
                }
                else //the toy us not haunted and save to suck
                {
                    StartSuckingToy(obj);
                    RemoveLineToObject();
                    return;
                }

            }
        }
    }
    public void StartSuckingToy(GameObject toy)
    {
        if (suckBlockTimer > 0f || currentObject != null) return;

        currentObject = toy;
        objectType = ObjectType.Toy;
        objectRb = toy.GetComponent<Rigidbody>();

        if (objectRb != null)
            objectRb.isKinematic = true;

        state = VacuumState.Sucking;
    }
    public void StartSuckingGhost(GameObject ghost)
    {
        if (suckBlockTimer > 0f || currentObject != null) return;

        // Only accept stunned ghosts
        GhostBehavior behavior = ghost.GetComponent<GhostBehavior>();
        if (behavior == null || !behavior.isGrabbable) return;

        currentObject = ghost;
        objectType = ObjectType.Ghost;
        objectRb = ghost.GetComponent<Rigidbody>();
        ghostOriginalScale = ghost.transform.localScale;
        ghostSuckDistance = Vector3.Distance(transform.position, ghost.transform.position);

        if (objectRb != null)
            objectRb.isKinematic = true;

        behavior.OnGrabbed(); // notify ghost it's been caught

        state = VacuumState.Sucking;
    }
    void ReleaseObject()
    {
        ResetEffects();

        if (objectType == ObjectType.Ghost && currentObject != null)
        {
            GhostBehavior ghost = currentObject.GetComponent<GhostBehavior>();
            if (ghost != null)
                ghost.ReturnToStunned();
            currentObject.transform.localScale = ghostOriginalScale;
        }

        if (objectRb != null)
            objectRb.isKinematic = false;

        ClearCurrentObject();
        state = VacuumState.Idle;
    }
    // -------------------------------------------------------------------------
    // Suck movement
    // -------------------------------------------------------------------------

    void MoveObjectToVacuum()
    {
        if (currentObject == null) return;

        Vector3 dir = (transform.position - currentObject.transform.position).normalized;
        currentObject.transform.position += dir * suckSpeed * Time.deltaTime;
        currentObject.transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Ghost absorption
    // -------------------------------------------------------------------------

    void AbsorbGhost()
    {
        ResetEffects();

        if (hapticController != null)
        {
            hapticController.CutOff();
            hapticController.SendOnce(1f, 0.3f);
        }

        AudioManager.Instance.PlayAudioAtPosition("ghostAbsorbed", transform.position);

        UnregisterCandidate(currentObject);
        currentObject.GetComponent<GhostBehavior>().Die(true);
        ClearCurrentObject();
        state = VacuumState.Idle;
    }

    // -------------------------------------------------------------------------
    // Toy hold & shoot
    // -------------------------------------------------------------------------

    void EnterHolding()
    {
        state = VacuumState.Holding;
        SessionLogger.Instance.IncreaseToyHeldCount();
        ResetEffects();
    }

    void HoldToy()
    {
        if (currentObject == null) return;

        // Apply Grabbable hold offset if present, otherwise sit at nozzle tip
        var grabComp = currentObject.GetComponent<Grabbable>();
        if (grabComp != null)
        {
            // Express the offset in the vacuum's local space
            currentObject.transform.position = transform.TransformPoint(grabComp.holdOffset);
            currentObject.transform.rotation = transform.rotation * Quaternion.Euler(grabComp.holdRotation);
        }
        else
        {
            currentObject.transform.position = transform.position + transform.up * 0.1f;
            currentObject.transform.rotation = transform.rotation;
        }
    }
    void ShootToy()
    {
        if (objectRb == null) return;

        ResetEffects();

        objectRb.isKinematic = false;
        objectRb.AddForce(-transform.right * shootForce + transform.up * 0.1f * shootForce, ForceMode.Impulse);


        if (hapticController != null)
            hapticController.SendOnce(1f, 0.2f);

        suckBlockTimer = suckBlockDuration;
        ClearCurrentObject();
        state = VacuumState.Idle;
    }
    void ClearCurrentObject()
    {
        currentObject = null;
        objectRb = null;
        objectType = ObjectType.None;
    }

    // -------------------------------------------------------------------------
    // Effects
    // -------------------------------------------------------------------------
    // Plays a sound by key, crossfading if a different sound is already playing,
    // or fading in from silence if nothing is playing yet.
    void PlaySound(string key)
    {
        if (currentSoundKey == key && audioEmitter.isPlaying) return;

        float ramp = audioEmitter.isPlaying ? audioCrossfadeDuration : audioRampDuration;
        audioEmitter.Play(AudioManager.Instance.GetSound(key), ramp);
        currentSoundKey = key;
    }

    void ApplyHoldEffect()
    {
        // More intense shake than sucking
        transform.localPosition = originalLocalPosition + Random.insideUnitSphere * 0.015f;

        if (hapticController != null)
            hapticController.SetTarget(1f);

        if (suckEffect != null)
            suckEffect.SetActive(false);

        PlaySound("bigVacuum");
    }

    void ApplyIdleEffect()
    {
        if (hapticController != null)
            hapticController.SetTarget(0.4f);

        if (suckEffect != null)
            suckEffect.SetActive(true);

        PlaySound("smallVacuum");
    }

    void ApplySuckEffect()
    {
        transform.localPosition = originalLocalPosition + Random.insideUnitSphere * 0.01f;

        if (hapticController != null)
            hapticController.SetTarget(0.7f);

        if (suckEffect != null)
            suckEffect.SetActive(true);

        PlaySound("smallVacuum");
    }

    void ResetEffects()
    {
        transform.localPosition = originalLocalPosition;

        if (hapticController != null)
            hapticController.SetTarget(0f);

        if (suckEffect != null)
            suckEffect.SetActive(false);

        audioEmitter.Stop(audioRampDuration);
        currentSoundKey = "";
    }

    // -------------------------------------------------------------------------
    // API for collider
    // -------------------------------------------------------------------------
    public void RegisterCandidate(GameObject obj)
    {
        if (!candidates.Contains(obj))
            candidates.Add(obj);
    }
    public void UnregisterCandidate(GameObject obj)
    {
        candidates.Remove(obj);
    }

}
