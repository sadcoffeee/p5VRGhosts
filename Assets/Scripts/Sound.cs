using UnityEngine.Audio;
using UnityEngine;

// it is a data container that defines a sound
[System.Serializable]
public class Sound{

    public string name; //unique name to find the sound

    public AudioClip clip; //actual audio clip

    [Range(0f,1f)]
    public float volume; //default volume

    [HideInInspector]
    public AudioSource source; // the AudioSource created at runtime, it is used in audio manager

    public bool loop;


}
