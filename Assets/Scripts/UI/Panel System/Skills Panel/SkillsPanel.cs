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

    [Header("Passive Collection Sub Panel")]
    public PassiveSkillPanel passiveCollectionPanel;

    [Header("Tutorial Overlay")]
    public GameObject tutorialOverlay;

    [Header("Other UI Bits")]
    public UnspentSkillPointIndicator unspentIndicator;

  

    private List<SkillEntry> knownSkillEntries = new List<SkillEntry>();
    private List<SkillEntry> classFeatureEntries = new List<SkillEntry>();

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
        AbilityUtilities.PopulateSkillEntryList(ref classFeatureEntries, skillEntryTemplate, classFeatureSkillHolder, SkillEntry.SkillEntryLocation.ClassFeatureSkill);

        //ShowTutorial();

        CheckForUnspentSkillPoints();
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
        return AbilityUtilities.GetSkillEntryByAbility(activeSkillEntries, ability);
    }

    public void OnPassiveSlotClicked(SkillEntry slot) {
        passiveCollectionPanel.OnSlotClicked(slot);
        //passiveCollectionPanel.selectedActiveEntry = slot;
        passiveCollectionPanel.Open();
    }

    public void OnKnownPassiveSelected(SkillEntry entry) {
        passiveCollectionPanel.selectedKnownEntry = entry;
        passiveCollectionPanel.OnKnownEntryClicked(entry);

        SetPassiveHighlights(entry);
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
        
        unspentIndicator.gameObject.SetActive(skillPoints > 0);
    }


    public void ShowInfoTooltip() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Drag unassigned Skills from the Known Skills section to the Active Skills section to assign them.");
        builder.AppendLine("You can also drag assigned skills around in the Active Skills Section to change their posiitons.");
        builder.AppendLine();
        builder.AppendLine("Right Click any Skill to edit that Skill's Runes.");
        builder.AppendLine();
        builder.AppendLine("Left Click an Empty Passive Skill Slot to assign a Passive from your Known Passives.");
        builder.AppendLine("Left Click on an assigned Passive Skill to unasign that Passive Skill.");


        TooltipManager.Show(builder.ToString(), "Skills Info");
    }

    public void HideInfoTooltip() {
        TooltipManager.Hide();
    }

}
