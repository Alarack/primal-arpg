using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkillPanel : BasePanel
{

    public SkillEntry selectedActiveEntry;
    public SkillEntry selectedKnownEntry;



    public void OnEquipClicked() {
        
        if(selectedKnownEntry == null) {
            return;
        }


        if(selectedActiveEntry.Ability != null) {
            selectedActiveEntry.Ability.Uneqeuip();
        }
        
        selectedActiveEntry.AssignNewAbility(selectedKnownEntry.Ability);
        selectedKnownEntry.Ability.Equip();
        Close();
    }

}
