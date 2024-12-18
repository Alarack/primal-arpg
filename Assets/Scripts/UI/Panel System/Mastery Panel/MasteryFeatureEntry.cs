using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using LL.Events;

public class MasteryFeatureEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    
    public Image featureIcon;
    public TextMeshProUGUI learnButtonText;
    public Image dimmer;
    public Image selectedFrame;

    public MasteryData.MasteryFeatureData FeatureData { get; private set; }

    public Ability FeatureAbility { get; private set; }

    public List<Ability> PathAbilities { get; private set; } = new List<Ability>();

    private MasteryEntry parentEntry;

    private Ability displayAbility;

    private List<MasteryPathEntry> pathEntries = new List<MasteryPathEntry>();

    public bool Selected { get; private set; }

    public void Setup(MasteryData.MasteryFeatureData data, MasteryEntry parentEntry) {
        this.parentEntry = parentEntry;
        FeatureData = data;

        FeatureAbility = EntityManager.ActivePlayer.AbilityManager.GetAbilityByName(FeatureData.featureAbility.AbilityData.abilityName, AbilityCategory.PassiveSkill);

        if (FeatureAbility == null) {
            displayAbility = FeatureData.featureAbility.FetchAbilityForDisplay(EntityManager.ActivePlayer);
        }


        SetupDisplay();
        SetDimmer();
        UpdateButtonText();
    }

    private void SetupDisplay() {
        featureIcon.sprite = FeatureData.featureAbility.AbilityData.abilityIcon;
    }

    private void SetDimmer() {
        if(FeatureAbility == null || FeatureAbility.IsEquipped == false) {
            dimmer.gameObject.SetActive(true);
        }

        if (FeatureAbility != null && FeatureAbility.IsEquipped == true) {
            dimmer.gameObject.SetActive(false);
        }
    }



    private void ShowFeaturePaths() {
        pathEntries.PopulateList(FeatureData.featurePathAbilities.Count, parentEntry.pathTemplate, parentEntry.pathHolder, true);
        for (int i = 0; i < pathEntries.Count; i++) {
            pathEntries[i].Setup(FeatureData.featurePathAbilities[i], this);
        }
    }


    public void Select() {
        Selected = true;
        selectedFrame.gameObject.SetActive(true);
        ShowFeaturePaths();
        UpdateButtonText();
    }

    public void Deselect() {
        Selected = false;
        selectedFrame.gameObject.SetActive(false);
        pathEntries.ClearList();
        UpdateButtonText();
        Unlearn();
    }

    public void OnLearnClicked() {
        if(FeatureAbility != null) {
            if(FeatureAbility.IsEquipped == true) {
                Unlearn();
                
                //FeatureAbility.Uneqeuip();
                //UnequipPathAbilities();
            }
            else {
                Learn();
                //FeatureAbility.Equip();
                //EquipPathAbilities();
            }
        }
        else {
            FeatureAbility = EntityManager.ActivePlayer.AbilityManager.LearnAbility(FeatureData.featureAbility.AbilityData, true);
            PanelManager.GetPanel<MasteryPanel>().TrackFeature(this);
        }

        SetDimmer();
        UpdateButtonText();
    }

    private void Learn() {
        FeatureAbility.Equip();
        EquipPathAbilities();

        PanelManager.GetPanel<MasteryPanel>().TrackFeature(this);
    }

    public void Unlearn() {
        if (FeatureAbility == null)
            return;

        FeatureAbility.Uneqeuip();
        UnequipPathAbilities();

        PanelManager.GetPanel<MasteryPanel>().RemoveFeature(this);

    }

    private void UnequipPathAbilities() {
        for (int i = 0; i < pathEntries.Count; i++) {
            pathEntries[i].OnParentFeatureUnequipped();
        }
    }

    private void EquipPathAbilities() {
        for (int i = 0; i < pathEntries.Count; i++) {
            pathEntries[i].OnParentFeatureEquipped();
        }
    }

    private void UpdateButtonText() {
        if (FeatureAbility != null) {
            if (FeatureAbility.IsEquipped == true) {
                learnButtonText.text = "UnSelect";
            }
            else {
                learnButtonText.text = "Select";
            }
        }
        else {
            learnButtonText.text = "Select";
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        
        if(eventData.button == PointerEventData.InputButton.Left) {
            parentEntry.OnFeatureSelected(this);
        }

        if(eventData.button == PointerEventData.InputButton.Right) {

            if (FeatureAbility == null || FeatureAbility.IsEquipped == false)
                return;
            
            PanelManager.OpenPanel<PopupPanel>().Setup("Unlearn Mastery", "Are you sure you want to unlearn: " + FeatureData.featureName, Unlearn);
        }

    }


    public void OnPointerEnter(PointerEventData eventData) {
        ShowTooltip();
    }

    public void ShowTooltip() {
        if (FeatureAbility != null)
            TooltipManager.Show(FeatureAbility.GetTooltip(), FeatureAbility.Data.abilityName);
        else
            TooltipManager.Show(displayAbility.GetTooltip(), displayAbility.Data.abilityName);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
