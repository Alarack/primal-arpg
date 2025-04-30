using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using static Unity.VisualScripting.Member;

public class AudioManager : Singleton<AudioManager>
{
    public AudioMixer audioMixer;
    public AudioSource SFXTemplate;

    public AudioSource sfxSource;
    public float minSoundInterval;


    public AudioClip basicButtonHover;
    public AudioClip basicButtonPressed;
    public AudioClip potionSound;
    public AudioClip forgeSound;
    public AudioClip forgeSelection;
    public AudioClip abilitySelection;
    public AudioClip abilityLevelUp;

    private Dictionary<AudioClip, List< AudioSource>> activeSources = new Dictionary<AudioClip, List<AudioSource>>();

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


    public static void PlayButtonHover() {
        PlaySoundClip(Instance.basicButtonHover, Instance.transform.position, 1f);
    }

    public static void PlayButtonPressed() {
        PlaySoundClip(Instance.basicButtonPressed, Instance.transform.position, 1f);
    }

    public static void PlayPotionSound() {
        PlaySoundClip(Instance.potionSound, Instance.transform.position, 1f);
    }

    public static void PlayForgeSound() {
        PlaySoundClip(Instance.forgeSound, Instance.transform.position, 0.15f);
    }

    public static void PlayForgeSelect() {
        PlaySoundClip(Instance.forgeSelection, Instance.transform.position, 1f);
    }

    public static void PlayAbilitySelect() {
        PlaySoundClip(Instance.abilitySelection, Instance.transform.position, 1f);
    }

    public static void PlayAbilityLevelUp() {
        PlaySoundClip(Instance.abilityLevelUp, Instance.transform.position, 1f);
    }

    public static void PlaySoundClip(AudioClip clip, Vector2 position, float volume, float pitchVariance = 1f) {

        if (Instance.activeSources.ContainsKey(clip)) {
            if (Instance.activeSources[clip][0].time < clip.length / 2f) {
                return;
            }
        }

        AudioSource activeAudio = Instantiate(Instance.SFXTemplate, Instance.transform);
        activeAudio.transform.localPosition = position;


        //Debug.Log("Playing: " + clip.name);

        TrackAudioClip(clip, activeAudio);

        
        activeAudio.clip = clip;

        //int countOfActiveClips = GetCountOfClip(clip);
        //float modifier = countOfActiveClips > 1 ? countOfActiveClips / 2 : 1;

        activeAudio.volume = volume;
        activeAudio.Play();

        new Task(Instance.ResolveSound(clip, activeAudio));

        //Destroy(activeAudio.gameObject, clip.length);

    }



    private IEnumerator ResolveSound(AudioClip clip, AudioSource source) {
        WaitForSeconds waiter = new WaitForSeconds( clip.length);

        yield return waiter;

        if(activeSources.ContainsKey(clip) == true) {
            //Debug.Log("Resolving: " + clip.name);

            activeSources.Remove(clip);
        }

        Destroy(source.gameObject);
    }

    private static int GetCountOfClip(AudioClip clip) {
        if (Instance.activeSources.TryGetValue(clip, out List<AudioSource> list)) {
            return list.Count;
        }

        return 0;
    }

    private static void TrackAudioClip(AudioClip clip, AudioSource source) {
        if(Instance.activeSources.TryGetValue(clip, out List<AudioSource> list)) {
            list.Add(source);
        }
        else {
            Instance.activeSources.Add(clip, new List<AudioSource> { source });
        }
    }

    public static void PlayRandomClip(List<AudioClip> clips, Vector2 position, float volume, float pitchVariance = 1f) {

        if (clips == null || clips.Count == 0)
            return;

        AudioClip randomClip = clips[Random.Range(0, clips.Count)];

        PlaySoundClip(randomClip, position, volume, pitchVariance);
    }

}
