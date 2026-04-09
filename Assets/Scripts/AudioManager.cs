using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sound;

    // Car sound
    float carSoundTimer;
    [SerializeField] float carTimeIntervalMin;
    [SerializeField] float carTimeIntervalMax;

    Vector2 carSoundTimeInterval;

    [SerializeField] int spatialSourcePoolSize = 10;
    Queue<AudioSource> _spatialPool = new();

    //to call anywhere use AudioManager.Instance.PlayAudio("name of sound")


    private void Awake()
    {

        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (Sound s in sound)
        {
            //creates a new audio source for each sound in the array
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
        for (int i = 0; i < spatialSourcePoolSize; i++)
        {
            var go = new GameObject("SpatialAudioSource");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f; // full 3D
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            go.SetActive(false);
            _spatialPool.Enqueue(src);
        }
    }

    private void Start()
    {
        //Car Sound
        carSoundTimeInterval = new Vector2(carTimeIntervalMin, carTimeIntervalMax);
        carSoundTimer = UnityEngine.Random.Range(carSoundTimeInterval[0], carSoundTimeInterval[1]);

        PlayAudio("BirdSound");
    }

    void Update()
    {
        //Car sound
        if (carSoundTimer > 0) carSoundTimer -= Time.deltaTime;
        if (carSoundTimer <= 0)
        {
            PlayAudio("CarSound");
            carSoundTimer = UnityEngine.Random.Range(carSoundTimeInterval[0], carSoundTimeInterval[1]);
        }
    }
    public void PlayAudioAtPosition(string name, Vector3 position)
    {
        Sound s = Array.Find(sound, sound => sound.name == name);
        if (s == null) { Debug.LogWarning($"Sound not found: {name}"); return; }

        if (_spatialPool.Count == 0) { Debug.LogWarning("Spatial pool exhausted"); return; }

        AudioSource src = _spatialPool.Dequeue();
        src.gameObject.SetActive(true);
        src.transform.position = position;
        src.clip = s.clip;
        src.volume = s.volume;
        src.loop = false;
        src.Play();

        StartCoroutine(ReturnToPool(src, s.clip.length));
    }

    IEnumerator ReturnToPool(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        src.Stop();
        src.gameObject.SetActive(false);
        _spatialPool.Enqueue(src);
    }

    public void PlayAudio (string name)
    {
        //This function is legacy and should not be called by any scripts in the latest game version
        Sound s = Array.Find(sound, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log($"PlayAudio, failed song load: {name}");
            return;
        }
        else
        {
            s.source.Play();
        }
    }

    public void StopAudio(string name)
    {
        Sound s = Array.Find(sound, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log($"StopAudio, failed song load: {name}");
        }
        else
        {
            s.source.Stop();
        }
        
    }

    public Sound GetSound(string name)
    {
        Sound s = Array.Find(sound, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log($"Failed to find sound: {name}");
            return new Sound();
        }
        return s;
    }
}
