using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameButtonType = InputHelper.GameButtonType;

public class SkillsPanel : SkillBasePanel
{

    [Header("Template")]
    public Transform knownSkillsHolder;
   
    private List<SkillEntry> knownSkillEntries = new List<SkillEntry>();

  
    public override void Open() {
        base.Open();

        AbilityUtilities.PopulateSkillEntryList(ref knownSkillEntries, skillEntryTemplate, knownSkillsHolder, SkillEntry.SkillEntryLocation.KnownSkill);
    }

    public override void Close() {
        base.Close();
        TooltipManager.Hide();
    }

    protected override void CreateEmptySlots() {
        AbilityUtilities.CreateEmptySkillEntries(ref activeSkillEntries, 6, skillEntryTemplate, holder, SkillEntry.SkillEntryLocation.ActiveSkill, defaultKeybinds);
    }

    public SkillEntry IsAbilityInActiveList(Ability ability) {
        return AbilityUtilities.GetSkillEntryByAbility(activeSkillEntries, ability);
    }

}
