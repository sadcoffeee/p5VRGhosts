using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Cone")]
    [SerializeField] float coneHalfAngle = 20f;
    [SerializeField] float coneRange = 12f;

    [Header("Haptic Armbands")]
    [SerializeField] VibratorController vibController;
    [SerializeField] float minVibration = 10f;
    [SerializeField] float maxVibration = 120f;
    [SerializeField] float hapticSmoothSpeed = 8f;
    [SerializeField] bool onlyHapticsForLingeringGhosts = true;

    [Header("Stun Reward Impulse")]
    [SerializeField] float rewardVibration = 150f;
    [SerializeField] float rewardDuration = 0.3f;

    [Header("Debug")]
    [SerializeField] bool drawDebugGizmos = true;
    [SerializeField] bool doVibrationVisualization = true;
    [SerializeField] bool doVibration = true;
    [SerializeField] VibrationVisualizer leftVibrationVisualizer;
    [SerializeField] VibrationVisualizer rightVibrationVisualizer;


    // Smoothed current vibration values so they don't jump abruptly
    private float currentLeftVib = 0f;
    private float currentRightVib = 0f;

    // Ghosts that were inside the cone last frame (for lost-notification)
    private HashSet<GhostBehavior> litLastFrame = new HashSet<GhostBehavior>();

    // Tracks whether any ghost is being actively lit this frame
    private bool ghostBeingLitThisFrame = false;

    // Stun reward impulse countdown (> 0 means impulse is active)
    private float rewardTimer = 0f;

    private void Start()
    {
        Conditions currentCondition = GazeGameManager.Instance.condition;
        if (currentCondition == Conditions.AllVibration || currentCondition == Conditions.ArmVibration)
            doVibration = true;
        else doVibration = false;
    }
    void Update()
    {
        GhostBehavior[] allGhosts = GazeGameManager.Instance.GetAllGhosts();
        Transform[] allToys = GazeGameManager.Instance.GetAllToys();

        HashSet<GhostBehavior> litThisFrame = new HashSet<GhostBehavior>();
        GhostBehavior nearestHapticGhost = null;
        float nearestDist = float.MaxValue;
        ghostBeingLitThisFrame = false;

        if (rewardTimer > 0f)
            rewardTimer -= Time.deltaTime;

        foreach (GhostBehavior ghost in allGhosts)
        {
            if (ghost == null) continue;

            Vector3 toGhost = ghost.transform.position - transform.position;
            float dist = toGhost.magnitude;

            if (dist > coneRange) continue;

            float angle = Vector3.Angle(transform.forward, toGhost);
            bool inCone = angle <= coneHalfAngle;

            if (inCone)
            {
                litThisFrame.Add(ghost);
                ghost.NotifyFlashlightHit(Time.deltaTime);
                if (ghost.currentState == GhostBehavior.GhostState.Lingering)
                    ghostBeingLitThisFrame = true;
            }

            // Haptic targeting: nearest eligible ghost regardless of cone
            bool eligibleForHaptics = !onlyHapticsForLingeringGhosts
                || ghost.currentState == GhostBehavior.GhostState.Lingering;

            if (eligibleForHaptics && dist < nearestDist)
            {
                nearestDist = dist;
                nearestHapticGhost = ghost;
            }
        }

        // Notify ghosts that left the cone
        foreach (GhostBehavior ghost in litLastFrame)
        {
            if (ghost != null && !litThisFrame.Contains(ghost))
                ghost.NotifyFlashlightLost();
        }
        litLastFrame = litThisFrame;

        // Drive armbands
        DriveHaptics(nearestHapticGhost);

        foreach (Transform toy in allToys)
        {
            // if toy is haunted and toy is inside cone, light it
            Vector3 toToy = toy.transform.position - transform.position;
            float dist = toToy.magnitude;

            if (dist > coneRange) continue;

            float angle = Vector3.Angle(transform.forward, toToy);
            bool inCone = angle <= coneHalfAngle;

            if (inCone) 
            {
                HauntableToy hauntableToy = toy.transform.GetComponent<HauntableToy>();

                if (hauntableToy != null && hauntableToy.IsHaunted())
                {
                    hauntableToy.FlashLightHit(Time.deltaTime);
                }
            }
        }
    }

    void DriveHaptics(GhostBehavior nearestGhost)
    {
        float targetLeft;
        float targetRight;

        if (rewardTimer > 0f)
        {
            // Reward impulse: both armbands spike to rewardVibration, still smoothed
            // still smoothed, just a very fast ramp up then natural decay
            targetLeft = rewardVibration;
            targetRight = rewardVibration;
        }
        else if (ghostBeingLitThisFrame)
        {
            // Ghost is actively being lit: both arms at maxVibration equally
            // Smoothing via hapticSmoothSpeed also keeps this a ramp, not a spike
            targetLeft = maxVibration;
            targetRight = maxVibration;
        }
        else if (nearestGhost == null)
        {
            // No ghost: both arms at minimum
            targetLeft = minVibration;
            targetRight = minVibration;
        }
        else
        {
            Vector3 toGhost = nearestGhost.transform.position - transform.position;

            // --- Signed horizontal offset from the gaze plane ---
            // The gaze plane normal is transform.forward (look direction).
            // We want the component of toGhost that is perpendicular to forward,
            // then project onto the world-right axis of the player.
            Vector3 projected = Vector3.ProjectOnPlane(toGhost, transform.forward);
            // Player's right in world space
            float signedRight = Vector3.Dot(projected, transform.right);
            // signedRight > 0 means ghost is to the right of the gaze plane

            // Normalize by a reasonable lateral range (3 metres feels good)
            float lateralRange = 3f;
            float blend = Mathf.Clamp(signedRight / lateralRange, -1f, 1f);
            // blend: -1 = hard left, 0 = center, +1 = hard right

            // --- Proximity intensity ---
            // As the angle to the ghost decreases, intensity rises.
            float angle = Vector3.Angle(transform.forward, toGhost);
            float searchRadius = coneHalfAngle * 3f;
            float proximityT = 1f - Mathf.Clamp01(angle / searchRadius);

            float range = maxVibration - minVibration;

            float leftT = Mathf.Clamp01((1f - blend) / 2f);
            float rightT = Mathf.Clamp01((1f + blend) / 2f);

            targetLeft = minVibration + range * leftT * Mathf.Lerp(0.5f, 1f, proximityT);
            targetRight = minVibration + range * rightT * Mathf.Lerp(0.5f, 1f, proximityT);

            float centerBoost = proximityT * minVibration;
            targetLeft = Mathf.Min(targetLeft + centerBoost, maxVibration);
            targetRight = Mathf.Min(targetRight + centerBoost, maxVibration);
        }

        // Smooth toward targets
        currentLeftVib = Mathf.Lerp(currentLeftVib, targetLeft, hapticSmoothSpeed * Time.deltaTime);
        currentRightVib = Mathf.Lerp(currentRightVib, targetRight, hapticSmoothSpeed * Time.deltaTime);

        // Only send commands if we've confirmed that there's a connection
        if (vibController.connectionEstablished && doVibration)
        {
            SendArmband(vibController, "PC", currentLeftVib);
            SendArmband(vibController, "PL", currentRightVib);
        }
        if (doVibrationVisualization)
        {
            leftVibrationVisualizer.SetVibrationValue(currentLeftVib);
            rightVibrationVisualizer.SetVibrationValue(currentRightVib);
        }
    }

    void SendArmband(VibratorController controller, string code, float intensity)
    {
        if (controller == null) return;
        if (!controller.connectionEstablished) return;
        controller.SendArduinoSignal(code, Mathf.RoundToInt(intensity));
    }
    // Called by GhostBehavior when a ghost is successfully stunned by the flashlight
    public void TriggerStunReward()
    {
        rewardTimer = rewardDuration;
    }

    // Used by logger to get vibration values on pain stimuli
    public float[] getVibrationValues()
    {
        float[] values = new float[2];
        values[0] = currentLeftVib;
        values[1] = currentRightVib;
        return values;
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;

        // Draw flashlight cone
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        float coneLength = coneRange;
        float coneRadius = Mathf.Tan(coneHalfAngle * Mathf.Deg2Rad) * coneLength;

        Vector3 tip = transform.position;
        Vector3 dir = transform.forward;

        // Four edge rays of the cone
        Vector3[] offsets = {
            transform.up,
            -transform.up,
            transform.right,
            -transform.right
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 edge = (dir * coneLength + offset * coneRadius).normalized * coneLength;
            Gizmos.DrawLine(tip, tip + edge);
        }

        // Draw the gaze plane normal
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(tip, tip + dir * 1f);
    }
#endif
}
