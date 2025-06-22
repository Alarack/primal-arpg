using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Michsky.MUIP;
using UnityEngine.Events;

public class PopupPanel : BasePanel {

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;

    public GameObject cancelButton;
    public ModalWindowManager modalWindoManager;


    private Action confirmCallback;

    public void Setup(string title, string body, Action confirmCallback = null) {
        titleText.text = title;
        bodyText.text = body;
        this.confirmCallback = confirmCallback;

        cancelButton.SetActive(confirmCallback != null);


        
        modalWindoManager.Open();
        modalWindoManager.titleText = title;
        modalWindoManager.descriptionText = body;
        modalWindoManager.UpdateUI();
        modalWindoManager.confirmButton.onClick.AddListener(OnConfirmClicked);
        modalWindoManager.cancelButton.gameObject.SetActive(confirmCallback != null);



    }

    protected override void Update() {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Escape) && IsOpen == true) {
            OnConfirmClicked();
        }
    }

    public override void Close() {
        base.Close();
        modalWindoManager.confirmButton.onClick.RemoveAllListeners();
    }


    public void OnConfirmClicked() {
        confirmCallback?.Invoke();
        Close();
    }


}
