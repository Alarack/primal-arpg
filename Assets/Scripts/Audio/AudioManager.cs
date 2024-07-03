using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public AudioMixer audioMixer;
    public AudioSource SFXTemplate;



    private void Start() {
        SetMasterVolume(PlayerPrefs.GetFloat("masterVolume"));
        SetSoundEffectsVolume(PlayerPrefs.GetFloat("soundEffectsVolume"));
        SetMusicVolume(PlayerPrefs.GetFloat("musicVolume"));
    }

    public static void SetMasterVolume(float value) {

        if (value <= 0)
            return;

        PlayerPrefs.SetFloat("masterVolume", value);

        Instance.audioMixer.SetFloat("masterVolume", Mathf.Log10(value) * 20f);
    }

    public static void SetSoundEffectsVolume(float value) {

        if (value <= 0)
            return;

        PlayerPrefs.SetFloat("soundEffectsVolume", value);

        Instance.audioMixer.SetFloat("soundEffectsVolume", Mathf.Log10(value) * 20f);
    }

    public static void SetMusicVolume(float value) {

        if (value <= 0)
            return;

        PlayerPrefs.SetFloat("musicVolume", value);

        Instance.audioMixer.SetFloat("musicVolume", Mathf.Log10(value) * 20f);
    }


    public static void PlaySoundClip(AudioClip clip, Vector2 position, float volume, float pitchVariance = 1f) {

        AudioSource activeAudio = Instantiate(Instance.SFXTemplate, Instance.transform, true);

        activeAudio.clip = clip;
        activeAudio.volume = volume;
        activeAudio.Play();

        Destroy(activeAudio.gameObject, clip.length);

    }

    public static void PlayRandomClip(List<AudioClip> clips, Vector2 position, float volume, float pitchVariance = 1f) {

        if (clips == null || clips.Count == 0)
            return;

        AudioClip randomClip = clips[Random.Range(0, clips.Count)];

        PlaySoundClip(randomClip, position, volume, pitchVariance);
    }

}
