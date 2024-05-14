using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusIndicatorEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


    public Image statusIconImage;
    public Status activeStatus;



    public void Setup(Status status) {
        this.activeStatus = status;
        SetupDisplay();
    }
    
    
    
    private void SetupDisplay() {
        statusIconImage.sprite = activeStatus.Data.statusSprite;
    }
    
    
    
    
    
    
    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Show(activeStatus.ParentEffect.GetTooltip());
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
