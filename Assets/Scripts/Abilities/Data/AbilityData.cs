using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;


public enum AbilityTag {
    None,
    Fire,
    Poison,
    Healing,
    Melee,
    Force,
    Area,
    Ranged,
    Projectile,
    Physical,
    Affliction,
    Water,
    Summoning,
    Death,
    Space,
    Teleportation,
    CastTime,
    Air,
    Channeled,
    ClassFeature,
    Orbital,
    Weapon,
    Arcane,
    Void,
    Time,
    Passive,
    Mastery,
    Lightning,
    DamageOverTime
}

public enum AbilityCategory {
    ActiveSkill,
    KnownSkill,
    Item,
    Rune,
    Any,
    ChildAbility,
    PassiveSkill,
    ClassFeatureSkill,
}


[Serializable]
public class AbilityData 
{

    public AbilityCategory category;
    public Sprite abilityIcon;
    public string abilityName;
    public string abilityDescription;
    public float resourceCost = 0f;
    public bool toBestow;
    public bool suspend;
    public bool autoFire;
    public bool ignoreOtherCasting;
    public bool includeEffectsInTooltip;
    public bool showChildAbilitiesInTooltip;
    public bool ignoreTooltip;
    public bool recastOnChannelCost;
    public bool normalizedProcRate;
    public bool startingAbility;
    public bool scaleProcByLevel;
    public string runeAbilityTarget;
    public string animationString;
    public bool waitForAnimToResolve;

    public float initSFXVolume = 1f;
    public List<AudioClip> initiationSounds = new List<AudioClip>();
    public float channelSFXVolume = 1f;
    public List<AudioClip> channeledSounds = new List<AudioClip>();

    public List<AbilityDefinition> childAbilities = new List<AbilityDefinition>();
    public List<AbilityRuneGroupData> runeGroupData = new List<AbilityRuneGroupData>();

    public List<AbilityTag> tags = new List<AbilityTag>();

    public List<GameEvent> resetEvents = new List<GameEvent>();
    public List<TriggerData> activationTriggerData = new List<TriggerData>();
    public List<TriggerData> endTriggerData = new List<TriggerData>();

    public TriggerActivationCounterData counterData = new TriggerActivationCounterData();
    public TriggerActivationCounterData endCounterData = new TriggerActivationCounterData();

    public List<EffectDefinition> effectDefinitions = new List<EffectDefinition>();
    public List<EffectData> effectData = new List<EffectData>();

    public int startingRecoveryCharges = 1;
    public int baseRuneSlots = 2;
    public int maxRanks = -1;
    public GameObject windupVFX;

    public List<StatData> abilityStatData = new List<StatData>();

    public List<RecoveryData> recoveryData = new List<RecoveryData>();
    

    public AbilityData() {

    }

    public AbilityData(AbilityData copy, bool bestow = false) {
        this.abilityName = copy.abilityName;
        this.toBestow = bestow;
        this.suspend = copy.suspend;

        this.counterData = new TriggerActivationCounterData();
        this.endCounterData = new TriggerActivationCounterData();

        CloneTriggers(activationTriggerData, copy.activationTriggerData);
        CloneTriggers(endTriggerData, copy.endTriggerData);
        CopyEffects(copy.effectData);
    }

    private void CopyEffects(List<EffectData> copy) {
        for (int i = 0; i < copy.Count; i++) {
            effectData.Add(new EffectData(copy[i]));
        }
    }

    private void CloneTriggers(List<TriggerData> target, List<TriggerData> clone) {
        int count = clone.Count;
        for (int i = 0; i < count; i++) {
            target.Add(new TriggerData(clone[i]));
        }
    }


    public bool ContainsStat(StatName stat) {
        for (int i = 0; i < abilityStatData.Count; i++) {
            if (abilityStatData[i] == null) 
                continue;
            
            if (abilityStatData[i].statName == stat) 
                return true;
        }

        return false;
    }

    public bool HasManualActivation() {
        for (int i = 0; i < activationTriggerData.Count; i++) {
            if (activationTriggerData[i].type == TriggerType.UserActivated || activationTriggerData[i].type == TriggerType.AIActivated) {
                return true;
            }
        }

        return false;
    }

}
