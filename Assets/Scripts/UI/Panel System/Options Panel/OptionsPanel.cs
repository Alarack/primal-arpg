using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsPanel : BasePanel
{

    public TabManager tabmanager;

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


}
