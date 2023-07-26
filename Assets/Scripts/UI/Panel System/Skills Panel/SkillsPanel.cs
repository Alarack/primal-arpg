using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameButtonType = InputHelper.GameButtonType;

public class SkillsPanel : SkillBasePanel
{

    [Header("Template")]
    public Transform knownSkillsHolder;
    public Transform activePassiveSkillsHolder;
    public Transform knownPassiveSkillsHolder;

    [Header("Passive Collection Sub Panel")]
    public PassiveSkillPanel passiveCollectionPanel;

    private List<SkillEntry> knownSkillEntries = new List<SkillEntry>();
    
    private List<SkillEntry> activePassiveSkillEntries = new List<SkillEntry>();
    private List<SkillEntry> knownPassiveSkillEntries = new List<SkillEntry>();
  
    public override void Open() {
        base.Open();

        AbilityUtilities.PopulateSkillEntryList(ref knownSkillEntries, skillEntryTemplate, knownSkillsHolder, SkillEntry.SkillEntryLocation.KnownSkill);
        AbilityUtilities.PopulateSkillEntryList(ref knownPassiveSkillEntries, skillEntryTemplate, knownPassiveSkillsHolder, SkillEntry.SkillEntryLocation.Passive);
    }

    public override void Close() {
        base.Close();
        TooltipManager.Hide();
    }

    protected override void CreateEmptySlots() {
        AbilityUtilities.CreateEmptySkillEntries(ref activeSkillEntries, 6, skillEntryTemplate, holder, SkillEntry.SkillEntryLocation.ActiveSkill, defaultKeybinds);
        AbilityUtilities.CreateEmptyPassiveSkillEntries(ref activePassiveSkillEntries, 2, skillEntryTemplate, activePassiveSkillsHolder);
    }

    public SkillEntry IsAbilityInActiveList(Ability ability) {
        return AbilityUtilities.GetSkillEntryByAbility(activeSkillEntries, ability);
    }

    public void OnPassiveSlotClicked(SkillEntry slot) {
        passiveCollectionPanel.selectedActiveEntry = slot;
        passiveCollectionPanel.Open();
    }

    public void OnKnownPassiveSelected(SkillEntry entry) {
        for (int i = 0; i < knownPassiveSkillEntries.Count; i++) {
            if (knownPassiveSkillEntries[i] == entry) {
                knownPassiveSkillEntries[i].SelectPassive();
                passiveCollectionPanel.selectedKnownEntry = entry;
            }
            else {
                knownPassiveSkillEntries[i].DeselectPassive();
            }
        }
    }

}
