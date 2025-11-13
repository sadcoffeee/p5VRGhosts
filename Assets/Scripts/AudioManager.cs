using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sound;

    // Car sound
    float carSoundTimer;
    [SerializeField] float carTimeIntervalMin;
    [SerializeField] float carTimeIntervalMax;
    Vector2 carSoundTimeInterval;

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

    public void PlayAudio (string name)
    {
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

    
}
