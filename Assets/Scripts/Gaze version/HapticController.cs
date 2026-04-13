using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;


public class HapticController : MonoBehaviour
{
    [Header("Ramp Settings")]
    public float rampSpeed = 3f;
    public float impulseDuration = 0.02f;

    private HapticImpulsePlayer _hapticPlayer;
    private float _currentAmplitude = 0f;
    private float _targetAmplitude  = 0f;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _hapticPlayer = GetComponent<HapticImpulsePlayer>();
        if (_hapticPlayer == null)
            Debug.LogWarning("[HapticController] No HapticImpulsePlayer found on this GameObject.");
    }

    private void Update()
    {
        // Step linearly toward the target amplitude this frame
        _currentAmplitude = Mathf.MoveTowards(_currentAmplitude, _targetAmplitude, rampSpeed * Time.deltaTime);

        // Fire a short impulse at the current amplitude every frame.
        // The impulse duration slightly overlaps the next frame's impulse, which is intentional
        if (_currentAmplitude > 0f && _hapticPlayer != null)
            _hapticPlayer.SendHapticImpulse(_currentAmplitude, impulseDuration);
    }


    // -------------------------------------------------------------------------
    // API
    // -------------------------------------------------------------------------
    public void SendOnce(float amplitude, float duration)
    {
        if (_hapticPlayer != null)
            _hapticPlayer.SendHapticImpulse(amplitude, duration);
    }

    public void SetTarget(float amplitude)
    {
        _targetAmplitude = Mathf.Clamp01(amplitude);
    }


    public void CutOff()
    {
        _currentAmplitude = 0f;
        _targetAmplitude = 0f;
    }
}
