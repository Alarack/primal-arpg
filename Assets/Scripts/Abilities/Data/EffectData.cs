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
    ApplyOtherEffect,
    AddStatScaler,
    ForceStatusTick,
    SpawnEntity,
    Teleport,
    ActivateOtherAbility,
    NPCStateChange,
    AddAbility,
    AddEffect,
    AddTag,
    RemoveTag,
    SuppressEffect,
    RemoveEffect
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
    CurrentValidTargets,
    CurrentAIBrainTarget
}

public enum EffectSubTarget {
    None,
    Entity,
    Effect,
    StatModifier,
    Ability,
    StatScalerMod
}

public enum DeliverySpawnLocation { 
    Source,
    Trigger,
    Cause,
    MousePointer,
    AITarget,
    FixedLocations,
    RandomViewportPosition,
    WorldPositionSequence,
    AbilityLastPayloadLocation,
    LastEffectZoneLocation
}

public enum DeliverySpawnLocationHardPoint {
    LeftCenter,
    RightCenter,
    TopCenter,
    BottomCenter,
}

public enum MovementDestination {
    SourceForward,
    SourceCurrentVelocity,
    MousePosition,
    AwayFromSource
}

public enum TeleportDestination {
    MousePointer,
    RandomViewport,
    RandomNearTarget,
    SourceForward,
    OtherTarget,
    TargetSequence
}

public enum EntitySpawnType {
    Manual,
    Clone,
    Series
}

[Serializable]
public class EffectData 
{
    public string effectName;
    public string effectDescription;
    public EffectType type;
    public EffectTarget targeting;
    public EffectSubTarget subTarget;
    public MaskTargeting maskTargeting;
    public bool hideFloatingText;
    public bool deliveryPayloadToTarget;
    public bool canOverload;
    public bool canAffectDeadTargets;
    public bool nonStacking;
    //public bool hideTooltip;
    public bool onlyShowTooltipInRune;
    public bool inheritStatsFromParentAbility = true;

    public int numberOfTargets = -1;
    public string otherAbilityName;
    public string otherEffectName;
    public string targetAbilityForLastPayload;

    public List<ConstraintData> targetConstraints = new List<ConstraintData>();
    public List<EffectDefinition> riderEffects = new List<EffectDefinition>();

    //Payload Delivery
    public Entity payloadPrefab;
    public int payloadCount = 1;
    //public int projectilePierceCount = 0;
    public float shotDelay = 0.2f;
    public List<StatData> payloadStatData = new List<StatData>();
    public DeliverySpawnLocation spawnLocation;
    public WorldPositionConstant spawnLocationStart;
    public WorldPositionConstant spawnLocationEnd;
    public Vector2 minViewportValues;
    public Vector2 maxViewportValues;
    public EffectZoneInfo effectZoneInfo;
    public LayerMask projectileHitMask;

    //Stat Adjustment
    public List<StatModifierData> modData = new List<StatModifierData>();
    public Gradient floatingTextColor = new Gradient();
    public Gradient overloadFloatingTextColor = new Gradient();
    public StatModifierData.StatModDesignation effectDesignation;
    //public List<StatScaler> adjustmentOptions = new List<StatScaler>();


    //Add Status
    public List<StatusData> statusToAdd = new List<StatusData>();

    //Remove Status
    public List<Status.StatusName> statusToRemove = new List<Status.StatusName>();

    //Movement
    public bool isTeleport;
    public bool invertDestination;
    public bool resetMovement;
    public MovementDestination targetDestination;
    //public float moveForce;

    //Teleport
    public TeleportDestination teleportDestination;
    public GameObject teleportVFX;
    public float forwardDistance;

    //Spawn Projectile
    public List<Projectile> tokenPrefabs = new List<Projectile>();
    public DeliverySpawnLocation tokenSpawnLocation;
    public float overlapCircleRadius;
    public LayerMask overlapLayerMask;

    //Add Child Ability
    public List<AbilityDefinition> abilitiesToAdd = new List<AbilityDefinition>();

    //Add Effect
    public List<EffectDefinition> effectsToAdd = new List<EffectDefinition>();
    public string targetAbilityToAddEffectsTo;

    //Remove Effect
    public List<string> effectsToRemove = new List<string>();
    public string targetAbilityToRemoveEffectsFrom;

    //Apply Other Effect
    public string targetOtherEffectParentAbilityName;
    public string targetOtherEffectName;
    public bool applyTriggeringEffect;

    //Add Stat Scaler
    public List<StatScaler> statScalersToAdd = new List<StatScaler>();

    //Spawn Entity
    public Entity entityPrefab;
    public float percentOfPlayerDamage = 1f;
    public bool destroyPreviousSummonAtCap;
    public bool inheritParentLayer = true;
    public EntitySpawnType spawnType;

    //Activate Other Ability
    public string nameOfAbilityToActivate;

    //NPC State Change
    public string targetStateName;

    //Add Tag
    public List<AbilityTag> tagsToAdd = new List<AbilityTag>();

    //Remove Tag
    public List<AbilityTag> tagsToRemove = new List<AbilityTag>();

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

    public float HasStat(StatName stat) {
        for (int i = 0; i < payloadStatData.Count; i++) {
            if (payloadStatData[i].statName == stat)
                return payloadStatData[i].value;
        }

        return 0f;
    }

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


        if(clone.scalerStat == null) {
            this.scalerStat = new SimpleStat(StatName.StatScaler, clone.statScaleBaseValue);
            //Debug.Log("Cloned scaler had a null stat");
        }
        else {
            //Debug.Log("Cloned scaler didn't have a null stat");
            this.scalerStat = new SimpleStat(clone.scalerStat);
        }

        //Debug.Log("Cloning a scaler: ");
        //Debug.Log("Stat scaler null: " + clone.scalerStat == null);

        //this.scalerStat = new SimpleStat(clone.scalerStat);

        //this.scalerStat.CloneMods(clone.scalerStat);

    }

    public void AddScalerMod(StatModifier mod) {
        scalerStat.AddModifier(mod);
    }

    public void RemoveScalerMod(StatModifier mod) {
        scalerStat.RemoveModifier(mod); 
    }

    public void RemoveAllScalarModsFromSource(object source) {
        scalerStat.RemoveAllModifiersFromSource(source);
    }

    //public float GetAdjustment(StatAdjustmentEffect effect) {

    //    float result = type switch {
    //        OptionType.StatScaler => effect.Stats[statScaler] * statMultiplier,
    //        _ => 1f
    //    };

    //    return result;

    //}



}
