using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using LL.Events;
using Michsky.MUIP;
using DG.Tweening;

public class MasteryFeatureEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    
    public Image featureIcon;
    //public TextMeshProUGUI learnButtonText;
    public ButtonManager masteryLearnButton;
    public Image dimmer;
    public Image selectedFrame;
    public CanvasGroup dimmerFader;
    public Sprite defaultButtonImage;
    public Sprite unlearnButtonImage;
    public Color unlearnButtonTextColor;

    public MasteryData.MasteryFeatureData FeatureData { get; private set; }
    public string ParentMasteryName { get { return parentEntry.MasteryName; } }

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

    //public void LoadFeature() {

    //    FeatureAbility = EntityManager.ActivePlayer.AbilityManager.GetAbilityByName(FeatureData.featureAbility.AbilityData.abilityName, AbilityCategory.PassiveSkill);

    //    if (FeatureAbility == null) {
    //        Debug.LogError("Couldn't find: " + FeatureData.featureAbility.AbilityData.abilityName + " on player, learning it fresh. Level will be wrong");
    //        FeatureAbility = EntityManager.ActivePlayer.AbilityManager.CreateAndLearnAbility(FeatureData.featureAbility.AbilityData, true);
    //    }

    //    SetDimmer();
    //    UpdateButtonText();
    //}

    private void SetupDisplay() {
        featureIcon.sprite = FeatureData.featureAbility.AbilityData.abilityIcon;
    }

    private void SetDimmer() {
        if(FeatureAbility == null || FeatureAbility.IsEquipped == false) {
            dimmer.gameObject.SetActive(true);
            dimmerFader.DOFade(1, 0.5f);
        }

        if (FeatureAbility != null && FeatureAbility.IsEquipped == true) {
            dimmer.gameObject.SetActive(false);
            dimmerFader.DOFade(0, 0.5f);
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
        //Unlearn();
    }


    //public void LearnOrUnlearn() {
    //    if(FeatureAbility == null)
    //        CreateFeatureAbility();

    //    if (FeatureAbility.IsEquipped) 
    //        Unlearn();
    //    else
    //        Learn();
    //}


    //public void OnLearnClicked() {
    //    if(FeatureAbility != null) {
    //        if(FeatureAbility.IsEquipped == true) {
    //            Unlearn();
                
    //            //FeatureAbility.Uneqeuip();
    //            //UnequipPathAbilities();
    //        }
    //        else {
    //            Learn();
    //            //FeatureAbility.Equip();
    //            //EquipPathAbilities();
    //        }
    //    }
    //    else {

    //        if (SaveLoadUtility.SaveData.CountOfMasteries >= 2) {
    //            PanelManager.OpenPanel<PopupPanel>().Setup("Maximum Masteries", "You can only have 2 Masteries at a time. Right Click one of your existing Masteries to unlearn it.");
    //            return;
    //        }


    //        FeatureAbility = EntityManager.ActivePlayer.AbilityManager.LearnAbility(FeatureData.featureAbility.AbilityData, true);
    //        SaveLoadUtility.SaveData.AddMastery(ParentMasteryName, FeatureData.featureName, FeatureAbility.Data.abilityName);
    //        SaveLoadUtility.SavePlayerData();
    //        PanelManager.GetPanel<MasteryPanel>().TrackFeature(this);
    //    }

    //    //SetDimmer();
    //    //UpdateButtonText();
    //}

    private void CreateFeatureAbility() {
        FeatureAbility = EntityManager.ActivePlayer.AbilityManager.CreateAndLearnAbility(FeatureData.featureAbility.AbilityData, false);
    }

    public void Learn() {

        if (FeatureAbility == null)
            CreateFeatureAbility();

        FeatureAbility.Equip();
        //EquipPathAbilities();

        SaveLoadUtility.SaveData.AddMastery(ParentMasteryName, FeatureData.featureName, FeatureAbility.Data.abilityName);
        SaveLoadUtility.SavePlayerData();

        //PanelManager.GetPanel<MasteryPanel>().TrackFeature(this);

        SetDimmer();
        UpdateButtonText();
    }

    public void Unlearn() {
        if (FeatureAbility == null)
            return;

        FeatureAbility.Uneqeuip();
        //UnequipPathAbilities();
        FullyUnlearnPathAbilities();

        //SaveLoadUtility.SaveData.RemoveMasteryPath(ParentMasteryName, FeatureData.featureName, FeatureAbility.Data.abilityName);
        SaveLoadUtility.SaveData.RemoveMasteryFeature(ParentMasteryName,FeatureData.featureName);
        SaveLoadUtility.SavePlayerData();

        //PanelManager.GetPanel<MasteryPanel>().RemoveFeature(this);


        SetDimmer();
        UpdateButtonText();

    }

    private void FullyUnlearnPathAbilities() {
        for (int i = 0; i < pathEntries.Count; i++) {
            pathEntries[i].UnlearnCompletely();
        }
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
                masteryLearnButton.SetText("Unlearn");
                masteryLearnButton.normalText.enableVertexGradient = false;
                masteryLearnButton.normalText.color = unlearnButtonTextColor;
                masteryLearnButton.normalBackgroundImage.sprite = unlearnButtonImage;
            }
            else {
                masteryLearnButton.SetText("Learn");
                masteryLearnButton.normalBackgroundImage.sprite = defaultButtonImage;
                masteryLearnButton.normalText.enableVertexGradient = true;
            }
        }
        else {
            masteryLearnButton.SetText("Learn");
            masteryLearnButton.normalBackgroundImage.sprite = defaultButtonImage;
            masteryLearnButton.normalText.enableVertexGradient = true;
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
