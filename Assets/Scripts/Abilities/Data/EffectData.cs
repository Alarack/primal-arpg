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
    Movement
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
    public EffectType type;
    public EffectTarget targeting;

    public int numberOfTargets = -1;
    public string otherAbilityName;
    public string otherEffectName;

    public List<ConstraintData> targetConstraints = new List<ConstraintData>();


    //Payload Delivery
    public Entity payloadPrefab;
    public int payloadCount = 1;
    public float shotDelay = 0.2f;
    public DeliverySpawnLocation spawnLocation;
    public EffectZoneInfo effectZoneInfo;

    //Stat Adjustment
    public List<StatModifierData> modData = new List<StatModifierData>();
    public Gradient floatingTextColor;
    public bool applyToEffect;
    public bool applyToOtherStatAdjustment;
    public StatModifierData.StatModDesignation effectDesignation;

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



    public EffectData() {

    }

    public EffectData(EffectData copy) {
        this.effectName = copy.effectName;
        this.type = copy.type;
        this.targeting = copy.targeting;
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
