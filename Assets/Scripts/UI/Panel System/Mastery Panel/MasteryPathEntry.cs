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

        string text = "";
        if(PathAbility != null) {
            text = PathAbility.Data.maxRanks > 0 ? PathAbility.AbilityLevel.ToString() + "/" + PathAbility.Data.maxRanks.ToString() : PathAbility.AbilityLevel.ToString();
            rankText.color = PathAbility.IsMaxRank() ? Color.green : Color.white;
        }
        else {
            text = "0/" + PathAbilityDef.AbilityData.maxRanks;
            rankText.color = Color.white;
        }


        rankText.text = text;

        //rankText.text = PathAbility != null ? PathAbility.AbilityLevel.ToString() : "0";
        
        //if(PathAbility != null) {
        //    rankText.color = PathAbility.IsMaxRank() ? Color.green : Color.white;
        //}
        //else {
        //    rankText.color = Color.white;
        //}
    }

    private void Invest() {
        if(PathAbility == null) {
            PathAbility = EntityManager.ActivePlayer.AbilityManager.LearnAbility(PathAbilityDef.AbilityData, true);
        }
        else {
            if (PathAbility.IsEquipped == false)
                PathAbility.Equip();
            
            if(PathAbility.IsMaxRank() == true) {
                return;
            }

            PathAbility.LevelUp();
        }

        UpdateRankText();
        //rankText.text = PathAbility.AbilityLevel.ToString();
    }

    private void UnInvest() {
        if (PathAbility == null || PathAbility.IsEquipped == false)
            return;

        if(PathAbility.AbilityLevel >= 1)
            PathAbility.LevelDown();

        if (PathAbility.AbilityLevel <= 0)
            PathAbility.Uneqeuip();

        UpdateRankText();
        //rankText.text = PathAbility.AbilityLevel.ToString();
    }

    private void UpdateRankText() {
        
        string text = PathAbility.Data.maxRanks > 0 ? PathAbility.AbilityLevel.ToString() + "/" + PathAbility.Data.maxRanks.ToString() : PathAbility.AbilityLevel.ToString();


        rankText.text = text;
        Color textColor = PathAbility.IsMaxRank() ? Color.green : Color.white;
        rankText.color = textColor;
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

        TooltipManager.Show(PathAbility.GetTooltip(), PathAbility.Data.abilityName);
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
