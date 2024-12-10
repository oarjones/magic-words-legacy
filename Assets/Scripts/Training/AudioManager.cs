using Assets.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource gameAudioSource;
    public AudioSource effectsAudioSource;
    public List<SoundsKeyValuePair> GameSounds = new List<SoundsKeyValuePair>();


    public void PlaySound(SoundTypes soundType)
    {
        effectsAudioSource = GetComponent<AudioSource>();
        effectsAudioSource.PlayOneShot(GameSounds.Where(c => c.key == soundType).FirstOrDefault().val);
        Debug.Log("started");
    }

    public void PlayMusic(SoundTypes soundType)
    {
        if (gameAudioSource.isPlaying) return;
        gameAudioSource.Play();
    }

    public void StopMusic(SoundTypes soundType)
    {
        if (gameAudioSource.isPlaying)
            gameAudioSource.Stop();
    }

    

    //void Awake()
    //{
    //    GameSounds.Add(new SoundsKeyValuePair() { key = SoundTypes.SELECT_TILE, val = (AudioClip)Resources.Load("select", typeof(AudioClip)) });
    //    GameSounds.Add(new SoundsKeyValuePair() { key = SoundTypes.DESELECT_TILE, val = (AudioClip)Resources.Load("deselect", typeof(AudioClip)) });
    //    GameSounds.Add(new SoundsKeyValuePair() { key = SoundTypes.INVALID_WORD, val = (AudioClip)Resources.Load("invalid_word", typeof(AudioClip)) });
    //}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
