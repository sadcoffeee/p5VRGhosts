using UnityEngine;


public class MiniGhostFloater : MonoBehaviour
{
    [Header("Float Settings")]
    public float moveSpeed = 0.005f;
    public float directionChangeInterval = 1.8f;
    public float bobAmplitude = 0.002f;
    public float rotationSpeed = 90f;

    // Capsule bounds set by GhostCapsuleManager
    private float _capsuleRadius;
    private float _capsuleHalfHeight;
    private float _ghostRadius;

    private Vector3 _velocity;
    private float _directionTimer;
    private float _bobPhase;

    public void Initialize(float capsuleRadius, float capsuleHalfHeight, float ghostRadius)
    {
        _capsuleRadius = capsuleRadius;
        _capsuleHalfHeight = capsuleHalfHeight;
        _ghostRadius = ghostRadius;

        // Start drifting in a random direction
        PickNewDirection();
        // Stagger timers
        _directionTimer = Random.Range(0f, directionChangeInterval);
        _bobPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        // Direction change timer
        _directionTimer -= Time.deltaTime;
        if (_directionTimer <= 0f)
        {
            PickNewDirection();
            _directionTimer = directionChangeInterval + Random.Range(-0.3f, 0.3f);
        }

        // Move in local space (parent is the vacuum capsule)
        Vector3 localPos = transform.localPosition;

        // Bob offset
        float bob = Mathf.Sin(_bobPhase + Time.time * 2f) * bobAmplitude;

        localPos += (_velocity + Vector3.up * bob) * Time.deltaTime;

        // Constrain to capsule
        localPos = ConstrainToCapsule(localPos);

        transform.localPosition = localPos;

        // Smoothly rotate to face movement direction
        if (_velocity.sqrMagnitude > 0.00001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(_velocity);
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // Capsule confinement
    private Vector3 ConstrainToCapsule(Vector3 localPos)
    {
        float effectiveR = _capsuleRadius - _ghostRadius;
        float effectiveHH = _capsuleHalfHeight;

        // The closest point on the capsule's central line segment to localPos
        float clampedY = Mathf.Clamp(localPos.y, -effectiveHH, effectiveHH);
        Vector3 axisPoint = new Vector3(0f, clampedY, 0f);

        Vector3 toPos = localPos - axisPoint;
        float distXZ = toPos.magnitude;

        if (distXZ > effectiveR)
        {
            // Push back inside
            Vector3 normal = toPos / distXZ;
            localPos = axisPoint + normal * effectiveR;

            // Reflect velocity off the capsule wall
            _velocity = Vector3.Reflect(_velocity, -normal);
        }

        return localPos;
    }

    // Direction helper
    private void PickNewDirection()
    {
        // Random direction on the unit sphere, scaled to moveSpeed
        _velocity = Random.onUnitSphere * moveSpeed;
    }
}
