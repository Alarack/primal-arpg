using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.MUIP;

public class PassiveSkillPanel : BasePanel
{

    public SkillEntry selectedActiveEntry;
    public SkillEntry selectedKnownEntry;

    public TextMeshProUGUI buttonText;

    public ButtonManager equipButton;

    public SkillsPanel skillPanel;

    public override void Open() {
        base.Open();

        skillPanel.ClearHighlights();

        SkillEntry knownLastClicked = skillPanel.GetMatchingActiveSlot(selectedActiveEntry);

        if (knownLastClicked != null) {
            knownLastClicked.SelectPassive();
            selectedKnownEntry = knownLastClicked;
            OnKnownEntryClicked(knownLastClicked);
        }
    }

    public override void Close() {
        base.Close();

        selectedActiveEntry = null;
        selectedKnownEntry = null;
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



    private void SetButtonText() {

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
