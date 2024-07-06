using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

public class ButtonEntry : Button, IPointerEnterHandler {

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
    
    
    
    public override void OnPointerClick(PointerEventData eventData) {
        base.OnPointerClick(eventData);
        
        if (data != null && data.buttonDataType == ButtonData.ButtonDataType.Informational)
            return;

        clickCallback?.Invoke();
        if (parentPanel != null && closeParentPanelOnClick == true)
            parentPanel.Close();

        AudioManager.PlayButtonPressed();

    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);

        AudioManager.PlayButtonHover();
    }
}
