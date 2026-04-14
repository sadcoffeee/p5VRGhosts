using System.Collections;
using UnityEngine;


public class SpatialAudioEmitter : MonoBehaviour
{
    public bool isPlaying => _sources[_activeIndex].isPlaying;


    private AudioSource[] _sources = new AudioSource[2];
    private int _activeIndex = 0;

    private Coroutine _fadeInCoroutine;
    private Coroutine _fadeOutCoroutine;

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            _sources[i] = gameObject.AddComponent<AudioSource>();
            _sources[i].spatialBlend = 1f;
            _sources[i].rolloffMode = AudioRolloffMode.Logarithmic;
            _sources[i].playOnAwake = false;
            _sources[i].volume = 0f;
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void Play(Sound s, float rampDuration = 0f)
    {
        int incomingIndex = 1 - _activeIndex;   // the source that isn't currently primary

        // Stop any in-progress fades on the incoming source so we start clean
        if (_fadeInCoroutine != null)
        {
            StopCoroutine(_fadeInCoroutine);
            _fadeInCoroutine = null;
        }

        if (_fadeOutCoroutine != null)
        {
            StopCoroutine(_fadeOutCoroutine);

            // Ensure outgoing source actually stops
            _sources[_activeIndex].Stop();
            _sources[_activeIndex].volume = 0f;

            _fadeOutCoroutine = null;
        }

        // Configure the incoming source
        AudioSource incoming = _sources[incomingIndex];
        incoming.clip = s.clip;
        incoming.loop = s.loop;
        incoming.volume = 0f;
        incoming.Play();

        if (rampDuration > 0f)
        {
            // Crossfade: ramp incoming up, ramp outgoing down, simultaneously
            _fadeInCoroutine = StartCoroutine(FadeVolume(incoming, 0f, s.volume, rampDuration));
            _fadeOutCoroutine = StartCoroutine(FadeAndStop(_sources[_activeIndex], rampDuration));
        }
        else
        {
            // Instant cut
            _sources[_activeIndex].Stop();
            _sources[_activeIndex].volume = 0f;
            incoming.volume = s.volume;
        }

        _activeIndex = incomingIndex;
    }

    public void Stop(float rampDuration = 0f)
    {
        // Kill running fades safely
        if (_fadeOutCoroutine != null)
        {
            StopCoroutine(_fadeOutCoroutine);
            _fadeOutCoroutine = null;
        }

        if (_fadeInCoroutine != null)
        {
            StopCoroutine(_fadeInCoroutine);
            _fadeInCoroutine = null;
        }

        if (rampDuration > 0f)
        {
            _fadeOutCoroutine = StartCoroutine(FadeAndStop(_sources[_activeIndex], rampDuration));
        }
        else
        {
            ForceStopAllSources();
        }
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;
        source.volume = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        source.volume = to;
    }

    private IEnumerator FadeAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        yield return FadeVolume(source, startVolume, 0f, duration);
        source.Stop();
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------
    private void ForceStopAllSources()
    {
        foreach (var src in _sources)
        {
            if (src.isPlaying)
            {
                src.Stop();
                src.volume = 0f;
            }
        }
    }
}
