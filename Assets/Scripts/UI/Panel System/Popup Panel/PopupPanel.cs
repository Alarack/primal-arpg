using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PopupPanel : BasePanel
{

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;

    public GameObject cancelButton;


    private Action confirmCallback;

    public void Setup(string title, string body, Action confirmCallback = null) {
        titleText.text = title;
        bodyText.text = body;
        this.confirmCallback = confirmCallback;

        cancelButton.SetActive(confirmCallback != null);
        
    }


    public void OnConfirmClicked() {
        confirmCallback?.Invoke();
        Close();
    }


}
