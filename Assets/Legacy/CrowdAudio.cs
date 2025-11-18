using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdAudio : MonoBehaviour
{
    public AudioClip start;
    public AudioClip crowd;

    AudioSource myAudioSource;
    
    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
        myAudioSource.clip = start;
        myAudioSource.Play();
    }

    public void musicReStart(){
        myAudioSource = GetComponent<AudioSource>();
        myAudioSource.clip = start;
        myAudioSource.Play();
    }

    public void MusicChange(){
        myAudioSource.Pause();
        myAudioSource.clip = crowd;
        myAudioSource.Play();
    }
    
    void Update()
    {
        if(!myAudioSource.isPlaying){
            myAudioSource.clip = start;
            myAudioSource.Play();
        }
    }
}
