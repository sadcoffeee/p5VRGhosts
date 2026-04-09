using UnityEngine;

public class SpatialAudioEmitter : MonoBehaviour
{
    AudioSource _source;
    public bool isPlaying = false;

    private void Awake()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _source.spatialBlend = 1f;
        _source.rolloffMode = AudioRolloffMode.Logarithmic;
        _source.playOnAwake = false;
    }

    public void Play(Sound s)
    {
        _source.clip = s.clip;
        _source.volume = s.volume;
        _source.loop = s.loop;
        _source.Play();
        isPlaying = true;
    }

    public void Stop()
    {
        _source.Stop();
        isPlaying = false;
    }
}