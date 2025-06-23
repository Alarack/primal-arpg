using LL.Events;
using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using GameButtonType = InputHelper.GameButtonType;

public class SkillsPanel : SkillBasePanel {

    [Header("Template")]
    public Transform knownSkillsHolder;
    public Transform activePassiveSkillsHolder;
    public Transform knownPassiveSkillsHolder;
    public Transform classFeatureSkillHolder;

    [Header("Sub Panels")]
    public PassiveSkillPanel passiveCollectionPanel;
    public KnownSkillsSubpanel knownSkillsSubpanel;

    [Header("Tutorial Overlay")]
    public GameObject tutorialOverlay;

    [Header("Other UI Bits")]
    public GameObject unspentSkillPointsHolder;
    public UnspentSkillPointIndicator unspentIndicator;
    public GameObject levelUpButton;

  

    private List<SkillEntry> knownSkillEntries = new List<SkillEntry>();
    //private List<SkillEntry> classFeatureEntries = new List<SkillEntry>();

    private List<SkillEntry> activePassiveSkillEntries = new List<SkillEntry>();
    private List<SkillEntry> knownPassiveSkillEntries = new List<SkillEntry>();


    public override void Open() {
        base.Open();

        if(EntityManager.ActivePlayer == null) {
            return;
        }

        PanelManager.ClosePanel<InventoryPanel>();

        AbilityUtilities.PopulateSkillEntryList(ref knownSkillEntries, skillEntryTemplate, knownSkillsHolder, SkillEntry.SkillEntryLocation.KnownSkill);
        AbilityUtilities.PopulateSkillEntryList(ref knownPassiveSkillEntries, skillEntryTemplate, knownPassiveSkillsHolder, SkillEntry.SkillEntryLocation.KnownPassive);
        //AbilityUtilities.PopulateSkillEntryList(ref classFeatureEntries, skillEntryTemplate, classFeatureSkillHolder, SkillEntry.SkillEntryLocation.ClassFeatureSkill);

        //ShowTutorial();

        CheckForUnspentSkillPoints();
        UpdateLevelupButton();
    }

    public override void Show() {
        base.Show();

        TooltipManager.Hide();
    }

    protected override void OnEnable() {
        base.OnEnable();

        EventManager.RegisterListener(GameEvent.AbilityUnequipped, OnPassiveAbilityUnequippped);
    }

    protected override void OnDisable() {
        base.OnDisable();

        EventManager.RemoveMyListeners(this);
    }

    public override void Close() {
        base.Close();
        passiveCollectionPanel.Close();
        knownSkillsSubpanel.Close();
        TooltipManager.Hide();
    }

    protected override void OnFadeOutComplete() {
        base.OnFadeOutComplete();
        TooltipManager.Hide();
    }

    protected override void CreateEmptySlots() {
        AbilityUtilities.CreateEmptySkillEntries(ref activeSkillEntries, 6, skillEntryTemplate, holder, SkillEntry.SkillEntryLocation.ActiveSkill, defaultKeybinds);
        AbilityUtilities.CreateEmptyPassiveSkillEntries(ref activePassiveSkillEntries, 4, skillEntryTemplate, activePassiveSkillsHolder);
    }

    public SkillEntry IsAbilityInActiveList(Ability ability) {
        if(ability == null) 
            return null;

        return AbilityUtilities.GetSkillEntryByAbility(activeSkillEntries, ability);
    }

    public SkillEntry IsPassiveAbilityInActiveList(Ability ability) {
        if (ability == null)
            return null;

        return AbilityUtilities.GetSkillEntryByAbility(activePassiveSkillEntries, ability);
    }

    public List<SkillEntry> GetActiveSkillEntries() {
        return activeSkillEntries;
    }

    public List<SkillEntry> GetActivePassiveEntries() {
        return activePassiveSkillEntries;
    }

    public void OnActiveSlotSelected(SkillEntry entry) {
        knownSkillsSubpanel.Open();
        knownSkillsSubpanel.Setup(entry, knownSkillEntries, this);   
    }

    public void OnKnownSkillSelected(SkillEntry entry) {
        knownSkillsSubpanel.AssignKnownSkill(entry);
    }


    public void OnPassiveSlotSelected(SkillEntry entry) {
        passiveCollectionPanel.Open();
        passiveCollectionPanel.Setup(entry, knownPassiveSkillEntries, this);
    }

    //public void OnPassiveSlotClicked(SkillEntry slot) {
    //    passiveCollectionPanel.OnSlotClicked(slot);
    //    //passiveCollectionPanel.selectedActiveEntry = slot;
    //    passiveCollectionPanel.Open();
    //}

    public void OnKnownPassiveSelected(SkillEntry entry) {
        passiveCollectionPanel.AssignKnownPassiveSkill(entry);
        //passiveCollectionPanel.selectedKnownEntry = entry;
        //passiveCollectionPanel.OnKnownEntryClicked(entry);

        //SetPassiveHighlights(entry);
    }

    public int GetFirstEmptyPassiveSlot() {
        for (int i = 0; i < activePassiveSkillEntries.Count; i++) {
            if (activePassiveSkillEntries[i].Ability == null) {
                return i;
            }
        }

        return -1;
    }

    public override SkillEntry GetSkillEntryByAbility(Ability ability) {

        if (ability.Data.category == AbilityCategory.KnownSkill) {
            for (int i = 0; i < activeSkillEntries.Count; i++) {
                if (activeSkillEntries[i].Ability == ability) {
                    return activeSkillEntries[i];
                }
            }
        }


        if (ability.Data.category == AbilityCategory.PassiveSkill) {
            for (int i = 0; i < activePassiveSkillEntries.Count; i++) {
                if (activePassiveSkillEntries[i].Ability == ability) {
                    return activePassiveSkillEntries[i];
                }
            }
        }


        return null;
    }

    public void UnequipAllPassives() {

    }

    public void AutoEquipPassiveToFirstEmptySlot(Ability ability) {

        if (ability.Data.tags.Contains(AbilityTag.Mastery))
            return;

        if (IsPassiveAbilityInActiveList(ability) == true) {
            Debug.LogWarning("A passive skill: " + ability.Data.abilityName + " is already on the active passive bar");
            return;
        }

        
        int firstEmpty = GetFirstEmptyPassiveSlot();

        if(firstEmpty > -1) {
            activePassiveSkillEntries[firstEmpty].AssignNewAbility(ability);
            ability.Equip();
        }
    }

    public void SetPassiveHighlights(SkillEntry entry) {
        for (int i = 0; i < knownPassiveSkillEntries.Count; i++) {
            if (knownPassiveSkillEntries[i] == entry) {
                knownPassiveSkillEntries[i].SelectPassive();
                //passiveCollectionPanel.selectedKnownEntry = entry;
            }
            else {
                knownPassiveSkillEntries[i].DeselectPassive();
            }
        }
    }

    public void ClearHighlights() {
        for (int i = 0; i < knownPassiveSkillEntries.Count; i++) {
            knownPassiveSkillEntries[i].DeselectPassive();
        }
    }

    public SkillEntry GetMatchingActiveSlot(SkillEntry activeSlot) {
       
        if(activeSlot.Ability == null)
            return null;    
        
        for (int i = 0; i < knownPassiveSkillEntries.Count; i++) {
            if (knownPassiveSkillEntries[i].Ability == null)
                continue;
            
            if (knownPassiveSkillEntries[i].Ability == activeSlot.Ability)
                return knownPassiveSkillEntries[i];
        }

        return null;
    }



    private void ShowTutorial() {
        int show = PlayerPrefs.GetInt("ShowSkillTutorial");

        if (show == 0) {
            tutorialOverlay.SetActive(true);
        }
    }

    public void OnTutoralOkayClicked() {
        PlayerPrefs.SetInt("ShowSkillTutorial", 1);
        tutorialOverlay.SetActive(false);
    }


    protected virtual void OnPassiveAbilityUnequippped(EventData data) {
        Ability ability = data.GetAbility("Ability");

        if(ability.Data.category != AbilityCategory.PassiveSkill) {
            return;
        }

        for (int i = 0; i < activePassiveSkillEntries.Count; i++) {
            if (activePassiveSkillEntries[i].Ability == ability) {
                activePassiveSkillEntries[i].AssignNewAbility(null);
            }
        }
    }

    public void CheckForUnspentSkillPoints() {
        float skillPoints = EntityManager.ActivePlayer.Stats[StatName.SkillPoint];
        
        unspentSkillPointsHolder.SetActive(skillPoints > 0);

        if (unspentSkillPointsHolder.activeSelf) {
            unspentIndicator.UpdateSkillPoints(skillPoints);
        }
    }


    public void ShowInfoTooltip() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Here you can view all your currently assigned Skills.");
        builder.AppendLine();
        builder.AppendLine("Left click any Skill, or an empty Skill Slot, to assign a Skill to that slot.");
        builder.AppendLine();
        builder.AppendLine("You can drag assigned Active Skills around in the Active Skills Section to change their positions.");
        builder.AppendLine();
        builder.AppendLine("Right click any Skill to customize that Skill's Runes.");

        TooltipManager.Show(builder.ToString(), "Skills Info");
    }

    public void HideInfoTooltip() {
        TooltipManager.Hide();
    }

    public void OnLevelUpClicked() {
        PanelManager.OpenPanel<LevelUpPanel>();
    }

    public void UpdateLevelupButton() {
        if (EntityManager.ActivePlayer.levelsStored > 0) {
            levelUpButton.SetActive(true);
        }
        else {
            levelUpButton.SetActive(false);
        }
    }

}
