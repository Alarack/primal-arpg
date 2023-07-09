using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using TriggerInstance = AbilityTrigger.TriggerInstance;

public class Ability  {

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


    protected List<AbilityTrigger> activationTriggers = new List<AbilityTrigger>();
    protected List<AbilityTrigger> endTriggers = new List<AbilityTrigger>();

    protected TriggerActivationCounter activationCounter;
    protected TriggerActivationCounter endCounter;


    protected List<Effect> effects = new List<Effect>();
    protected List<AbilityRecovery> recoveryMethods = new List<AbilityRecovery>();

    //protected StatCollection recoveryStats;
    protected List<Action<BaseStat, object, float>> recoveryStatListeners = new List<Action<BaseStat, object, float>>();

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

    }

    #region SETUP AND TEAR DOWN


    private void SetupStats() {
        Stats = new StatCollection(this);

        StatRange charges = new StatRange(StatName.AbilityCharge, 0, Data.startingRecoveryCharges, Data.startingRecoveryCharges);
        SimpleStat runeSlots = new SimpleStat(StatName.AbilityRuneSlots, Data.baseRuneSlots);
        Stats.AddStat(charges);
        Stats.AddStat(runeSlots);

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
    }

    public void Uneqeuip() {
        //Debug.Log("Unequipping: " + Data.abilityName);
        if (IsEquipped == false) {
            Debug.LogError("Tried to unequip " + Data.abilityName + " but it wasn't equipped");
            return;
        }

        EventData data = new EventData();
        data.AddAbility("Ability", this);

        EventManager.SendEvent(GameEvent.AbilityUnequipped, data);

        UnregisterAbility();
        TearDown();
        IsEquipped = false;
    }

    private void RegisterAbility() {
        List<Ability> list = Data.category switch {
            //AbilityCategory.ActiveSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            //AbilityCategory.KnownSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            AbilityCategory.Item => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Item],
            AbilityCategory.Rune => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Rune],
            _ => null,
        };

        if (list != null) {
            list.Add(this);

        }

    }

    private void UnregisterAbility() {
        List<Ability> list = Data.category switch {
            //AbilityCategory.ActiveSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            //AbilityCategory.KnownSkill => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.KnownSkill],
            AbilityCategory.Item => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Item],
            AbilityCategory.Rune => EntityManager.ActivePlayer.AbilityManager[AbilityCategory.Rune],
            _ => null,
        };

        if (list != null) {
            list.Remove(this);
        }
    }

    protected void SetupActivationTriggers() {
        foreach (TriggerData triggerData in Data.activationTriggerData) {

            AbilityTrigger trigger = AbilityFactory.CreateAbilityTrigger(triggerData, Source, this);
            trigger.ActivationCallback = ReceiveStartActivationInstance;
            activationTriggers.Add(trigger);
        }

        AutoFire = HasAutoFire();
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

    public float GetDamageEffectRatio() {

        float result = -1f;

        for (int i = 0; i < effects.Count; i++) {
            if (effects[i] is StatAdjustmentEffect) {
                StatAdjustmentEffect targetEffect = effects[i] as StatAdjustmentEffect;

                if (targetEffect.Data.effectDesignation == StatModifierData.StatModDesignation.PrimaryDamage) {
                    result = targetEffect.GetBaseWeaponPercent();
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

    public string GetTooltip() {

        StringBuilder builder = new StringBuilder();



        builder.Append(Data.abilityDescription).AppendLine();




        float damagePercent = GetDamageEffectRatio();

        if (damagePercent > 0f) {
            builder.Append("Damage: " + TextHelper.ColorizeText((damagePercent * 100).ToString() + "%", Color.green) + " of Weapon Damage").AppendLine();
        }


        float cooldown = GetCooldown();
        if (cooldown > 0f) {
            builder.Append("Cooldown: " + TextHelper.RoundTimeToPlaces(cooldown, 2)).Append(" Seconds").AppendLine();
        }

        //if (Data.category == AbilityCategory.Rune) {

        //    for (int i = 0; i < effects.Count; i++) {
        //        builder.Append(effects[i].GetTooltip());   
        //    }

        //}


        builder.Append(GetRunesTooltip());

        //List<Ability> runes = GetRunes();

        //if(runes.Count > 0) {
        //    builder.AppendLine();
        //}

        //for (int i = 0; i < runes.Count; i++) {
        //    //Debug.Log("Found a Rune: " + runes[i].Data.abilityName + " on " + Data.abilityName);
        //    builder.Append(TextHelper.ColorizeText("Rune: ", Color.cyan)).Append(runes[i].Data.abilityName).AppendLine();
        //    //builder.Append(runes[i].GetTooltip());
        //    for (int j = 0; j < runes[i].effects.Count; j++) {
        //        builder.Append(runes[i].effects[j].GetTooltip()).AppendLine();
        //    }
        //}


        return builder.ToString();
    }

    public List<Ability> GetRunes() {
        return Source.AbilityManager.GetRuneAbilities(Data.abilityName);
    }

    public string GetRunesTooltip() {
        StringBuilder builder = new StringBuilder();

        List<Ability> runes = GetRunes();

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

        if (activationCounter != null && activationCounter.Evaluate() == false) {
            //Debug.LogWarning(Counter.Count + " is not enough triggers for " + abilityName);
            return;
        }

        if (IsReady == false) {
            //Debug.Log("An ability: " + Data.abilityName + " tried to trigger, but is not ready.");
            //Debug.Log(Charges + " charges found");
            return;
        }

        if (TrySpendCharge(1) == false) {
            //Debug.LogWarning("Not enough charges on: " + Data.abilityName);
            return;
        }


        //Debug.Log("An ability: " + Data.abilityName + " is starting. Source: " + Source.gameObject.name);

        IsActive = true;

        new Task(TriggerAllEffects(activationInstance));

    }


    public void RecieveEndActivationInstance(TriggerInstance endInstance) {

        if (endCounter != null && endCounter.Evaluate() == false) {
            //Debug.LogError(Counter.Count + " is not enough triggers for " + abilityName);
            return;
        }

        IsActive = false;
        new Task(EndAllEffects(endInstance));

    }

    protected IEnumerator TriggerAllEffects(TriggerInstance activationInstance = null) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();

        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            yield return waiter;
            effects[i].ReceiveStartActivationInstance(activationInstance);
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

    public void ForceActivate() {
        ReceiveStartActivationInstance(null);
    }

    public void ForceEndTrigger(TriggerInstance endInstance) {
        new Task(EndAllEffects(endInstance));

        IsActive = false;
    }

    #endregion

}
