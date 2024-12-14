using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//the audio manager for the game
//manages music and sfx
//i think something broke within the implementation but USUALLY this allows for the player
//to mute music / sfx or to toggle the volume of them
//on start there are 2 music options for now. eventually there will be others added to account for the other books
// and boss battles
public class AudioMgr : MonoBehaviour
{
    public static AudioMgr Instance;
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;
    public int level;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        //starts when the game starts. will likely change level == 0 to account for the main menu
        if (level == 0)
            PlayMusic("Library");
        else if (level == 1)
            PlayMusic("BookOne");
    }
    public void PlayMusic(string name)
    {
        //plays a given music clip from an array or throws an error message
        Sound s = Array.Find(musicSounds, x => x.name == name);
        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }
    public void PlaySFX(string name)
    {
        //plays a given sound clip from an array or throws an error message
        Sound s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    public void PlaySFX(string name, AudioSource source)
    {
        //theres definitely a reason that i made the playsfx function twice
        //i just dont know what it is...
        //i think its for alternative audio sources not exclusive to sfxsource
        //or like footsteps. 
        Sound s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }
        else
        {
            source.clip = s.clip;
            sfxSource.PlayOneShot(s.clip);
        }
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }
    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void sfxVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
