using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


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
    Water
}

public enum AbilityCategory {
    ActiveSkill,
    KnownSkill,
    Item,
    Rune,
    Any,
    ChildAbility,
    PassiveSkill
}


[Serializable]
public class AbilityData 
{

    #region NOTES

    //What is an ability?

    //Activation Triggers
    //How does an ability get activated?

    //User Activated - Pushing a button on your hotbar
    //On Stat Changed - Trigger an ability if I take damage?
    //On Collision - If the source of this ability collides with something?
    //On Unit Dies - If an Entity dies?
    //On Item Collected - When I get money, or pick up some resource.
    //On Status Applied - When a status is first applied.
    //On Status Removed - when a status ends / is removed.
    //On Other Effect Applied / Rider - If one ability or effect resolves, trigger this one too.
    //Timed - Activated automaticaly every X seconds.

    //Activation Delay - Start the activation, but then wait X time, and check constraints again.

    //Ending Triggers
    //How does this ability end?
    //Could be any of the same triggers that activate an ability.

    //Constraints
    //Constraint Types
    //Stat Minimum Constrain - Only trigger this ability if I have X Health or more.
    //Could also be a ratio? 50% Health ect...
    //Stat Maximum Constraint - Only trigger this ability if I have X Health or Less.
    //Most Stat - Does the target have the most health?
    //Least Stat - Does the target have the least health?

    //On Collision Constraint - Who collides with what? If I step on a trap? If an enemy collides with me? 
    //Environmental Constraint - Am I in the rain / wind / some other kind of place?
    //Range Constraint - Am I with X units of somthing?
    //Owner Constraint - Is the thing mine, or an enemies, or an ally's?
    //Stat Changed Constraint - What stat changed? Did it go up, or down?
    //Rider Ability Tag Constraint - Does the Triggering Ability have X Tag?

    //Trigger Constraint targeting - Who should I compare my constraint data to?

    //Source - Does the source of the ablity meet certain conditions?
    //Trigger - Does the target of a Trigger of the abiliy meet certain conditions?
    //Cause - Does the cause of a trigger of the abiliy meet certain conditions?

    //Constraint Duration - If a constraint is true for X amount of time.

    //Ability Tags - A way to group similar themed abilities


    //Effects - What does this ability actually do?

    //Targeting Method
    //Source
    //User Selected
    //Payload Delivery

    //Number of Targets?

    //Target Constraints
    //Target - Does the target of the ablity meet certain conditions?


    //Aquisition - How can the player get this ability? / Unlock requirements

    //Recovery / Use Limitation - How often can I use this ability, and how do I get it back?

    //Recovery Types
    //Cooldown - Time until you can use it again.
    //Charges - can be used X times. Might regain charges on a cooldown?
    //Triggered Charges? Can we use the above trigger system here to regain charges?


    #endregion


    public AbilityCategory category;
    public Sprite abilityIcon;
    public string abilityName;
    public string abilityDescription;
    public float resourceCost = 0f;
    public bool toBestow;
    public bool suspend;
    public bool autoFire;
    public bool includeEffectsInTooltip;
    public bool ignoreTooltip;
    public string runeAbilityTarget;

    public List<AbilityDefinition> childAbilities = new List<AbilityDefinition>();

    public List<AbilityTag> tags = new List<AbilityTag>();

    public List<TriggerData> activationTriggerData = new List<TriggerData>();
    public List<TriggerData> endTriggerData = new List<TriggerData>();

    public TriggerActivationCounterData counterData = new TriggerActivationCounterData();
    public TriggerActivationCounterData endCounterData = new TriggerActivationCounterData();

    public List<EffectDefinition> effectDefinitions = new List<EffectDefinition>();
    public List<EffectData> effectData = new List<EffectData>();

    public int startingRecoveryCharges = 1;
    public int baseRuneSlots = 2;
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

}
