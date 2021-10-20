using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundClip
{
    public AudioClip inAudio;
    public AudioClip midAudio;
    public AudioClip outAudio;
    [Space(10)]
    public AudioSource source;

    public void Play()
    {
        source.clip = inAudio;
        source.loop = true;
        source.Play();
    }

    public void Stop()
    {
        source.loop = false;
        source.clip = outAudio;
        source.PlayScheduled(source.clip.length - source.time);
    }
}