using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusIndicatorEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


    public Image statusIconImage;
    public Image durationDimmer;
    public Status activeStatus;
    public TextMeshProUGUI stackCountText;



    public void Setup(Status status) {
        this.activeStatus = status;
        SetupDisplay();
    }

    private void Update() {
        UpdateDurationDimmer();
        
    }

    private void UpdateDurationDimmer() {
        if (activeStatus == null)
            return;

        if (activeStatus.TotalDuration <= 0f)
            return;

        //float durationInverse = 1 - activeStatus.DurationRatio;
        durationDimmer.fillAmount = activeStatus.DurationRatio;
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
        TooltipManager.Show(activeStatus.ParentEffect.GetTooltip(), activeStatus.ParentEffect.ParentAbility.Data.abilityName);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
