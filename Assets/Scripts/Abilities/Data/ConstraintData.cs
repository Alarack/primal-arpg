using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.FSM;


#region ENUMS

public enum ConstraintType {
    StatChanged,
    ParentAbilityTag,
    Collision,
    Range,
    Owner,
    StatMinimum,
    StatMaximum,
    SourceOnly,
    Subtype,
    PrimaryType,
    EntityName,
    HasStatus,
    IsInState,
    StateEntered,
    StatedExited,
    HasTarget,
    Dashing,
    AbilityTag,
    AbilityName,
    EffectName,
    EffectDesignation,
    UnitDamaged,
    MostStat,
    LeastStat,
    AbilityActive,
    EffectApplied,
    EffectType,
    StatRatio,
    UnitIsMoving,
    AbilityOnHotbar,
    EffectAppliedTo,
    TriggerOnly,
    CauseOnly,
    AbilityInSlot,
    HasDot,
    HasProjectile,
    StatusApplied,
    AbilityCategory
}

public enum GainedOrLost {
    Gained,
    Lost,
    None
}

public enum OwnerConstraintType {
    Friendly,
    Enemy,
    Ally,
    Neutral
}

public enum ConstraintFocus {
    Source,
    Trigger,
    Cause,
    AbilitySource,
    AbilityTrigger,
    AbiityCause
}

#endregion

[Serializable]
public class ConstraintData
{
    public ConstraintType type;
    public bool inverse;

    //Stat Changed
    public StatName statChangeTarget;
    public GainedOrLost changeDirection;
    public bool minStatChangeRequired;
    public float statChangeDelta = 0f;
    public bool constrainByDelivery;
    public Entity deliveryConstraint;

    //Parent Ability Tag
    //public AbilityTag targetTag;

    //Range
    public float minRange;
    public float maxRange;
    public EffectTarget rangeToWhat;
    //TODO: Range to what? Target? Some other target?

    //Owner
    public OwnerConstraintType ownerTarget;

    //Most Stat
    public StatName mostStatTarget;
    public string mostStatEffectName;
    public string mostStatAbilityName;

    //Least Stat
    public StatName leastStatTarget;
    public string leastStatEffectName;
    public string leastStatAbilityName;

    //Min Max Percent of Max
    public bool checkPercentOfMax;

    //Stat Minimum
    public StatName minStatTarget;
    public float minStatValue;
    public bool nonZero;

    //Stat Maximum
    public StatName maxStatTarget;
    public float maxStatValue;

    //Entity Name
    public string targetEntityName;

    //Subtype
    public Entity.EntitySubtype targetSubtype;

    //Primary Type
    public Entity.EntityType targetPrimaryType;

    //Ability Active
    public string targetActiveAbilityName;

    //Effect Applied / Rider
    public EffectType appliedEffectType;
    public bool appliedEffectName;

    //Is In State
    public StateData targetStateData;

    //State Entered
    public string stateEntered;

    //State Exited
    public string stateExited;

    //Ability & Effect Constraints
    public AbilityTag targetAbilityTag;
    public string targetAbiltyName;
    public string targetEffectName;
    public StatModifierData.StatModDesignation effectDesigantion;
    public AbilityCategory targetAbilityCategory;

    //Ability On Hotbar
    //public string targetAbilityOnHotbar;

    //Effect Type
    public EffectType targetEffectType;

    //Has Status
    public Status.StatusName targetStatus;
    public bool hasDotStatus;

    //Status Applied
    public Status.StatusName appliedStatusType;

    //Stat Ratio
    public StatName statRatioTarget;
    public float targetRatio;

    //Is Moving
    public float movementMagnitudeLimit = 0.1f;

    //Ability In Slot
    public int abilitySlot;

    //Has Projectile
    public Projectile projectileToCheck;

    public ConstraintData() {

    }

    public ConstraintData(ConstraintData clone) {
        this.type = clone.type;
        this.targetPrimaryType = clone.targetPrimaryType;
        this.inverse = clone.inverse;
        //this.currentZoneTarget = clone.currentZoneTarget;
        //this.previousZoneTarget = clone.previousZoneTarget;
        this.targetSubtype = clone.targetSubtype;
        this.statChangeTarget = clone.statChangeTarget;
        this.changeDirection = clone.changeDirection;
        this.ownerTarget = clone.ownerTarget;
        this.mostStatTarget = clone.mostStatTarget;
        this.mostStatAbilityName = clone.mostStatAbilityName;
        this.mostStatEffectName = clone.mostStatEffectName;
        this.leastStatTarget = clone.leastStatTarget;
        this.leastStatAbilityName = clone.leastStatAbilityName;
        this.leastStatEffectName = clone.leastStatEffectName;
        this.maxStatTarget = clone.maxStatTarget;
        this.maxStatValue = clone.maxStatValue;
        this.minStatTarget = clone.minStatTarget;
        this.minStatValue = clone.minStatValue;
        //this.ownersTurn = clone.ownersTurn;
        this.targetActiveAbilityName = clone.targetActiveAbilityName;
        this.appliedEffectType = clone.appliedEffectType;
        this.appliedEffectName = clone.appliedEffectName;
        this.targetEntityName = clone.targetEntityName;
        //this.minRiderTargets = clone.minRiderTargets;
        //this.maxRiderTargets = clone.maxRiderTargets;

    }

}

/// <summary>
/// A class to house the focus and the constraint data pertaining to that focus.
/// </summary>
[Serializable]
public class ConstraintDataFocus {

    /// <summary>
    /// Who are we comparing our constraint data to?
    /// </summary>
    public ConstraintFocus focus;
    public List<ConstraintData> constraintData = new List<ConstraintData>();


    public ConstraintDataFocus() {

    }

    public ConstraintDataFocus(ConstraintDataFocus clone) {
        this.focus = clone.focus;
        this.constraintData = new List<ConstraintData>();
        int count = clone.constraintData.Count;
        for (int i = 0; i < count; i++) {
            this.constraintData.Add(new ConstraintData(clone.constraintData[i]));
        }
    }

    public ConstraintDataFocus(ConstraintFocus focus) {
        this.focus = focus;
    }
}
