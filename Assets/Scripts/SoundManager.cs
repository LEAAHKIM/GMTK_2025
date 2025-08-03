using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager current;
    public static float SFXVolume = 1.0f;
    public static float musicVolume = 0.3f;
    public static float lastCharacterTalkTime = -1;
    public AudioSource SFXPlayer;
    public AudioSource musicPlayer;
    public float _lastSFXTime;
    public float _lastSFXLength;
    public AnimationCurve musicDecreaseWhenSFXCurve;
    public AudioClip nilaSong;
    public AudioClip creditsSong;
    private void Awake()
    {
        if (current == null)
        {
            current = this;
        }
    }

    private void Start()
    {
        //Debug.Log(SceneManager.GetActiveScene().name);
        if(SceneManager.GetActiveScene().name == "Credits" || SceneManager.GetActiveScene().name == "Home" || SceneManager.GetActiveScene().name == "Levels") { musicPlayer.clip = (LevelManager.gameStarted) ? creditsSong : nilaSong; }
        if (current != this) {
            if (current.musicPlayer.clip != musicPlayer.clip) { Debug.Log("a"); current.musicPlayer.clip = musicPlayer.clip; current.musicPlayer.Play(); }
            Destroy(gameObject);
            return; 
        }
        
        SFXPlayer.volume = SFXVolume;
        musicPlayer.volume = musicVolume;
        DontDestroyOnLoad(gameObject);
        musicPlayer.Play();
    }
    private void Update()
    {
        musicPlayer.volume = Mathf.Lerp(musicVolume / 1.5f, musicVolume, musicDecreaseWhenSFXCurve.Evaluate(Mathf.Clamp01((Time.time - _lastSFXTime) / _lastSFXLength)));
    }
    public void PlaySFXWithMusicMute(AudioClip sfx)
    {
        if (SFXVolume <= 0) { return; }
        SFXPlayer.PlayOneShot(sfx);
        _lastSFXTime = Time.time;
        _lastSFXLength = sfx.length;
    }
    public static void ChangeSFXVolume(float v)
    {
        SFXVolume = v;
        current.SFXPlayer.volume = v;
    }
    public static void ChangeMusicVolume(float v)
    {
        musicVolume = v;
        current.musicPlayer.volume = v;
    }

}