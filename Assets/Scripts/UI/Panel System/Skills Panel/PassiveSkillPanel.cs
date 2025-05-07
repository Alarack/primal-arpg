using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.MUIP;
using System;

public class PassiveSkillPanel : BasePanel
{

    public SkillEntry selectedActiveEntry;
    public SkillEntry selectedKnownEntry;

    public TextMeshProUGUI buttonText;

    public ButtonManager equipButton;

    public SkillsPanel skillPanel;

    private SkillEntry selectedEntry;
    private SkillsPanel skillsPanel;

    private List<SkillEntry> knownPassiveSkillEntries = new List<SkillEntry>();


    public override void Open() {
        base.Open();

        //skillPanel.ClearHighlights();

        //SkillEntry knownLastClicked = skillPanel.GetMatchingActiveSlot(selectedActiveEntry);

        //if (knownLastClicked != null) {
        //    knownLastClicked.SelectPassive();
        //    selectedKnownEntry = knownLastClicked;
        //    OnKnownEntryClicked(knownLastClicked);
        //}
    }

    public override void Close() {
        base.Close();

        selectedActiveEntry = null;
        selectedKnownEntry = null;
        selectedEntry = null;

        TooltipManager.Hide();
    }

    public void Setup(SkillEntry entry, List<SkillEntry> knownPassiveSkillEntries, SkillsPanel skillsPanel) {
        this.selectedEntry = entry;
        this.skillsPanel = skillsPanel;
        this.knownPassiveSkillEntries = knownPassiveSkillEntries;

        SetupDisplay();

    }


    private void SetupDisplay() {
        for (int i = 0; i < knownPassiveSkillEntries.Count; i++) {
            SkillEntry alreadyActive = skillsPanel.IsPassiveAbilityInActiveList(knownPassiveSkillEntries[i].Ability);

            //knownPassiveSkillEntries[i].alreadySelectedDimmer.gameObject.SetActive(alreadyActive != null);


            Action target = alreadyActive != null ? knownPassiveSkillEntries[i].GrayIcon : knownPassiveSkillEntries[i].ColorIcon;

            target?.Invoke();

        }
    }



    public void AssignKnownPassiveSkill(SkillEntry entry) {

        SkillEntry existingSkill = skillsPanel.IsPassiveAbilityInActiveList(selectedEntry.Ability);

        if (existingSkill != null) { //Skill slot is already filled

            //Debug.Log("Unassigning: " + existingSkill.Ability.Data.abilityName + " from slot " + existingSkill.Index);
            //EntityManager.ActivePlayer.AbilityManager.UnequipAbility(existingSkill.Ability, existingSkill.Index);

            existingSkill.Ability.Uneqeuip();
            selectedEntry.AssignNewAbility(null);
        }

        //EntityManager.ActivePlayer.AbilityManager.EquipAbility(entry.Ability, selectedEntry.Index);
        //Debug.Log("Assigning: " + entry.Ability.Data.abilityName + " to slot " + selectedEntry.Index);

        selectedEntry.AssignNewAbility(entry.Ability);
        selectedEntry.Ability.Equip();

        Close();
    }


    public void OnSlotClicked(SkillEntry slot) {
        selectedActiveEntry = slot;

        if(selectedActiveEntry.Ability != null) {
            buttonText.text = "Unequip";
            equipButton.SetText("Unequip");
            //skillPanel.SetPassiveHighlights(slot)
        }
        else {
            buttonText.text = "Equip";
            equipButton.SetText("Equip");

        }
    }

    public void OnKnownEntryClicked(SkillEntry knownEntry) {
        //Debug.Log(knownEntry.Ability.Data.abilityName + " is being passed in");
        //Debug.Log(selectedKnownEntry.Ability.Data.abilityName + " is currently selected");

        if (selectedActiveEntry.Ability != null && selectedActiveEntry.Ability == knownEntry.Ability) {
            buttonText.text = "Unequip";
            equipButton.SetText("Unequip");
        }
        else {
            buttonText.text = "Equip";
            equipButton.SetText("Equip");
        }

        TooltipManager.Hide();
        OnEquipClicked();
    }

    public void OnEquipClicked() {
        
        if(selectedKnownEntry == null) {
            return;
        }


        if(selectedActiveEntry.Ability != null && selectedKnownEntry.Ability == selectedActiveEntry.Ability) {
            selectedActiveEntry.Ability.Uneqeuip();
            selectedActiveEntry.AssignNewAbility(null);
            Close();
            return;
        }

        if (selectedKnownEntry.Ability.IsEquipped == true) {
            Debug.LogWarning(selectedKnownEntry.Ability.Data.abilityName + " needs to be moved");

            Close();
            return;
        }


        if (selectedActiveEntry.Ability != null) {
            selectedActiveEntry.Ability.Uneqeuip();
        }
 
        selectedActiveEntry.AssignNewAbility(selectedKnownEntry.Ability);
        selectedKnownEntry.Ability.Equip();
        Close();
    }

}
