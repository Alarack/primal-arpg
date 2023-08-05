using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using TriggerInstance = AbilityTrigger.TriggerInstance;

public class Ability {

    public List<AbilityTag> Tags { get; protected set; } = new List<AbilityTag>();
    public Entity Source { get; protected set; }
    public AbilityData Data { get; protected set; }
    public bool IsActive { get; protected set; }
    public bool AutoFire { get; protected set; }

    public bool IsEquipped { get; protected set; }

    //Recovery Stuff
    public bool IsReady { get { return CheckReady(); } }
    public bool HasRecovery { get { return recoveryMethods.Count > 0; } }
    public int Charges { get { return Mathf.FloorToInt(Stats[StatName.AbilityCharge]); } }
    public int MaxCharges { get { return Mathf.FloorToInt(Stats.GetStatRangeMaxValue(StatName.AbilityCharge)); } }
    public int RuneSlots { get { return Mathf.FloorToInt(Stats[StatName.AbilityRuneSlots]); } }

    public List<Item> equippedRunes = new List<Item>();

    protected List<AbilityTrigger> activationTriggers = new List<AbilityTrigger>();
    protected List<AbilityTrigger> endTriggers = new List<AbilityTrigger>();

    protected TriggerActivationCounter activationCounter;
    protected TriggerActivationCounter endCounter;


    protected List<Effect> effects = new List<Effect>();
    protected List<AbilityRecovery> recoveryMethods = new List<AbilityRecovery>();

    protected List<Action<BaseStat, object, float>> recoveryStatListeners = new List<Action<BaseStat, object, float>>();

    protected Task currentWindup;

    public List<Ability> ChildAbilities { get; protected set; } = new List<Ability>();

    public Ability ParentAbility { get; protected set; }

    public StatCollection Stats { get; protected set; }

    public Ability(AbilityData data, Entity source) {
        this.Data = data;
        this.Source = source;
        this.Tags = new List<AbilityTag>(data.tags);
        //SetupActivationTriggers();
        //SetupEndTriggers();
        //SetupTriggerCounters();


        //for (int i = 0; i < Tags.Count; i++) {
        //    Debug.Log(data.abilityName + " has a " + Tags[i] + " tag");
        //}

        SetupStats();
        SetupRecoveries();
        SetupEffects();

        SetupChildAbilities();

    }

    #region SETUP AND TEAR DOWN


    private void SetupStats() {
        Stats = new StatCollection(this, Data.abilityStatData);

        if(Stats.Contains(StatName.AbilityRuneSlots) == false) {
            SimpleStat runeSlots = new SimpleStat(StatName.AbilityRuneSlots, 2);
            Stats.AddStat(runeSlots);
        }

        if(Stats.Contains(StatName.AbilityCharge) == false) {
            StatRange charges = new StatRange(StatName.AbilityCharge, 0, Data.startingRecoveryCharges, Data.startingRecoveryCharges);
            Stats.AddStat(charges);
        }

        //if(Stats.Contains(StatName.EssenceCost) == false) {

        //}


        //if(Data.resourceCost > 0) {
        //    SimpleStat essenceCost = new SimpleStat(StatName.EssenceCost, Data.resourceCost);
        //    Stats.AddStat(essenceCost);
        //}

    }

    private void SetupChildAbilities() {
        for (int i = 0; i < Data.childAbilities.Count; i++) {
            AddChildAbility(Data.childAbilities[i]);
        }
    }

    public void Equip() {
        //Debug.Log("Equipping: " + Data.abilityName);
        if (IsEquipped == true) {
            Debug.LogError("Tried to equip " + Data.abilityName + " but it was alread equipped");
            return;
        }



        SetupActivationTriggers();
        SetupEndTriggers();
        SetupTriggerCounters();
        //SetupRecoveries();
        RegisterAbility();
        IsEquipped = true;

        EventData data = new EventData();
        data.AddAbility("Ability", this);

        EventManager.SendEvent(GameEvent.AbilityEquipped, data);

        for (int i = 0; i < ChildAbilities.Count; i++) {
            ChildAbilities[i].Equip();
        }
    }

    public void Uneqeuip() {
        //Debug.Log("Unequipping: " + Data.abilityName);
        if (IsEquipped == false) {
            Debug.LogError("Tried to unequip " + Data.abilityName + " but it wasn't equipped");
            return;
        }

        //for (int i = 0; i < equippedRunes.Count; i++) {
        //    equippedRunes[i].UnEquip();
        //}

        EventData data = new EventData();
        data.AddAbility("Ability", this);

        EventManager.SendEvent(GameEvent.AbilityUnequipped, data);

        UnregisterAbility();
        TearDown();
        IsEquipped = false;

        //for (int i = 0; i < ChildAbilities.Count; i++) {
        //    ChildAbilities[i].Uneqeuip();
        //}
    }

    private void RegisterAbility() {

        if(Source is NPC) {
            return;
        }

        List<Ability> list = Data.category switch {
            //AbilityCategory.ActiveSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            //AbilityCategory.KnownSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            AbilityCategory.Item => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Item],
            AbilityCategory.Rune => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Rune],
            //AbilityCategory.ChildAbility => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.ChildAbility],
            AbilityCategory.PassiveSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.PassiveSkill],
            _ => null,
        };

        if (list != null) {
            list.AddUnique(this);

        }

    }

    private void UnregisterAbility() {
        if (Source is NPC) {
            return;
        }

        List<Ability> list = Data.category switch {
            //AbilityCategory.ActiveSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            //AbilityCategory.KnownSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            AbilityCategory.Item => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Item],
            AbilityCategory.Rune => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Rune],
            //AbilityCategory.ChildAbility => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.ChildAbility],
            //AbilityCategory.PassiveSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.PassiveSkill],
            _ => null,
        };

        if (list != null) {
            list.RemoveIfContains(this);
        }
    }

    protected void SetupActivationTriggers() {
        foreach (TriggerData triggerData in Data.activationTriggerData) {

            AbilityTrigger trigger = AbilityFactory.CreateAbilityTrigger(triggerData, Source, this);
            trigger.ActivationCallback = ReceiveStartActivationInstance;
            activationTriggers.Add(trigger);
        }

        AutoFire = HasAutoFire();

        SetupEffectRiderEvents();
    }

    protected void SetupEffectRiderEvents() {
        for (int i = 0; i < effects.Count; i++) {
            effects[i].RegisterRiderEvents();
        }
    }

    protected void RemoveEffectRiderEvents() {
        for (int i = 0; i < effects.Count; i++) {
            effects[i].UnRegisterRiderEvents();
        }
    }

    protected void SetupEndTriggers() {
        foreach (TriggerData triggerData in Data.endTriggerData) {

            AbilityTrigger trigger = AbilityFactory.CreateAbilityTrigger(triggerData, Source, this);
            trigger.ActivationCallback = RecieveEndActivationInstance;
            endTriggers.Add(trigger);

        }
    }

    protected void SetupTriggerCounters() {
        if (Data.counterData.limitedNumberOfTriggers == true || Data.counterData.requireMultipleTriggers == true)
            activationCounter = new TriggerActivationCounter(Data.counterData, Source, this);
        else
            activationCounter = null;

        if (Data.endCounterData.limitedNumberOfTriggers == true || Data.endCounterData.requireMultipleTriggers == true)
            endCounter = new TriggerActivationCounter(Data.endCounterData, Source, this);
        else
            endCounter = null;

    }

    protected void SetupEffects() {
        for (int i = 0; i < Data.effectData.Count; i++) {
            Effect effect = AbilityFactory.CreateEffect(Data.effectData[i], Source, this);
            effects.Add(effect);
        }

        for (int i = 0; i < Data.effectDefinitions.Count; i++) {
            Effect effect = AbilityFactory.CreateEffect(Data.effectDefinitions[i].effectData, Source, this);
            effects.Add(effect);

            //Debug.LogWarning("An ability: " + Data.abilityName + " is creating an effect: " + effect.Data.effectName);

        }
    }

    protected void SetupRecoveries() {

        //Stats for Charges
        //recoveryStats = new StatCollection(this);
        //StatRange charges = new StatRange(StatName.AbilityCharge, 0, Data.startingRecoveryCharges, Data.startingRecoveryCharges);
        //recoveryStats.AddStat(charges);

        for (int i = 0; i < Data.recoveryData.Count; i++) {
            AbilityRecovery recovery = AbilityFactory.CreateAbilityRecovery(Data.recoveryData[i], Source, this);
            recoveryMethods.Add(recovery);

            //Debug.Log("Creating a recovery method for: " + Data.abilityName + " " + recovery.Type); 
        }
    }

    public void TearDown() {
        //End all current Effects
        ForceEndTrigger(null);

        //Tear down actvation triggers
        for (int i = 0; i < activationTriggers.Count; i++) {
            activationTriggers[i].TearDown();
        }
        activationTriggers.Clear();

        //Tear down end triggers
        for (int i = 0; i < endTriggers.Count; i++) {
            endTriggers[i].TearDown();
        }
        endTriggers.Clear();

        //Tear down recoveries
        //for (int i = 0; i < recoveryMethods.Count; i++) {
        //    recoveryMethods[i].TearDown();
        //}
        //recoveryMethods.Clear();

        //for (int i = 0; i < recoveryStatListeners.Count; i++) {
        //    recoveryStats.RemoveStatListener(StatName.AbilityCharge, recoveryStatListeners[i]);
        //}


        //Tear down counters
        if (activationCounter != null)
            activationCounter.TearDown();

        if (endCounter != null)
            endCounter.TearDown();

        RemoveEffectRiderEvents();
    }

    public void EndAllGlobalEffects() {
        int count = endTriggers.Count;
        for (int i = 0; i < count; i++) {


            //This doesn't make sense. Why is this in a for loop? End all effects multiple times?
            ForceEndTrigger(null);
        }

        IsActive = false;
    }

    #endregion

    #region CHILD ABILITIES

    public void AddChildAbility(Ability ability) {
        ChildAbilities.Add(ability);
        ability.ParentAbility = this;
    }

    public Ability AddChildAbility(AbilityData data) {
        Ability newChild = AbilityFactory.CreateAbility(data, Source);
        AddChildAbility(newChild);

        if (IsEquipped == true) {
            newChild.Equip();
        }

        return newChild;
    }

    public Ability AddChildAbility(AbilityDefinition abilityDef) {
        return AddChildAbility(abilityDef.AbilityData);
    }

    public void RemoveChildAbility(Ability ability) {
        if (ChildAbilities.RemoveIfContains(ability) == true) {
            if (ability.IsEquipped)
                ability.Uneqeuip();
        }
    }



    #endregion

    #region RECOVERY CHARGES

    public bool TrySpendCharge(int chargesToSpend = 1) {
        if (HasRecovery == false) {
            //Debug.Log(Data.abilityName + " has no recovery");
            return true;
        }

        if (Charges == 0) {
            return false;
        }


        float difference = Charges - chargesToSpend;
        if (difference < 0)
            return false;

        SpendCharge(chargesToSpend);
        return true;
    }


    public void RecoveryCharge(int chargesToRecover) {
        if (HasRecovery == false)
            return;

        //if (IsEquipped == false)
        //    return;

        Stats.AdjustStatRangeCurrentValue(StatName.AbilityCharge, chargesToRecover, StatModType.Flat, this);


        //recoveryStats.AdjustStatRangeCurrentValue(StatName.AbilityCharge, chargesToRecover, StatModType.Flat, this);
        //Debug.Log(Data.abilityName + " recovered a charge");
    }

    public void SpendCharge(int chargesToSpend = 1) {
        if (HasRecovery == false)
            return;

        Stats.AdjustStatRangeCurrentValue(StatName.AbilityCharge, -chargesToSpend, StatModType.Flat, this);
        //recoveryStats.AdjustStatRangeCurrentValue(StatName.AbilityCharge, -chargesToSpend, StatModType.Flat, this);
        //Debug.Log(Data.abilityName + " spent a charge");
    }

    public void AddMaxCharge(StatModifier mod) {
        if (HasRecovery == false)
            return;

        Stats.AddMaxValueModifier(StatName.AbilityCharge, mod);
    }

    public void RemoveMaxCharge(StatModifier mod) {
        if (HasRecovery == false)
            return;

        Stats.RemoveMaxValueModifier(StatName.AbilityCharge, mod);
    }

    public void AddChargesChangedListener(Action<BaseStat, object, float> callback) {
        Stats.AddStatListener(StatName.AbilityCharge, callback);
        recoveryStatListeners.Add(callback);
    }

    public void RemoveChargesChangedListener(Action<BaseStat, object, float> callback) {
        Stats.RemoveStatListener(StatName.AbilityCharge, callback);
        recoveryStatListeners.Remove(callback);
    }

    public void AddCooldownModifier(StatModifier mod) {

    }

    #endregion

    #region EVENTS




    #endregion


    #region HELPERS

    public Effect GetEffectByName(string name) {
        for (int i = 0; i < effects.Count; i++) {
            if (effects[i].Data.effectName == name)
                return effects[i];
        }

        return null;
    }

    public List<Effect> GetAllEffects() {
        return effects;
    }

    //public float GetDamageEffectRatio() {

    //    float result = -1f;

    //    for (int i = 0; i < effects.Count; i++) {
    //        if (effects[i] is StatAdjustmentEffect) {
    //            StatAdjustmentEffect targetEffect = effects[i] as StatAdjustmentEffect;

    //            if (targetEffect.Data.effectDesignation == StatModifierData.StatModDesignation.PrimaryDamage) {
    //                result = targetEffect.GetBaseWeaponPercent();
    //                break;
    //            }
    //        }
    //    }

    //    return result;
    //}

    public float GetWeaponDamageScaler() {
        float result = -1f;

        for (int i = 0; i < effects.Count; i++) {
            if (effects[i] is StatAdjustmentEffect) {
                StatAdjustmentEffect targetEffect = effects[i] as StatAdjustmentEffect;

                if (targetEffect.Data.effectDesignation == StatModifierData.StatModDesignation.PrimaryDamage) {
                    result = targetEffect.GetWeaponScaler();
                    break;
                }
            }
        }

        return result;
    }

    public List<Entity> GetMostRecentValidTargetsFromEffect(string effectName) {
        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            if (effects[i].Data.effectName == effectName)
                return effects[i].ValidTargets;
        }

        return null;
    }

    protected AbilityRecoveryCooldown GetCooldownRecovery() {
        if (HasRecovery == false)
            return null;

        for (int i = 0; i < recoveryMethods.Count; i++) {
            if (recoveryMethods[i] is AbilityRecoveryCooldown) {
                return recoveryMethods[i] as AbilityRecoveryCooldown;
            }
        }

        return null;
    }

    public float GetCooldownRatio() {
        AbilityRecoveryCooldown cooldown = GetCooldownRecovery();
        if (cooldown != null) {
            return cooldown.Ratio;
        }

        return 0f;
    }

    public bool CheckReady() {
        if (HasRecovery == false)
            return true;


        return Charges > 0;
    }

    public float GetDuration() {
        for (int i = 0; i < endTriggers.Count; i++) {
            if (endTriggers[i].Type == TriggerType.Timed) {
                float duration = endTriggers[i].Data.triggerTimerDuration;

                return duration;
            }
        }

        return -1f;
    }

    public float GetAbilityOverloadChance() {
        float sourceChance = Source.Stats[StatName.OverloadChance];
        float skillChance = Stats[StatName.OverloadChance];

        float totalChance = sourceChance + skillChance;

        return totalChance;
    }

    public string GetTooltip() {

        if (Data.ignoreTooltip == true) {
            return "";
        }


        StringBuilder builder = new StringBuilder();

        if (Tags.Count > 0) {

            builder.Append(TextHelper.ColorizeText("[", Color.gray, 20f));

            for (int i = 0; i < Tags.Count; i++) {
                builder.Append(TextHelper.ColorizeText(Tags[i].ToString(), Color.gray, 20f));

                if (i != Tags.Count - 1) {
                    builder.Append(", ");
                }
            }

            builder.Append(TextHelper.ColorizeText("]", Color.gray, 20f));
            builder.AppendLine();
        }


        if(Stats.Contains(StatName.AbilityWindupTime) == true && Stats[StatName.AbilityWindupTime] > 0f) {
            builder.AppendLine("Cast Time: " + TextHelper.ColorizeText(Stats[StatName.AbilityWindupTime].ToString(), Color.yellow) + " Seconds");
        }




        if (string.IsNullOrEmpty(Data.abilityDescription) == false) {
            int targets = GetMaxTargets();

            string replacement = Data.abilityDescription;

            if (targets > 0) {
                replacement = Data.abilityDescription.Replace("{T}", TextHelper.ColorizeText(GetMaxTargets().ToString(), Color.green));

                //builder.Append(replacement).AppendLine();
            }
            //else{
            //    builder.Append(Data.abilityDescription).AppendLine();
            //}

            float size = effects[0].Stats[StatName.EffectSize];

            float globalSizeMod = Source.Stats[StatName.GlobalEffectSizeModifier];

            size *= (1 + globalSizeMod);

            string radiusReplacement = replacement.Replace("{ES}", TextHelper.ColorizeText(size.ToString(), Color.green));

            float lifetime = effects[0].Stats[StatName.ProjectileLifetime];

            string durationReplacment = radiusReplacement.Replace("{D}", TextHelper.ColorizeText(lifetime.ToString(), Color.yellow));

            builder.Append(durationReplacment).AppendLine();
        }




        //float damagePercent = GetWeaponDamageScaler(); //GetDamageEffectRatio();

        if (effects[0] is StatAdjustmentEffect) {

            StatAdjustmentEffect adj = effects[0] as StatAdjustmentEffect;
            if (Data.category != AbilityCategory.Rune) {

                string scalarTooltip = adj.ScalarTooltip();
                if (scalarTooltip == "No Scalers Found") {
                    //Debug.LogWarning("No scalers on: " + Data.abilityName);
                }
                else {
                    builder.AppendLine();
                    builder.AppendLine("Scales From: ");

                    builder.Append(scalarTooltip).AppendLine();
                }

                string projectileStats = effects[0].GetProjectileStatsTooltip();
                if (string.IsNullOrEmpty(projectileStats) == false) {
                    builder.AppendLine(projectileStats);
                }

                if (effects[0].Data.canOverload == true) {
                    float overloadChance = GetAbilityOverloadChance();

                    builder.AppendLine("Overload Chance: " + TextHelper.ColorizeText( TextHelper.FormatStat(StatName.OverloadChance, overloadChance), Color.green));
                }

            }
        }





        float cooldown = GetCooldown();

        if (Data.includeEffectsInTooltip == true) {
            builder.AppendLine();
            
            foreach (Effect effect in effects) {

                string effectTooltip = effect.GetTooltip();

                if (string.IsNullOrEmpty(effectTooltip) == false) {

                    builder.Append(effect.GetTooltip()).AppendLine();

                    //if (cooldown > 0f)
                    //    builder.Append(effect.GetTooltip()).AppendLine();
                    //else
                    //    builder.Append(effect.GetTooltip());
                }
            }
        }


        if (Stats.Contains(StatName.EssenceCost)) {

            if (Stats[StatName.EssenceCost] > 0) {
                builder.AppendLine("Cost: " + TextHelper.ColorizeText(Stats[StatName.EssenceCost].ToString(), Color.cyan) + " Essence");
            }
            else{
                builder.AppendLine("Generates: " + TextHelper.ColorizeText(Mathf.Abs(Stats[StatName.EssenceCost]).ToString(), Color.cyan) + " Essence");

            }


        }


        if (cooldown > 0f) {
            builder.Append("Cooldown: " + TextHelper.ColorizeText(TextHelper.RoundTimeToPlaces(cooldown, 2), Color.yellow)).Append(" Seconds").AppendLine();
        }

        //if(ChildAbilities.Count > 0) {
        //    builder.AppendLine("Attached Abilities: ");
        //}


        //for (int i = 0; i < ChildAbilities.Count; i++) {
        //    builder.Append(ChildAbilities[i].GetTooltip());

        //    if(i != ChildAbilities.Count  -1)
        //        builder.AppendLine();
        //}


        builder.Append(GetRunesTooltip());

        return builder.ToString();
    }

    public List<Ability> GetRunes() {
        //return Source.AbilityManager.GetRuneAbilities(Data.abilityName);

        List<Ability> runeAbilities = new List<Ability>();

        for (int i = 0; i < equippedRunes.Count; i++) {
            runeAbilities.AddRange(equippedRunes[i].Abilities);
        }

        return runeAbilities;

    }

    public string GetRunesTooltip() {
        StringBuilder builder = new StringBuilder();

        List<Ability> runes = GetRunes();

        if (runes.Count > 0) {
            builder.AppendLine();
        }

        for (int i = 0; i < runes.Count; i++) {
            //Debug.Log("Found a Rune: " + runes[i].Data.abilityName + " on " + Data.abilityName);
            builder.Append(TextHelper.ColorizeText("Rune: ", Color.cyan)).AppendLine(runes[i].Data.abilityName);

            //if(i != runes.Count - 1) {
            //    builder.AppendLine();
            //}

            //builder.Append(runes[i].GetTooltip());
            for (int j = 0; j < runes[i].effects.Count; j++) {
                builder.Append(runes[i].effects[j].GetTooltip()).AppendLine();

                //if(j != runes[i].effects.Count - 1) {
                //    builder.AppendLine();
                //}
            }
        }

        return builder.ToString();
    }

    public static string GetRunesTooltip(List<Ability> runes) {
        StringBuilder builder = new StringBuilder();

        if (runes.Count > 0) {
            builder.AppendLine();
        }

        for (int i = 0; i < runes.Count; i++) {
            //Debug.Log("Found a Rune: " + runes[i].Data.abilityName + " on " + Data.abilityName);
            builder.Append(TextHelper.ColorizeText("Rune: ", Color.cyan)).Append(runes[i].Data.abilityName).AppendLine();
            //builder.Append(runes[i].GetTooltip());
            for (int j = 0; j < runes[i].effects.Count; j++) {
                builder.Append(runes[i].effects[j].GetTooltip()).AppendLine();
            }
        }

        return builder.ToString();
    }

    public float GetCooldown() {


        if (Stats.Contains(StatName.Cooldown) == true) {
            return Stats[StatName.Cooldown];
        }

        //AbilityRecoveryCooldown cooldown = GetCooldownRecovery();
        //if (cooldown != null) {
        //    return cooldown.Cooldown;
        //}

        return -1f;
    }

    public int GetMaxTargets() {
        return (int)effects[0].Stats[StatName.EffectMaxTargets]; ;
    }

    public bool HasAutoFire() {
        bool manual = false;

        for (int i = 0; i < activationTriggers.Count; i++) {
            if (activationTriggers[i].Type == TriggerType.UserActivated) {
                manual = true;
                break;
            }
        }
        return manual == true && Data.autoFire == true;
    }

    //public List<Entity> GetFirstEffectTargets() {
    //    if (effects.Count == 0) {
    //        Debug.LogError("No effects found on: " + Data.abilityName + " while trying to preview effect targets");
    //        return null;
    //    }

    //    return effects[0].GetPreviewTargets();

    //}

    #endregion

    #region ACTIVATION

    public void ReceiveStartActivationInstance(TriggerInstance activationInstance) {



        if (IsReady == false) {
            //Debug.Log("An ability: " + Data.abilityName + " tried to trigger, but is not ready.");
            //Debug.Log(Charges + " charges found");
            return;
        }

        if (IsEquipped == false) {
            Debug.Log("An ability: " + Data.abilityName + " tried to trigger, but is not equipped.");
            return;
        }

        if(Stats.Contains(StatName.AbilityWindupTime) && Stats[StatName.AbilityWindupTime] > 0f) {

            if (CheckCost() == false)
                return;

            if (currentWindup == null) {
                currentWindup = new Task(StartAbilityWindup(activationInstance));
                return;
            }
            else {
                //Debug.LogWarning(Data.abilityName + " is mid windup and cannot trigger again");
                return;
            }
        }

        if (TrySpendCharge(1) == false) {
            //Debug.LogWarning("Not enough charges on: " + Data.abilityName);
            return;
        }


        if (CheckCost() == false)
            return;


        if (activationCounter != null && activationCounter.Evaluate() == false) {
            //Debug.LogWarning(activationCounter.Count + " is not enough triggers for " + Data.abilityName);
            return;
        }

        //Debug.Log(TextHelper.ColorizeText( "An ability: " + Data.abilityName + " is starting. Source: " + Source.gameObject.name, Color.green));
        IsActive = true;
        
        SendAbilityInitiatedEvent(activationInstance);

        TriggerAllEffectsInstantly(activationInstance);

        //new Task(TriggerAllEffectsWithDelay(activationInstance));

    }

    protected void SendAbilityResolvedEvent(TriggerInstance triggerInstance) {
        EventData data = new EventData();
        data.AddAbility("Ability", this);
        data.AddEntity("Source", Source);

        EventManager.SendEvent(GameEvent.AbilityResolved, data);
    }

    protected void SendAbilityInitiatedEvent(TriggerInstance triggerInstance) {
        EventData data = new EventData();
        data.AddAbility("Ability", this);
        data.AddEntity("Source", Source);

        EventManager.SendEvent(GameEvent.AbilityInitiated, data);
    }

    private bool CheckCost() {
        if (Stats.Contains(StatName.EssenceCost) && Stats[StatName.EssenceCost] != 0f) {
            //Debug.Log("Cost: " + Stats[StatName.EssenceCost]);

            if (EntityManager.ActivePlayer.TrySpendEssence(Stats[StatName.EssenceCost]) == false) {
                //Debug.LogWarning("Not enough essence");
                return false;
            }
        }

        return true;
    }

    private void ResumeActivation(TriggerInstance activationInstance) {

        if(Source == null || Source.IsDead == true) {
            //Debug.LogWarning("The Source of an Ability: " + Data.abilityName + " is dead or null when resolving a cast time.");
            currentWindup = null;
            return;
        }


        if (TrySpendCharge(1) == false) {
            currentWindup = null;
            return;
        }

        //if (CheckCost() == false) {
        //    currentWindup = null;
        //    return;
        //}


        IsActive = true;

        //new Task(TriggerAllEffectsWithDelay(activationInstance));
        TriggerAllEffectsInstantly(activationInstance);
        
        currentWindup = null;
    }

    public IEnumerator StartAbilityWindup(TriggerInstance activationInstance) {
        WaitForSeconds waiter = new WaitForSeconds(Stats[StatName.AbilityWindupTime]);

        //Debug.Log("Showing some kind of vfx");

        if(Source == null)
            yield break;

        GameObject activeVFX = GameObject.Instantiate(Data.windupVFX, Source.transform);
        activeVFX.transform.localPosition = Vector3.zero;
        GameObject.Destroy(activeVFX, 3f);

        yield return waiter;

        ResumeActivation(activationInstance);
    }

    public void RecieveEndActivationInstance(TriggerInstance endInstance) {

        if (endCounter != null && endCounter.Evaluate() == false) {
            //Debug.LogError(Counter.Count + " is not enough triggers for " + abilityName);
            return;
        }

        IsActive = false;
        //new Task(EndAllEffectsWithDelay(endInstance));
        EndAllEffectsInstantly(endInstance);

    }


    protected void TriggerAllEffectsInstantly(TriggerInstance activationInstance = null) {
        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            effects[i].ReceiveStartActivationInstance(activationInstance);
        }
        //Debug.Log(TextHelper.ColorizeText( "An ability: " + Data.abilityName + " has resolved. Source: " + Source.gameObject.name, Color.red));
        SendAbilityResolvedEvent(activationInstance);
    }

    protected IEnumerator TriggerAllEffects(TriggerInstance activationInstance = null) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();

        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            yield return waiter;
            effects[i].ReceiveStartActivationInstance(activationInstance);
        }

        //Debug.Log("An ability: " + Data.abilityName + " has resolved. Source: " + Source.gameObject.name);
        SendAbilityResolvedEvent(activationInstance);
    }

    protected IEnumerator TriggerAllEffectsWithDelay(TriggerInstance activationInstance = null) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();

        yield return waiter;

        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            effects[i].ReceiveStartActivationInstance(activationInstance);
        }

        //Debug.Log("An ability: " + Data.abilityName + " has resolved. Source: " + Source.gameObject.name);
        SendAbilityResolvedEvent(activationInstance);
    }

    protected void EndAllEffectsInstantly(TriggerInstance activationInstance = null) {
        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            effects[i].RecieveEndActivationInstance(activationInstance);
        }
    }

    protected IEnumerator EndAllEffects(TriggerInstance activationInstance = null) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();

        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            yield return waiter;
            effects[i].RecieveEndActivationInstance(activationInstance);
        }
    }

    protected IEnumerator EndAllEffectsWithDelay(TriggerInstance activationInstance = null) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();
        yield return waiter;

        int count = effects.Count;
        for (int i = 0; i < count; i++) {

            effects[i].RecieveEndActivationInstance(activationInstance);
        }
    }

    public void ForceActivate() {
        ReceiveStartActivationInstance(null);
    }

    public void NPCActivation() {

    }

    public void ForceEndTrigger(TriggerInstance endInstance) {
        //new Task(EndAllEffectsWithDelay(endInstance));
        EndAllEffectsInstantly(endInstance);

        IsActive = false;
    }

    #endregion

}
