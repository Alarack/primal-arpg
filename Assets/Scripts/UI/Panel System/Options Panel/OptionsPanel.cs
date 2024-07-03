using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsPanel : BasePanel
{

    public TabManager tabmanager;

    [Header("Audio Stuff")]
    public Slider masterVolumeSlider;
    public Slider soundEffectsVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("Cursor Stuff")]
    public Slider cursorThicknessSlider;
    public TMP_Dropdown cursorModeDropdown;
    private CustomCursor customCursor;


    protected override void Awake()
    {
        base.Awake();

        customCursor = GameObject.FindObjectOfType<CustomCursor>();

        if(customCursor == null)
        {
            Debug.LogError("Could not locate Custom Cursor in scene.");
        }
    }

    public override void Open() {
        base.Open();

        tabmanager.OnTabSelected(tabmanager.selectedTab);


        float master = PlayerPrefs.GetFloat("masterVolume");
        float sfx = PlayerPrefs.GetFloat("soundEffectsVolume");
        float music = PlayerPrefs.GetFloat("musicVolume");

        if(master != 0f) 
            masterVolumeSlider.value = master;
        
        if(sfx != 0f)
            soundEffectsVolumeSlider.value = sfx;

        if(music != 0f)
            musicVolumeSlider.value = music;
    }

    #region Cursor Methods

    public void OnCursorThicknessChanged()
    {
        CursorEdgeThickness selectedThickness = (CursorEdgeThickness)cursorThicknessSlider.value;

        customCursor.ChangeCursorThickness(selectedThickness);

    }


    public void OnCursorModeChanged()
    {

        CursorMode selectedMode = (CursorMode)cursorModeDropdown.value;

        customCursor.ChangeCursorMode(selectedMode);

    }


    #endregion

    #region Audio Methods

    public void OnMasterVolumeChanged(float value) {
        AudioManager.SetMasterVolume(value);
    }

    public void OnSoundEffectsVolumeChanged(float value) {
        AudioManager.SetSoundEffectsVolume(value);
    }

    public void OnMusicVolumeChanged(float value) {
        AudioManager.SetMusicVolume(value);
    }


    #endregion
}
