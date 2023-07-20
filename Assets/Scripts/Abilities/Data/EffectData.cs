using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum EffectType {
    None,
    StatAdjustment,
    SpawnProjectile,
    AddStatus,
    RemoveStatus,
    Movement,
    AddChildAbility,
    ApplyOtherEffect
}

public enum EffectTarget {
    None,
    Source,
    Trigger,
    Cause,
    LogicSelected,
    PayloadDelivered,
    OtherEffectTarget,
    OtherMostRecentTarget,
    LogicSelectedAbility,
    LogicSelectedEffect,
    CurrentValidTargets
}

public enum EffectSubTarget {
    None,
    Entity,
    Effect,
    StatModifier,
    Ability,
    StatScaler
}

public enum DeliverySpawnLocation { 
    Source,
    Trigger,
    Cause,
    MousePointer,
    Target
}

public enum MovementDestination {
    SourceForward,
    SourceCurrentVelocity,
    MousePosition
}

[Serializable]
public class EffectData 
{
    public string effectName;
    public string effectDescription;
    public EffectType type;
    public EffectTarget targeting;
    public EffectSubTarget subTarget;
    public bool deliveryPayloadToTarget;

    public int numberOfTargets = -1;
    public string otherAbilityName;
    public string otherEffectName;

    public List<ConstraintData> targetConstraints = new List<ConstraintData>();


    //Payload Delivery
    public Entity payloadPrefab;
    public int payloadCount = 1;
    //public int projectilePierceCount = 0;
    public float shotDelay = 0.2f;
    public List<StatData> payloadStatData = new List<StatData>();
    public DeliverySpawnLocation spawnLocation;
    public EffectZoneInfo effectZoneInfo;

    //Stat Adjustment
    public List<StatModifierData> modData = new List<StatModifierData>();
    public Gradient floatingTextColor;
    public StatModifierData.StatModDesignation effectDesignation;
    //public List<StatScaler> adjustmentOptions = new List<StatScaler>();


    //Add Status
    public List<StatusData> statusToAdd = new List<StatusData>();

    //Remove Status
    public List<Status.StatusName> statusToRemove = new List<Status.StatusName>();

    //Movement
    public bool isTeleport;
    public bool invertDestination;
    public MovementDestination targetDestination;

    public float moveForce;

    //Spawn Projectile
    public List<Projectile> tokenPrefabs = new List<Projectile>();
    public DeliverySpawnLocation tokenSpawnLocation;
    public float overlapCircleRadius;
    public LayerMask overlapLayerMask;

    //Add Child Ability
    public List<AbilityDefinition> abilitiesToAdd = new List<AbilityDefinition>();

    //Apply Other Effect
    public string targetOtherEffectParentAbilityName;
    public string targetOtherEffectName;
    public bool applyTriggeringEffect;


    public EffectData() {

    }

    public EffectData(EffectData copy) {
        this.effectName = copy.effectName;
        this.type = copy.type;
        this.targeting = copy.targeting;
        this.deliveryPayloadToTarget = copy.deliveryPayloadToTarget;
        this.numberOfTargets = copy.numberOfTargets;
        this.otherAbilityName = copy.otherAbilityName;
        this.otherEffectName = copy.otherEffectName;
        CopyStatModData(copy.modData);
        CloneTargetConstraints(copy.targetConstraints);
        //this.forcedRemoveal = copy.forcedRemoveal;
        //CloneKeywords(keywordsToAdd, copy.keywordsToAdd);
        //CloneKeywords(keywordsToRemove, copy.keywordsToRemove);
    }

    private void CopyStatModData(List<StatModifierData> copy) {
        for (int i = 0; i < copy.Count; i++) {
            modData.Add(new StatModifierData(copy[i]));
        }
    }

    private void CloneTargetConstraints(List<ConstraintData> clone) {
        int count = clone.Count;
        for (int i = 0; i < count; i++) {
            targetConstraints.Add(new ConstraintData(clone[i]));
        }
    }

    //private void CloneKeywords(List<KeywordData> target, List<KeywordData> clone) {
    //    for (int i = 0; i < clone.Count; i++) {
    //        target.Add(new KeywordData(clone[i]));
    //    }
    //}

}

[System.Serializable]
public class StatScaler {

    //public enum OptionType {
    //    StatScaler,
    //    WeaponDamageScaler
    //}

    //public OptionType type;
    public StatName targetStat;
    public StatModifierData.DeriveFromWhom deriveTarget;
    public float statScaleBaseValue;
    public SimpleStat scalerStat;

    public StatScaler() {
        
    }

    public void InitStat() {
        scalerStat = new SimpleStat(StatName.StatScaler, statScaleBaseValue);
    }

    public StatScaler(StatScaler clone) {
        //this.type = clone.type;
        this.targetStat = clone.targetStat;
        this.statScaleBaseValue = clone.statScaleBaseValue;
        this.deriveTarget = clone.deriveTarget;

        this.scalerStat = new SimpleStat(StatName.StatScaler, clone.statScaleBaseValue);
    }

    public void AddScalerMod(StatModifier mod) {
        scalerStat.AddModifier(mod);
    }

    public void RemoveScalerMod(StatModifier mod) {
        scalerStat.RemoveModifier(mod); 
    }

    //public float GetAdjustment(StatAdjustmentEffect effect) {

    //    float result = type switch {
    //        OptionType.StatScaler => effect.Stats[statScaler] * statMultiplier,
    //        _ => 1f
    //    };

    //    return result;

    //}



}
