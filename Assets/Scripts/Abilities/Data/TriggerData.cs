using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum TriggerType {
    None,
    UnitStatChanged,
    UnitDied,
    StatusApplied,
    StatusRemoved,
    Rider,
    UserActivated,
    Timed,
    AutoActivated,
    Collision,
    UnitDetected,
    AbilityResolved,
    UnitForgotten,
    WeaponCooldownFinished,
    WeaponCooldownStarted,
    StateEntered,
    StateExited,
    AbilityEquipped,
    AbilityUnequipped,
    AbilityLearned,
    AbilityUnlearned,
    RuneEquipped,
    RuneUnequipped,
    DashStarted,
    ProjectilePierced,
    ProjectileChained,
    ProjectileSplit,
    UnitDiedWithStatus,
    OverloadTriggered,
    ProjectileCreated
}


[Serializable]
public class TriggerData
{

    public TriggerType type;

    public string riderAbilityName;
    public string riderEffectName;

    public float triggerTimerDuration;

    public List<ConstraintDataFocus> allConstraints = new List<ConstraintDataFocus>();


    public TriggerData() {

    }

    public TriggerData(TriggerData clone) {
        this.type = clone.type;
        this.riderEffectName = clone.riderEffectName;
        this.riderAbilityName = clone.riderAbilityName;
        this.allConstraints = new List<ConstraintDataFocus>();

        for (int i = 0; i < clone.allConstraints.Count; i++) {
            this.allConstraints.Add(new ConstraintDataFocus(clone.allConstraints[i]));
        }
    }

    public bool HasConstraintListOfType(ConstraintFocus type) {
        return GetConstraintDataListByTarget(type) != null;
    }

    private List<ConstraintData> GetConstraintDataListByTarget(ConstraintFocus focus) {
        for (int i = 0; i < allConstraints.Count; i++) {
            if (allConstraints[i].focus == focus)
                return allConstraints[i].constraintData;
        }

        return null;
    }

    public ConstraintDataFocus GetListByType(ConstraintFocus focus) {
        int count = allConstraints.Count;
        for (int i = 0; i < count; i++) {
            if (allConstraints[i].focus == focus)
                return allConstraints[i];
        }

        return null;
    }

}

[System.Serializable]
public class TriggerActivationCounterData {

    public bool useCustomRefreshTrigger;
    public TriggerData customRefreshTriggerData = new TriggerData();

    //This Constraint can only be true if a trigger has happend X or less times.
    public bool limitedNumberOfTriggers;
    public int maxTriggerCount;

    //This Constraint can only be true if a trigger has happend X or more times.
    public bool requireMultipleTriggers;
    public int minTriggerCount;

}
