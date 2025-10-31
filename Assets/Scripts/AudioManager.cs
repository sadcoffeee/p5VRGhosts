using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sound;

    //to call anywhere use FindObjectOfType<AudioManager>().PlayAudio("name of sound")


    private void Awake()
    {      
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
        PlayAudio("BackgroundMusic");
    }

    public void PlayAudio (string name)
    {
        Sound s = Array.Find(sound, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("failed song load");
            return;
        }

        s.source.Play();
    }

    
}
