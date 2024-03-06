using System.Collections;
using System.Collections.Generic;
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

    private List<SkillEntry> knownSkillEntries = new List<SkillEntry>();
    private List<SkillEntry> classFeatureEntries = new List<SkillEntry>();

    private List<SkillEntry> activePassiveSkillEntries = new List<SkillEntry>();
    private List<SkillEntry> knownPassiveSkillEntries = new List<SkillEntry>();

    public override void Open() {
        base.Open();

        if(EntityManager.ActivePlayer == null) {
            return;
        }

        AbilityUtilities.PopulateSkillEntryList(ref knownSkillEntries, skillEntryTemplate, knownSkillsHolder, SkillEntry.SkillEntryLocation.KnownSkill);
        AbilityUtilities.PopulateSkillEntryList(ref knownPassiveSkillEntries, skillEntryTemplate, knownPassiveSkillsHolder, SkillEntry.SkillEntryLocation.KnownPassive);
        AbilityUtilities.PopulateSkillEntryList(ref classFeatureEntries, skillEntryTemplate, classFeatureSkillHolder, SkillEntry.SkillEntryLocation.ClassFeatureSkill);

    }

    public override void Close() {
        base.Close();
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

}
