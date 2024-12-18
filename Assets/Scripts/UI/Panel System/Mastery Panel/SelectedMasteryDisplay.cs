using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectedMasteryDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image icon;

    public MasteryFeatureEntry Feature { get; private set; }



    public void Setup(MasteryFeatureEntry data) {
        Feature = data;
        icon.sprite = data.FeatureData.featureAbility.AbilityData.abilityIcon;
    }


    public void OnPointerEnter(PointerEventData eventData) {
        Feature.ShowTooltip();
    }


    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
