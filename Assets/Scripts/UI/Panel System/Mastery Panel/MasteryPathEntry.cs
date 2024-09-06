using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MasteryPathEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image pathIcon;
    public TextMeshProUGUI rankText;


    public AbilityDefinition PathAbilityDef { get; private set; }
    public Ability PathAbility { get; private set; }

    public Ability DisplayAbility {  get; private set; }

    private MasteryFeatureEntry parentFeature;

    public void Setup(AbilityDefinition abilityDef, MasteryFeatureEntry parentFeature) {
        this.parentFeature = parentFeature;
        this.PathAbilityDef = abilityDef;

        PathAbility = EntityManager.ActivePlayer.AbilityManager.GetAbilityByName(PathAbilityDef.AbilityData.abilityName, AbilityCategory.PassiveSkill);

        if(PathAbility == null) {
            DisplayAbility = abilityDef.FetchAbilityForDisplay(EntityManager.ActivePlayer);
        }

        SetupDisplay();
    }

    private void SetupDisplay() {
        pathIcon.sprite = PathAbilityDef.AbilityData.abilityIcon;

        rankText.text = PathAbility != null ? PathAbility.AbilityLevel.ToString() : "0";
    }

    private void Invest() {
        if(PathAbility == null) {
            PathAbility = EntityManager.ActivePlayer.AbilityManager.LearnAbility(PathAbilityDef.AbilityData, true);
        }
        else {
            if (PathAbility.IsEquipped == false)
                PathAbility.Equip();
            
            PathAbility.LevelUp();
        }

        rankText.text = PathAbility.AbilityLevel.ToString();
    }

    private void UnInvest() {
        if (PathAbility == null || PathAbility.IsEquipped == false)
            return;

        if(PathAbility.AbilityLevel >= 1)
            PathAbility.LevelDown();

        if (PathAbility.AbilityLevel <= 0)
            PathAbility.Uneqeuip();

        rankText.text = PathAbility.AbilityLevel.ToString();
    }

    public void OnPointerClick(PointerEventData eventData) {

        if (parentFeature.FeatureAbility == null || parentFeature.FeatureAbility.IsEquipped == false)
            return;

        switch (eventData.button) {
            case PointerEventData.InputButton.Left:
                Invest();
                break;
            case PointerEventData.InputButton.Right:
                UnInvest();
                break;
        }
    }


    public void OnParentFeatureEquipped() {
        if(PathAbility != null && PathAbility.AbilityLevel >= 1) { 
            PathAbility.Equip();
        }
    }

    public void OnParentFeatureUnequipped() {
        if(PathAbility != null) {
            PathAbility.Uneqeuip();
        }
    }


    public void OnPointerEnter(PointerEventData eventData) {

        if (PathAbility != null)
            TooltipManager.Show(PathAbility.GetTooltip(), PathAbility.Data.abilityName);
        else
            TooltipManager.Show(DisplayAbility.GetTooltip(), DisplayAbility.Data.abilityName);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
