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
    public bool IsChanneled { get { return Tags.Contains(AbilityTag.Channeled); } }
    public bool IsEquipped { get; protected set; }
    public bool IgnoreOtherCasting { get; protected set; }
    public bool Locked { get; set; }

    public Vector2 LastPayloadLocation { get; set; } = Vector2.zero;

    //Recovery Stuff
    public bool IsReady { get { return CheckReady(); } }
    public bool IsCasting { get { return currentWindup != null; } }
    public bool HasRecovery { get { return recoveryMethods.Count > 0; } }
    public int Charges { get { return Mathf.FloorToInt(Stats[StatName.AbilityCharge]); } }
    public int MaxCharges { get { return Mathf.FloorToInt(Stats.GetStatRangeMaxValue(StatName.AbilityCharge)); } }
    public int RuneSlots { get { return Mathf.FloorToInt(Stats[StatName.AbilityRuneSlots]); } }

    public int AbilityLevel { get; protected set; } = 1;

    public List<Item> equippedRunes = new List<Item>();
    public Dictionary<int, List<Item>> runeItemsByTier = new Dictionary<int, List<Item>>();


    protected List<AbilityTrigger> activationTriggers = new List<AbilityTrigger>();
    protected List<AbilityTrigger> endTriggers = new List<AbilityTrigger>();

    protected TriggerActivationCounter activationCounter;
    protected TriggerActivationCounter endCounter;


    protected List<Effect> effects = new List<Effect>();
    protected List<AbilityRecovery> recoveryMethods = new List<AbilityRecovery>();

    protected List<Action<BaseStat, object, float>> recoveryStatListeners = new List<Action<BaseStat, object, float>>();

    protected Task currentWindup;
    protected GameObject currentWindupVFX;
    protected Timer channelingCostTimer;

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

        if(data.category == AbilityCategory.KnownSkill || data.category == AbilityCategory.PassiveSkill) {
            Locked = true;
        }

        if(data.startingAbility == true) {
            Locked = false;
        }

        SetupStats();
        SetupRecoveries();
        SetupEffects();

        SetupChildAbilities();
        SetupRuneItems();

        SetupIgnoreCasting();
        SetupChannelTimer();

    }

    #region SETUP AND TEAR DOWN

    private void SetupChannelTimer() {
        if(IsChanneled == true) {
            float channelInterval = Stats.Contains(StatName.ChannelInterval) ? Stats[StatName.ChannelInterval] : 1f;
            
            channelingCostTimer = new Timer(channelInterval, ApplyCost, true);

            EventManager.RegisterListener(GameEvent.AbilityStatAdjusted, OnStatChanged);
        }
    }

    private void SetupIgnoreCasting() {
        
        if(Data.ignoreOtherCasting == true) {
            SetIgnoreCastings(true);
            return;
        }
        
        if(Data.HasManualActivation() == true) {
            SetIgnoreCastings(false);
            //Debug.Log(Data.abilityName + " has manual castings");
        }
        else {
            SetIgnoreCastings(true);
            //Debug.Log(Data.abilityName + " has NO manual castings");
        }
        

    }

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
    }

    private void SetupChildAbilities() {
        for (int i = 0; i < Data.childAbilities.Count; i++) {
            AddChildAbility(Data.childAbilities[i]);
        }
    }

    private void SetupRuneItems() {
        foreach (AbilityRuneGroupData runeDataGroup in Data.runeGroupData) {
            foreach (ItemDefinition runedata in runeDataGroup.runes) {
                Item runeItem = ItemFactory.CreateItem(runedata.itemData, EntityManager.ActivePlayer);

                if(runeItemsByTier.TryGetValue(runeDataGroup.tier, out List<Item> runeItems) == true) {
                    runeItemsByTier[runeDataGroup.tier].Add(runeItem);
                }
                else {
                    runeItemsByTier.Add(runeDataGroup.tier, new List<Item> { runeItem });
                }
            }
        }
    }

    public void Equip() {
        //Debug.Log("Equipping: " + Data.abilityName);
        if (IsEquipped == true) {
            Debug.LogError("Tried to equip " + Data.abilityName + " but it was alread equipped");
            return;
        }

        //Debug.Log("Equipping: " + Data.abilityName);

        SetupActivationTriggers();
        SetupEndTriggers();
        SetupTriggerCounters();
        SetupChannelTimer();
        //SetupRecoveries();
        RegisterAbility();
        IsEquipped = true;


        for (int i = 0; i < equippedRunes.Count; i++) {
            equippedRunes[i].ReactivateEquippedRunes();
        }

        EventData data = new EventData();
        data.AddAbility("Ability", this);

        EventManager.SendEvent(GameEvent.AbilityEquipped, data);

        for (int i = 0; i < ChildAbilities.Count; i++) {
            ChildAbilities[i].Equip();
        }

        if(Data.category == AbilityCategory.PassiveSkill || Data.category == AbilityCategory.Item)
            IsActive = true;

        if (IsChanneled == true)
            TimerManager.AddTimerAction(HandleChannelingCost);
    }

    public void Uneqeuip() {

        //Debug.Log("Unequipping: " + Data.abilityName);

        if (IsEquipped == false) {
            Debug.LogWarning("Tried to unequip " + Data.abilityName + " but it wasn't equipped");
            return;
        }

        for (int i = 0; i < equippedRunes.Count; i++) {
            equippedRunes[i].DeactivateEquippedRunes();
        }

        EventData data = new EventData();
        data.AddAbility("Ability", this);

        EventManager.SendEvent(GameEvent.AbilityUnequipped, data);

        UnregisterAbility();
        TearDown();
        IsEquipped = false;
        IsActive = false;

        for (int i = 0; i < ChildAbilities.Count; i++) {
            ChildAbilities[i].Uneqeuip();
        }

        if (IsChanneled == true)
            TimerManager.RemoveTimerAction(HandleChannelingCost);

        
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

        SetupEffectEvents();
    }

    protected void SetupEffectEvents() {
        for (int i = 0; i < effects.Count; i++) {
            effects[i].RegisterEvents();
        }
    }

    protected void RemoveEffectEvents() {
        for (int i = 0; i < effects.Count; i++) {
            effects[i].UnregisterEvents();
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
        AbortAbilityWindup();


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

        RemoveEffectEvents();

        EventManager.RemoveMyListeners(this);
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

    #region CHILD ABILITIES / ADD EFFECTS

    public void AddChildAbility(Ability ability) {
        ChildAbilities.Add(ability);
        ability.ParentAbility = this;

        //Debug.Log("Adding " + ability.Data.abilityName + " as a child to " + Data.abilityName);
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

    public void AddEffect(Effect newEffect) {
        effects.Add(newEffect);

        //Debug.LogWarning("Adding a new effect: " + newEffect.Data.effectName + " to " + Data.abilityName);
        //newEffect.InheritStatsFromParentAbility(this);
    }

    public void RemoveEffect(Effect target) {
        effects.RemoveIfContains(target);
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

    private void OnStatChanged(EventData data) {
        Ability target = data.GetAbility("Ability");
        StatName stat = (StatName)data.GetInt("Stat");
        if (target != this)
            return;

        if(stat == StatName.ChannelInterval && Tags.Contains(AbilityTag.Channeled)) {
            channelingCostTimer.SetDuration(Stats[StatName.ChannelInterval]);
        }

    }

    private void OnTagAdded(EventData data) {
        AbilityTag tag = (AbilityTag)data.GetInt("Tag");
    }

    private void OnTagRemoved(EventData data) {

    }

    public void SendTagAddedEvent(AbilityTag tagAdded, Ability cause = null) {
        EventData data = new EventData();
        data.AddInt("Tag", (int)tagAdded);
        data.AddAbility("Cause", cause);
        data.AddAbility("Ability", this);


        EventManager.SendEvent(GameEvent.TagAdded, data);
    }

    public void SendTagRemovedEvent(AbilityTag tagRemoved, Ability cause = null) {
        EventData data = new EventData();
        data.AddInt("Tag", (int)tagRemoved);
        data.AddAbility("Cause", cause);
        data.AddAbility("Ability", this);


        EventManager.SendEvent(GameEvent.TagRemoved, data);
    }

    #endregion

    #region TAGS

    public void AddTag(AbilityTag tag, Ability cause = null) {
    
        if(Tags.AddUnique(tag) == false) {
            Debug.LogError("Tried To add a tag: " + tag + " to " + Data.abilityName + ", but it was already present");
            return;
        }

        if(tag == AbilityTag.Channeled) {
            IsActive = false;
            SetupChannelTimer();
        }


        SendTagAddedEvent(tag, cause);
    }

    public void RemoveTag(AbilityTag tag, Ability cause = null) {
        if (Tags.RemoveIfContains(tag) == false) {
            Debug.LogError("Tried To remove a tag: " + tag + " from " + Data.abilityName + ", but it wasn't present");
            return;
        }

        if(tag == AbilityTag.Channeled) {
            if (channelingCostTimer != null)
                channelingCostTimer = null;
        }

        SendTagRemovedEvent(tag, cause);
    }

    #endregion  

    #region OPTIONS

    public void SetIgnoreCastings(bool ignoreCastings) { 
        this.IgnoreOtherCasting = ignoreCastings;
    }



    #endregion

    #region LEVELING

    public void LevelUp() {
        AbilityLevel++;
    }

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

    public void ModifyCooldownElasped(float amount) {
        AbilityRecoveryCooldown cooldownRecovery = GetCooldownRecovery();
        if(cooldownRecovery == null) 
            return;

        cooldownRecovery.ModifiyCooldownElapsed(amount);
    }

    public float GetCooldownRatio() {
        AbilityRecoveryCooldown cooldown = GetCooldownRecovery();
        if (cooldown != null) {
            return cooldown.Ratio;
        }

        return 0f;
    }

    public float GetTotalEssenceCost() {
        if(Stats.Contains(StatName.EssenceCost) == false) 
            return 0f;

        if (Stats[StatName.EssenceCost] < 0f) {
            return Stats[StatName.EssenceCost]; 
        }

        float cost = Stats[StatName.EssenceCost] * (1f + Source.Stats[StatName.GlobalEssenceCostModifier]);

        //Debug.Log("Total cost for: " + Data.abilityName + " : " + cost);

        return cost;
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

    public string GetTooltip(bool showTags = true, bool addLine = false) {

        if (Data.ignoreTooltip == true) {
            return "";
        }


        StringBuilder builder = new StringBuilder();

        if (Tags.Count > 0 && showTags == true) {

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

            //Debug.Log("Global projectile life: " + Source.EntityName + " :: " + Source.Stats[StatName.GlobalProjectileLifetimeModifier]);

            float lifetime = effects[0].Stats[StatName.ProjectileLifetime] * (1f + Source.Stats[StatName.GlobalProjectileLifetimeModifier]);

            string durationReplacment = radiusReplacement.Replace("{D}", TextHelper.ColorizeText(lifetime.ToString(), Color.yellow));

            int shotCount = (int)effects[0].Stats[StatName.ShotCount];

            string shotCountReplacement = durationReplacment.Replace("{SC}", TextHelper.ColorizeText(shotCount.ToString(), Color.green));

            int chainCount = (int)effects[0].Stats[StatName.ProjectileChainCount];

            string chainCountReplacement = shotCountReplacement.Replace("{CC}", TextHelper.ColorizeText(chainCount.ToString(), Color.green));



            float procChance = Stats[StatName.ProcChance];

            string procReplacement = chainCountReplacement.Replace("{PR}", TextHelper.FormatStat(StatName.ProcChance, procChance));

            float statusDuration = Stats[StatName.StatusLifetime] * (1 + Source.Stats[StatName.GlobalStatusDurationModifier]);

            //float statusLife = Stats[StatName.StatusLifetime] > 0f ? Stats[StatName.StatusLifetime] : -1f;
            string statusLifeText = statusDuration > 0 ? TextHelper.ColorizeText(statusDuration.ToString(), Color.yellow) : TextHelper.ColorizeText( "Infintie", Color.yellow);
            string statusLifeReplacement = procReplacement.Replace("{SL}", statusLifeText);


            float timerInterval = GetTimerTriggerInterval();

            string timerInteralText = TextHelper.ColorizeText(timerInterval.ToString(), Color.yellow);
            string timerIntervalReplacment = statusLifeReplacement.Replace("{TI}", timerInteralText);

            builder.Append(timerIntervalReplacment);

            if(Data.showChildAbilitiesInTooltip == false) {
                builder.AppendLine();
            }
        }

        if (Data.showChildAbilitiesInTooltip == true) {
            for (int i = 0; i < ChildAbilities.Count; i++) {
                builder.Append(ChildAbilities[i].GetTooltip(false, true));

                if (i != ChildAbilities.Count - 1)
                    builder.AppendLine();
            }
        }

        if (effects[0] is StatAdjustmentEffect) {

            StatAdjustmentEffect adj = effects[0] as StatAdjustmentEffect;
            if (Data.category != AbilityCategory.Rune) {

                //Debug.Log("Ability tooltip for an effect: " + effects[0].Data.effectName);

                string scalarTooltip = adj.ScalarTooltip();
                if (scalarTooltip == "No Scalers Found") {
                    //Debug.LogWarning("No scalers on: " + Data.abilityName);
                    
                    if(string.IsNullOrEmpty(Data.abilityDescription) == false || addLine == true) {
                        Debug.LogWarning("Instering 2 blank lines for: " + Data.abilityName);
                        builder.AppendLine().AppendLine();
                    }

                    Debug.LogWarning("Instering another blank lines for: " + adj.Data.effectName);

                    builder.AppendLine(adj.GetTooltip());
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

                if(adj.Data.showRiderTooltip == true) {
                    for (int i = 0; i < adj.RiderEffects.Count; i++) {
                        builder.AppendLine(adj.RiderEffects[i].GetTooltip());
                    }
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

                if (effect.Data.onlyShowTooltipInRune == true) {
                    continue;
                }

                string effectTooltip = effect.GetTooltip();
                if (string.IsNullOrEmpty(effectTooltip) == false) {
                    builder.Append(effectTooltip).AppendLine();
                }
            }
        }

        if (Stats.Contains(StatName.EssenceCost)) {

            float totalCost = GetTotalEssenceCost();

            if (totalCost > 0) {
                builder.AppendLine("Cost: " + TextHelper.ColorizeText(totalCost.ToString(), Color.cyan) + " Essence");
            }
            else if (totalCost < 0){
                builder.AppendLine("Generates: " + TextHelper.ColorizeText(Mathf.Abs(totalCost).ToString(), Color.cyan) + " Essence");

            }
        }


        if (cooldown > 0.01f) {
            builder.Append("Cooldown: " + TextHelper.ColorizeText(TextHelper.RoundTimeToPlaces(cooldown, 2), Color.yellow)).Append(" Seconds").AppendLine();
        }

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

            //for (int j = 0; j < runes[i].effects.Count; j++) {
            //    string effectTooltip = runes[i].effects[j].GetTooltip();

            //    if(string.IsNullOrEmpty(effectTooltip) == false)
            //        builder.Append(effectTooltip).AppendLine();
            //}
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
            
            //Debug.Log("Displaing tooltip for: " + runes[i].effects.Count + " effects on the ability: " + runes[i].Data.abilityName);
            
            for (int j = 0; j < runes[i].effects.Count; j++) {
                //Debug.Log("Getting tooltip for the effect: " + runes[i].effects[j].Data.effectName);
                string effectTooltip = runes[i].effects[j].GetTooltip();

                if(string.IsNullOrEmpty (effectTooltip) == false)
                    builder.Append(effectTooltip).AppendLine();
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

    public float GetTimerTriggerInterval() {
        foreach (AbilityTrigger trigger in activationTriggers) {
            if(trigger is TimedTrigger) {
                TimedTrigger timer = trigger as TimedTrigger;
                return timer.TriggerInterval;
            }
        }

        return -1f;
    }

    public int GetMaxTargets() {
        if(effects.Count == 0) 
            return 0;

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

        if (IsChanneled == true && IsActive == true) {
            return;
        }



        if (IsReady == false) {
            //Debug.Log("An ability: " + Data.abilityName + " tried to trigger, but is not ready.");
            //Debug.Log(Charges + " charges found");
            return;
        }

        if (IsEquipped == false) {
            Debug.Log("An ability: " + Data.abilityName + " tried to trigger, but is not equipped.");
            return;
        }

        if(IgnoreOtherCasting == false) {
            Ability castingAbility = Source.ActivelyCastingAbility;

            if(castingAbility != null) {
                //Debug.LogWarning("Another ability is casting at the moment: " + castingAbility.Data.abilityName);
                return;
            }
        }




        if(Stats.Contains(StatName.AbilityWindupTime) && Stats[StatName.AbilityWindupTime] > 0f) {

            if (CheckCost() == false)
                return;

            if (currentWindup == null) {
                currentWindup = new Task(StartAbilityWindup(activationInstance));
                Source.ActivelyCastingAbility = this;
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

        //if (Source != null && Source.ownerType == OwnerConstraintType.Friendly && Source.subtypes.Contains(Entity.EntitySubtype.Orbital))
        //    Debug.Log(TextHelper.ColorizeText("An ability: " + Data.abilityName + " is starting. Source: " + Source.gameObject.name, Color.green));


        IsActive = true;
        
        SendAbilityInitiatedEvent(activationInstance);

        TriggerAllEffectsInstantly(activationInstance);



        //new Task(TriggerAllEffectsWithDelay(activationInstance));

    }

    protected void ApplyCost(EventData data) {

        if (CheckCost() == false) {
            RecieveEndActivationInstance(null);
        }
        else if (Data.recastOnChannelCost == true) {
            SendAbilityInitiatedEvent(null);
            TriggerAllEffectsInstantly(null);
        }
    }

    protected void HandleChannelingCost() {
        if(IsActive == false || IsChanneled == false) 
            return;

        channelingCostTimer.UpdateClock();
    }

    protected void SendAbilityResolvedEvent(TriggerInstance triggerInstance) {
        EventData data = new EventData();
        data.AddAbility("Ability", this);
        data.AddEntity("Source", Source);

        EventManager.SendEvent(GameEvent.AbilityResolved, data);
    }

    public void SendAbilityEndedEvent(EffectZone zone = null) {

        //Debug.LogWarning("Sending end event for: " + Data.abilityName);

        EventData data = new EventData();
        data.AddAbility("Ability", this);
        data.AddEntity("Source", Source);
        data.AddEntity("EffectZone", zone);

        EventManager.SendEvent(GameEvent.AbilityEnded, data);
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
            if (EntityManager.ActivePlayer.TrySpendEssence(GetTotalEssenceCost()) == false) {
                //Debug.LogWarning("Not enough essence for " + Data.abilityName);
                return false;
            }
        }

        return true;
    }

    private void ResumeActivation(TriggerInstance activationInstance) {

        if(Source == null || Source.IsDead == true) {
            //Debug.LogWarning("The Source of an Ability: " + Data.abilityName + " is dead or null when resolving a cast time.");
            currentWindup = null;
            Source.ActivelyCastingAbility = null;
            return;
        }


        if (TrySpendCharge(1) == false) {
            currentWindup = null;
            Source.ActivelyCastingAbility = null;
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
        Source.ActivelyCastingAbility = null;
    }

    public void AbortAbilityWindup() {
        if(currentWindup != null && currentWindup.Running == true) {
            currentWindup.Stop();
            if(currentWindupVFX != null)
                GameObject.Destroy(currentWindupVFX);

            currentWindup = null;
            Source.ActivelyCastingAbility = null;
        }
    }

    public IEnumerator StartAbilityWindup(TriggerInstance activationInstance) {

        float windupTime = Stats[StatName.AbilityWindupTime];
        float ownerCastSpeed = Source.Stats[StatName.CastSpeedModifier] > 0f ? Source.Stats[StatName.CastSpeedModifier] : 1f;

        if (windupTime <= 0) {
            Debug.LogError("0 Cast time detected on " + Data.abilityName);
            ResumeActivation(activationInstance);
            yield break;
        }

        float waitTime = windupTime / ownerCastSpeed;

        WaitForSeconds waiter = new WaitForSeconds(waitTime);

        //Debug.Log("Winding up: " + Data.abilityName);

        if(Source == null)
            yield break;

        currentWindupVFX = GameObject.Instantiate(Data.windupVFX, Source.transform);
        currentWindupVFX.transform.localPosition = Vector3.zero;
        GameObject.Destroy(currentWindupVFX, 3f);

        yield return waiter;

        ResumeActivation(activationInstance);
    }

    public void RecieveEndActivationInstance(TriggerInstance endInstance) {

        //Debug.LogWarning("Recieveing end event for: " + Data.abilityName);



        if (endCounter != null && endCounter.Evaluate() == false) {
            //Debug.LogError(Counter.Count + " is not enough triggers for " + abilityName);
            return;
        }

        IsActive = false;
        //new Task(EndAllEffectsWithDelay(endInstance));

        if(Tags.Contains(AbilityTag.Channeled) == true) {
            channelingCostTimer.ResetTimer();
        }

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

    //protected IEnumerator TriggerAllEffects(TriggerInstance activationInstance = null) {
    //    WaitForEndOfFrame waiter = new WaitForEndOfFrame();

    //    int count = effects.Count;
    //    for (int i = 0; i < count; i++) {
    //        yield return waiter;
    //        effects[i].ReceiveStartActivationInstance(activationInstance);
    //    }

    //    //Debug.Log("An ability: " + Data.abilityName + " has resolved. Source: " + Source.gameObject.name);
    //    SendAbilityResolvedEvent(activationInstance);
    //}

    //protected IEnumerator TriggerAllEffectsWithDelay(TriggerInstance activationInstance = null) {
    //    WaitForEndOfFrame waiter = new WaitForEndOfFrame();

    //    yield return waiter;

    //    int count = effects.Count;
    //    for (int i = 0; i < count; i++) {
    //        effects[i].ReceiveStartActivationInstance(activationInstance);
    //    }

    //    //Debug.Log("An ability: " + Data.abilityName + " has resolved. Source: " + Source.gameObject.name);
    //    SendAbilityResolvedEvent(activationInstance);
    //}

    protected void EndAllEffectsInstantly(TriggerInstance activationInstance = null) {
        int count = effects.Count;
        for (int i = 0; i < count; i++) {
            effects[i].RecieveEndActivationInstance(activationInstance);
        }

        SendAbilityEndedEvent();
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


    public void SetActive(bool active) {
        IsActive = active;
    }

    public void ForceActivate() {
        //Debug.LogWarning("Force activating: " + Data.abilityName);
        ReceiveStartActivationInstance(null);
    }

    public void ForceEndTrigger(TriggerInstance endInstance) {
        //new Task(EndAllEffectsWithDelay(endInstance));
        EndAllEffectsInstantly(endInstance);

        IsActive = false;
    }

    #endregion

}
