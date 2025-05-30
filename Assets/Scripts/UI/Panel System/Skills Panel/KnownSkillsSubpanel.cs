using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static SkillEntry;
using static UnityEngine.Rendering.DebugUI;

public class KnownSkillsSubpanel : BasePanel {


    private SkillEntry selectedEntry;
    private SkillsPanel skillsPanel;

    private List<SkillEntry> knownSkillEntries = new List<SkillEntry>();

    public void Setup(SkillEntry entry, List<SkillEntry> knownSkillEntries, SkillsPanel skillsPanel) {
        selectedEntry = entry;
        this.skillsPanel = skillsPanel;
        this.knownSkillEntries = knownSkillEntries;

        SetupDisplay();
    }

    public override void Close() {
        base.Close();

        selectedEntry = null;
        TooltipManager.Hide();
    }


    private void SetupDisplay() {

        //if (selectedEntry.Ability != null) {
        //    selectedEntry.SelectActive();
        //}
        
        for (int i = 0; i < knownSkillEntries.Count; i++) {
            SkillEntry alreadyActive = skillsPanel.IsAbilityInActiveList(knownSkillEntries[i].Ability);


            Action target = alreadyActive != null ? knownSkillEntries[i].GrayIcon : knownSkillEntries[i].ColorIcon;

            target?.Invoke();

            //knownSkillEntries[i].alreadySelectedDimmer.gameObject.SetActive(alreadyActive != null);
        }
    }
    public void AssignKnownSkill(SkillEntry entry) {

        SkillEntry existingSkill = skillsPanel.IsAbilityInActiveList(selectedEntry.Ability);

        if (existingSkill != null) { //Skill slot is already filled

            //Debug.Log("Unassigning: " + existingSkill.Ability.Data.abilityName + " from slot " + existingSkill.Index);

            EntityManager.ActivePlayer.AbilityManager.UnequipAbility(existingSkill.Ability, existingSkill.Index);
        }
        
        EntityManager.ActivePlayer.AbilityManager.EquipAbility(entry.Ability, selectedEntry.Index);

        //Debug.Log("Assigning: " + entry.Ability.Data.abilityName + " to slot " + selectedEntry.Index);

        Close();
    }

    public void ShowInfoTooltip() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Here you can view your Active Skills collection.");
        builder.AppendLine();
        builder.AppendLine("Grayed out Skills are already assigned on your Hotbar.");
        builder.AppendLine();
        builder.AppendLine("Click an unassigned Skill to assign it.");

        TooltipManager.Show(builder.ToString(), "Known Active Skills Info");
    }

    public void HideInfoTooltip() {
        TooltipManager.Hide();
    }
}


