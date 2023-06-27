using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class ButtonEntry : MonoBehaviour, IPointerClickHandler {


    public TextMeshProUGUI buttonText;
    public bool closeParentPanelOnClick;

    private Action clickCallback;
    private BasePanel parentPanel;
    private ButtonData data;
    
    
    public void Initialize(BasePanel parentPanel, ButtonData buttonData) {
        this.parentPanel = parentPanel;
        this.data = buttonData;
        clickCallback = buttonData.callback;

        buttonText.text = buttonData.buttonText;
        buttonText.color = buttonData.buttonTextColor;
    }
    
    
    
    public void OnPointerClick(PointerEventData eventData) {

        if (data.buttonDataType == ButtonData.ButtonDataType.Informational)
            return;

        clickCallback?.Invoke();

        if (closeParentPanelOnClick == true)
            parentPanel.Close();

    }
}
