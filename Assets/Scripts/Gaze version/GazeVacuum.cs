using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

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

    [Header("References")]
    public HapticImpulsePlayer hapticPlayerR;
    public GameObject suckEffect;

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
    private bool latestSoundBig = false;

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
        hapticPlayerR = GetComponent<HapticImpulsePlayer>();
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
        HoldToy();
        ResetEffects();

        // Press trigger again to shoot
        if (triggerPressed && !lastTriggerState)
            ShootToy();
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
            return;
        }

        OutlineObject outlineObject = currentTarget.GetComponent<OutlineObject>();
        if (outlineObject != null)
        {
            outlineObject.Select();
        }
        else
        {
            OutlineObject.Deselect();
        }
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
                return;
            }
        }
        foreach (var obj in candidates)
        {
            if (obj == null) continue;

            else if (obj.CompareTag("Toy"))
            {
                if (!obj.GetComponent<HauntableToy>().IsHaunted()) 
                {
                    StartSuckingToy(obj);
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

        if (hapticPlayerR != null)
            hapticPlayerR.SendHapticImpulse(1f, 0.3f);

        // AudioManager.Instance.PlayAudio("GhostAbsorbed"); TO DO: FIND SFX

        UnregisterCandidate(currentObject);
        Destroy(currentObject);
        ClearCurrentObject();
        state = VacuumState.Idle;
    }

    // -------------------------------------------------------------------------
    // Toy hold & shoot
    // -------------------------------------------------------------------------

    void EnterHolding()
    {
        state = VacuumState.Holding;
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

        objectRb.isKinematic = false;
        objectRb.AddForce(-transform.right * shootForce + transform.up * 0.1f * shootForce, ForceMode.Impulse);


        if (hapticPlayerR != null)
            hapticPlayerR.SendHapticImpulse(1f, 0.2f);

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
    void ApplyIdleEffect()
    {
        if (hapticPlayerR != null)
            hapticPlayerR.SendHapticImpulse(0.4f, 0.1f);

        if (suckEffect != null)
            suckEffect.SetActive(true);


        if (!audioEmitter.isPlaying || latestSoundBig) 
        {
            audioEmitter.Play(AudioManager.Instance.GetSound("smallVacuum"));
            latestSoundBig = false;
        }
    }
    void ApplySuckEffect()
    {
        transform.localPosition = originalLocalPosition + Random.insideUnitSphere * 0.01f;

        if (hapticPlayerR != null)
            hapticPlayerR.SendHapticImpulse(0.7f, 0.1f);

        if (suckEffect != null)
            suckEffect.SetActive(true);

        if (!audioEmitter.isPlaying || !latestSoundBig)
        {
            audioEmitter.Play(AudioManager.Instance.GetSound("bigVacuum"));
            latestSoundBig = true;
        }
    }
    void ResetEffects()
    {
        transform.localPosition = originalLocalPosition;

        if (suckEffect != null)
            suckEffect.SetActive(false);

        audioEmitter.Stop();
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
