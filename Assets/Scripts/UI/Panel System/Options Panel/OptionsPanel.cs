using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

    [Header("Enemy speed")]
    public Slider enemySpeedSlider;
    public TextMeshProUGUI enemySpeedText;


    [Header("Gameplay Stuff")]
    public Toggle confirmSkillInvestmentToggle;
    public Toggle healOnRoomEndToggle;

    public ColorSwatchHelper edgeColorOneSwatch;
    public ColorSwatchHelper edgeColorTwoSwatch;

    private ColorPickerSubPanel colorPickerSubPanel;



    //[HideInInspector]
    private ColorSwatchHelper selectedSwatch;

    protected override void Awake()
    {
        base.Awake();

        colorPickerSubPanel = GetComponentInChildren<ColorPickerSubPanel>();

        customCursor = FindFirstObjectByType<CustomCursor>();

        if(customCursor == null)
        {
            Debug.LogError("Could not locate Custom Cursor in scene.");
        }
    }

    protected override void Start()
    {
        base.Start();

        SetupDefaultCursorEdgeColors();
    }


    public override void Open() {
        base.Open();

        tabmanager.OnTabSelected(tabmanager.selectedTab);


        float master = PlayerPrefs.GetFloat("masterVolume");
        float sfx = PlayerPrefs.GetFloat("soundEffectsVolume");
        float music = PlayerPrefs.GetFloat("musicVolume");
        float enemySpeed = PlayerPrefs.GetFloat("EnemySpeed");

        if (master != 0f) 
            masterVolumeSlider.value = master;
        
        if(sfx != 0f)
            soundEffectsVolumeSlider.value = sfx;

        if(music != 0f)
            musicVolumeSlider.value = music;

        if(enemySpeed != 0f) {
            enemySpeedSlider.value = enemySpeed;
            enemySpeedText.text = "Enemy Speed: " + (MathF.Round(enemySpeedSlider.value * 100f, 0)) + "%";
        }

        int confirmSkillInvestment = PlayerPrefs.GetInt("ConfirmSkillInvest");

        confirmSkillInvestmentToggle.isOn = confirmSkillInvestment == 0;

        int healOnRoomEnd = PlayerPrefs.GetInt("HealOnRoomEnd");

        healOnRoomEndToggle.isOn = healOnRoomEnd == 1;
    }

    private void SetupDefaultCursorEdgeColors()
    {
        Color defaultEdgeColorOne;
        float defaultRed = PlayerPrefs.GetFloat("Default Cursor Edge One Red Value");
        float defaultGreen = PlayerPrefs.GetFloat("Default Cursor Edge One Green Value");
        float defaultBlue = PlayerPrefs.GetFloat("Default Cursor Edge One Blue Value");

        if (defaultRed == 0 && defaultGreen == 0 && defaultBlue == 0)
        {
            defaultEdgeColorOne = Color.white;
        }
        else
        {
            defaultEdgeColorOne = new Color(defaultRed, defaultGreen, defaultBlue, 1f);
        }

        CustomCursor.Instance.ChangeEdgeColorOne(defaultEdgeColorOne);
        edgeColorOneSwatch.swatchImage.color = defaultEdgeColorOne;

        Color defaultEdgeColorTwo;
        float defaultEnemyRed = PlayerPrefs.GetFloat("Default Cursor Edge Two Red Value");
        float defaultEnemyGreen = PlayerPrefs.GetFloat("Default Cursor Edge Two Green Value");
        float defaultEnemyBlue = PlayerPrefs.GetFloat("Default Cursor Edge Two Blue Value");

        if (defaultEnemyRed == 0 && defaultEnemyGreen == 0 && defaultEnemyBlue == 0)
        {
            defaultEdgeColorTwo = Color.blue;
        }
        else
        {
            defaultEdgeColorTwo = new Color(defaultEnemyRed, defaultEnemyGreen, defaultEnemyBlue, 1f);
        }

        CustomCursor.Instance.ChangeEdgeColorTwo(defaultEdgeColorTwo);
        edgeColorTwoSwatch.swatchImage.color = defaultEdgeColorTwo;

    }

    #region UI Callbacks

    public void OnEdgeSwatchClicked(ColorSwatchHelper swatchHelper)
    {
        selectedSwatch = swatchHelper;
        colorPickerSubPanel.Open();
    }

    #endregion

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

    public void OnColorChosen(Color targetColor)
    {

        colorPickerSubPanel.Close();
        selectedSwatch.OnColorSelected(targetColor);

        switch (selectedSwatch.colorSwatchDesignation)
        {
            case ColorSwatchHelper.ColorSwatchDesignation.EdgeOne:

                PlayerPrefs.SetFloat("Default Cursor Edge One Red Value", targetColor.r);
                PlayerPrefs.SetFloat("Default Cursor Edge One Green Value", targetColor.g);
                PlayerPrefs.SetFloat("Default Cursor Edge One Blue Value", targetColor.b);
                customCursor.ChangeEdgeColorOne(targetColor);
                break;
            case ColorSwatchHelper.ColorSwatchDesignation.EdgeTwo:

                PlayerPrefs.SetFloat("Default Cursor Edge Two Red Value", targetColor.r);
                PlayerPrefs.SetFloat("Default Cursor Edge Two Green Value", targetColor.g);
                PlayerPrefs.SetFloat("Default Cursor Edge Two Blue Value", targetColor.b);
                customCursor.ChangeEdgeColorTwo(targetColor);
                break;
        }

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

    #region Game Speed Methods

    public void OnGameSpeedChanged() {
        PlayerPrefs.SetFloat("EnemySpeed", enemySpeedSlider.value);
        enemySpeedText.text = "Enemy Speed: " + (MathF.Round(enemySpeedSlider.value * 100f, 0)) + "%";
    }

    #endregion

    #region Data Methods

    public void OnDeleteSaveClicked() {
        PanelManager.OpenPanel<PopupPanel>().Setup("Delete Save", "Are you sure you want to Delete ALL Save Data?", ConfirmDeleteSavedata);
    }

    private void ConfirmDeleteSavedata() {
        SaveLoadUtility.ResetSaveData();
        PlayerPrefs.DeleteAll();
    }

    #endregion

    #region Gameplay Methods

    public void OnConfirmSkillInvestmentToggle() {
        PlayerPrefs.SetInt("ConfirmSkillInvest", confirmSkillInvestmentToggle.isOn == true ? 0 : 1);
    }

    public void OnHealOrbsToggle() {
        PlayerPrefs.SetInt("HealOnRoomEnd", healOnRoomEndToggle.isOn ? 1 : 0);
    }

    #endregion
}
