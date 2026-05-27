using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;


public class HapticController : MonoBehaviour
{
    [Header("Ramp Settings")]
    public float rampSpeed = 3f;
    public float impulseDuration = 0.02f;

    [Header("Drift Settings")]
    [Range(0f, 0.5f)]
    public float maxDrift = 0.15f;
    public float driftIntervalMin = 0.08f;
    public float driftIntervalMax = 0.25f;


    private HapticImpulsePlayer _hapticPlayer;
    private float _currentAmplitude = 0f;
    private float _targetAmplitude = 0f;
    private float _blockTimer = 0f;
    private float _driftOffset = 0f;
    private float _driftTimer = 0f;
    private float _latestValue;
    private bool _doHaptics;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _hapticPlayer = GetComponent<HapticImpulsePlayer>();
        if (_hapticPlayer == null)
            Debug.LogWarning("[HapticController] No HapticImpulsePlayer found on this GameObject.");

        PickNewDriftOffset();
    }
    private void Start()
    {
        Conditions currentCondition = GazeGameManager.Instance.condition;
        if (currentCondition == Conditions.AllVibration || currentCondition == Conditions.HandVibration)
            _doHaptics = true;
        else _doHaptics = false;
    }

    private void Update()
    {
        // Tick down the SendOnce block first; skip everything else while active
        if (_blockTimer > 0f || !_doHaptics)
        {
            _blockTimer -= Time.deltaTime;
            return;
        }

        // Step linearly toward the target amplitude this frame
        _currentAmplitude = Mathf.MoveTowards(_currentAmplitude, _targetAmplitude, rampSpeed * Time.deltaTime);
        

        // Advance drift timer and pick a new offset when it expires.
        // Drift is only applied while there's meaningful amplitude to vary
        if (_currentAmplitude > 0f && maxDrift > 0f)
        {
            _driftTimer -= Time.deltaTime;
            if (_driftTimer <= 0f)
                PickNewDriftOffset();
        }
        else
        {
            _driftOffset = 0f;
        }

        // Fire a short impulse at the drifted amplitude every frame.
        // The impulse duration slightly overlaps the next frame's impulse, which is intentional
        float driftedAmplitude = Mathf.Clamp01(_currentAmplitude + _driftOffset);

        _latestValue = driftedAmplitude;

        if (driftedAmplitude > 0f && _hapticPlayer != null)
            _hapticPlayer.SendHapticImpulse(driftedAmplitude, impulseDuration);
    }

    // -------------------------------------------------------------------------
    // API
    // -------------------------------------------------------------------------
    public void SendOnce(float amplitude, float duration)
    {
        if (_hapticPlayer == null) return;

        if (!_doHaptics) return;

        _hapticPlayer.SendHapticImpulse(amplitude, duration);
        _blockTimer = duration;

        _latestValue = amplitude;
    }

    public void SetTarget(float amplitude)
    {
        _targetAmplitude = Mathf.Clamp01(amplitude);
    }

    public void CutOff()
    {
        _currentAmplitude = 0f;
        _targetAmplitude = 0f;
        _driftOffset = 0f;
        _blockTimer = 0f;
    }
    public float GetLatestValue() => _latestValue;

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private void PickNewDriftOffset()
    {
        _driftOffset = Random.Range(-maxDrift, maxDrift);
        _driftTimer = Random.Range(driftIntervalMin, driftIntervalMax);
    }
}