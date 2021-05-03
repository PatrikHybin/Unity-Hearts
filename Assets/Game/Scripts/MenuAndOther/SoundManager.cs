using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{

    [SerializeField] private AudioClip[] audioClips;

    private AudioSource audioSource;
    public SoundManager soundManager;
    void Start()
    {
        audioClips = Resources.LoadAll<AudioClip>("Sound/");
        DontDestroyOnLoad(gameObject);
        soundManager = this;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySound(string name) {
        audioSource.PlayOneShot(GetAudioClip(name));
    }

    public void PlayCardSound(string name)
    {
        audioSource.PlayOneShot(GetCardAudioClip(name));
    }

    private AudioClip GetCardAudioClip(string name)
    {
        Debug.Log(name);
        List<AudioClip> tmpAudioClips = new List<AudioClip>();
        foreach (AudioClip clip in audioClips) {
            
            if (clip.name.Contains(name)) {
                tmpAudioClips.Add(clip);
            }
        }
        if (tmpAudioClips.Count > 0) {
            Random.InitState(DateTime.Now.Second);
            int clipId = Random.Range(0, tmpAudioClips.Count);
            audioSource.volume = Settings.gameVolume;
            return tmpAudioClips[clipId];
        }
        return null;
    }

    private AudioClip GetAudioClip(string name)
    {
        foreach (AudioClip clip in audioClips) {
            if (clip.name == name)
            {
                audioSource.volume = Settings.defualtEffectVolume;
                return clip;
            }
        }
        Debug.LogError("There is no sound with that name");
        return null;
    }
}
