using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusIndicatorEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


    public Image statusIconImage;
    public Status activeStatus;
    public TextMeshProUGUI stackCountText;



    public void Setup(Status status) {
        this.activeStatus = status;
        SetupDisplay();
    }
    
    
    
    private void SetupDisplay() {
        statusIconImage.sprite = activeStatus.Data.statusSprite;

        if(activeStatus.Data.stackMethod == Status.StackMethod.None) {
            stackCountText.gameObject.SetActive(false);
        }
        else {
            stackCountText.gameObject.SetActive(true); 
            UpdateStackCount();
        }
    }
    
    
    public void UpdateStackCount() {
        stackCountText.text = activeStatus.StackCount.ToString();
    }
    
    
    
    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Show(activeStatus.ParentEffect.GetTooltip());
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
