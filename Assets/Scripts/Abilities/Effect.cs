using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static AbilityTrigger;
using static Status;
using Random = UnityEngine.Random;


public abstract class Effect {

    public abstract EffectType Type { get; }
    public EffectTarget Targeting { get; protected set; }
    public Ability ParentAbility { get; protected set; }
    public EffectData Data { get; protected set; }
    public Entity Source { get; protected set; }
    public List<Entity> EntityTargets { get; protected set; } = new List<Entity>();
    public List<Effect> EffectTargets { get; protected set; } = new List<Effect>();
    public List<Ability> AbilityTargets { get; protected set; } = new List<Ability>();
    public List<Entity> ValidTargets { get { return targeter.GatherValidTargets(); } }
    public Entity LastTarget { get; protected set; }

    public Entity PayloadPrefab { get; set; }
    public EffectZone EffectZonePrefab { get { return ZoneInfo.effectZonePrefab != null ? ZoneInfo.effectZonePrefab : null; } }
    public EffectZoneInfo ZoneInfo { get; set; }

    public bool Suppressed { get; set; } = false;

    public List<EffectZone> ActiveEffectZones { get; set; } = new List<EffectZone>();
    public Dictionary<Entity, int> PsudoStacks { get; set; } = new Dictionary<Entity, int>();

    protected TriggerInstance currentTriggerInstance;
    protected List<AbilityConstraint> targetConstraints = new List<AbilityConstraint>();
    protected EffectTargeter targeter;

    protected Projectile activeDelivery;

    public StatCollection Stats { get; protected set; }

    //[System.NonSerialized]
    public List<Effect> RiderEffects { get; protected set; } = new List<Effect>();
    protected Effect parentEffect;

    protected bool isOverloading;

    public Effect(EffectData data, Entity source, Ability parentAbility = null) {
        this.Data = data;
        this.ParentAbility = parentAbility;
        this.Targeting = data.targeting;
        this.Source = source;
        this.PayloadPrefab = data.payloadPrefab;
        //this.EffectZonePrefab = data.effectZoneInfo.effectZonePrefab;
        this.ZoneInfo = data.effectZoneInfo;
        SetupStats();
        SetupTargetConstraints();
        SetupRiderEffects();
        targeter = new EffectTargeter(this);
    }

    protected void SetupStats() {
        StatCollection parentStats = ParentAbility != null ? ParentAbility.Stats : null;
        //Debug.LogWarning("Setting up stats for: " + Data.effectName);
        Stats = new StatCollection(this, Data.payloadStatData, parentStats);
        //Stats.AddMissingStats(Data.stat)



        if (Data.HasStat(StatName.ShotCount) == -1f) {
            SimpleStat effectShotCount = new SimpleStat(StatName.ShotCount, 1f);
            Stats.AddStat(effectShotCount);
        }




        if (Data.HasStat(StatName.FireDelay) == -1f) {
            SimpleStat shotDelay = new SimpleStat(StatName.FireDelay, 0f);
            Stats.AddStat(shotDelay);
        }

        SimpleStat maxTargets = new SimpleStat(StatName.EffectMaxTargets, Data.numberOfTargets);
        //SimpleStat effectRange = new SimpleStat(StatName.EffectRange, Data.)


        Stats.AddStat(maxTargets);

        InheritStatsFromParentAbility();

        //if(Data.effectName == "Sword Guy Swipe Damage") {
        //    Debug.Log("Effect Range: " + Stats[StatName.EffectRange]);
        //}
    }

    public void InheritStatsFromParentAbility() {

        if (ParentAbility == null) {
            //Debug.LogWarning(Data.effectName + " has no parent ability to inherit from");
            return;
        }

        if (Data.inheritStatsFromParentAbility == false) {
            Debug.LogWarning(Data.effectName + " does not inherit stats from it's parent");
            return;
        }

        List<StatName> exceptions = new List<StatName> {
            StatName.Cooldown,
            StatName.AbilityCharge,
            StatName.AbilityRuneSlots,
            StatName.AbilityWeaponCoefficicent,
            StatName.AbilityWindupTime,
            StatName.ProcChance,
            StatName.EssenceCost,
            StatName.OverloadChance,
            StatName.ChannelInterval,

        };


        Stats.AddMissingStats(ParentAbility.Stats, exceptions, ParentAbility.Data.abilityName, Data.effectName);
    }

    protected void SetupTargetConstraints() {
        for (int i = 0; i < Data.targetConstraints.Count; i++) {
            AbilityConstraint constraint = AbilityFactory.CreateAbilityConstraint(Data.targetConstraints[i], Source, ParentAbility);
            constraint.SetParentEffect(this);
            targetConstraints.Add(constraint);
        }
    }

    protected void SetupRiderEffects() {
        for (int i = 0; i < Data.riderEffects.Count; i++) {

            if (Data.riderEffects[i] == null) {
                Debug.LogError("Null Rider in effect data: " + Data.effectName);
                continue;
            }
            
            Effect rider = AbilityFactory.CreateEffect(Data.riderEffects[i].effectData, Source, ParentAbility);
            rider.parentEffect = this;
            RiderEffects.Add(rider);
            //Debug.Log("Creating a rider: " + rider.Data.effectName + " for " + Data.effectName);
        }
    }

    public virtual Effect AddRider(EffectDefinition effectDef) {
        return AddRider(effectDef.effectData);
    }

    public virtual Effect AddRider(EffectData data) {
        Effect rider = AbilityFactory.CreateEffect(data, Source, ParentAbility);
        rider.parentEffect = this;
        RiderEffects.Add(rider);

        //Debug.LogWarning("Adding: " + rider.Data.effectName + " to " + Data.effectName);
        rider.RegisterEvents();
        rider.RegisterRiderOnEventApplied();

        return rider;
    }


    public virtual EffectData RemoveRider(Effect target) {
        if(RiderEffects.Contains(target) == false) {
            Debug.LogError("No Rider named: " + target.Data.effectName + " exists on : " + Data.effectName);
            return null;
        }

        target.RemoveFromAllTargets();
        target.UnregisterEvents();
        RiderEffects.Remove(target);

        //Debug.LogWarning("Removing: " + target.Data.effectName + " from " + Data.effectName);

        return target.Data;
    }

    public virtual EffectData RemoveRider(string riderName) {
        Effect target = null;

        for (int i = 0; i < RiderEffects.Count; i++) {
            if (RiderEffects[i].Data.effectName == riderName) {
                target = RiderEffects[i];
                break;
            }
        }

        if (target == null) {
            Debug.LogError("Could not find Rider: " + riderName + " on the effect: " + Data.effectName);
            return null;
        }

        return RemoveRider(target);
    }

    public bool EvaluateTargetConstraints(Entity target) {
        if (target == null) {
            Debug.LogWarning(Data.effectName + " on " + ParentAbility.Data.abilityName + " tried to evaluate a null entity.");
            return false;
        }

        for (int i = 0; i < targetConstraints.Count; i++) {
            if (targetConstraints[i].Evaluate(target, currentTriggerInstance) == false)
                return false;
        }


        return true;

    }

    public bool EvaluateAbilityTargetConstraints(Ability target) {
        if (target == null) {
            Debug.LogWarning(Data.effectName + " on " + ParentAbility.Data.abilityName + " tried to evaluate a null ability.");
            return false;
        }

        for (int i = 0; i < targetConstraints.Count; i++) {
            if (targetConstraints[i].Evaluate(target, currentTriggerInstance /*as AbilityTriggerInstance*/) == false) {
                //Debug.LogWarning(target.Data.abilityName + " failed a constraint test for a constraint of type: " + targetConstraints[i].GetType().ToString());
                return false;

            }
        }

        return true;
    }

    public bool EvaluateEffectTargetConstraints(Effect target) {
        if (target == null) {
            Debug.LogWarning(Data.effectName + " on " + ParentAbility.Data.abilityName + " tried to evaluate a null effect.");
            return false;
        }

        for (int i = 0; i < targetConstraints.Count; i++) {
            if (targetConstraints[i].Evaluate(target, currentTriggerInstance) == false) {
                //Debug.LogWarning(target.Data.effectName + " failed a constraint test for a constraint of type: " + targetConstraints[i].GetType().ToString());
                return false;

            }
        }

        return true;
    }

    public void ReceiveStartActivationInstance(TriggerInstance activationInstance) {

        if (Suppressed == true) {
            Debug.LogWarning(Data.effectName + " is suppressed");
            return;
        }

        currentTriggerInstance = activationInstance;

        targeter.SetTriggerInstance(activationInstance);
        targeter.Apply();
    }

    public void RecieveEndActivationInstance(TriggerInstance endInstance) {
        RemoveFromAllTargets();
    }

    public virtual bool Apply(Entity target) {

        if (target == null /*|| target.IsDead*/) {
            Debug.LogError(ParentAbility.Data.abilityName + " is trying to affect a null target");
            return false;
        }

        if (target.IsDead == true && Data.canAffectDeadTargets == false) {
            //Debug.Log(ParentAbility.Data.abilityName + " is trying to affect a dead target");
            return false;
        }

        if (EvaluateTargetConstraints(target) == false)
            return false;

        if (CheckNonStacking(target) == true)
            return false;

        if (EntityTargets.Contains(target) == false) {
            EntityTargets.Add(target);
        }

        LastTarget = target;

        if (Data.canOverload == true) {
            if (CheckOverload(target) == true) {

                isOverloading = true;
                //Debug.Log("Overload: " + Data.effectName);
                SendOverloadEvent(target);
            }
            else {
                isOverloading = false;
            }
        }

        if (Data.targeting != EffectTarget.PayloadDelivered && Data.deliveryPayloadToTarget == false) {
            CreateVFX(target);
        }

        return true;
    }

    private bool CheckNonStacking(Entity target) {
        if (Data.nonStacking == false)
            return false;

        if (EntityTargets.Contains(target) == true) {
            if (PsudoStacks.ContainsKey(target) == true) {
                PsudoStacks[target]++;
                //Debug.LogWarning("Incementing a count for : " + Data.effectName + " on " + target.EntityName + " :: " + count);
            }
            else {
                //Debug.LogWarning("Starting a Psudo Stack for: " + Data.effectName + " on " + target.EntityName);
                PsudoStacks.Add(target, 1);
            }

            return true;
        }


        return false;
    }

    private bool CheckOverload(Entity target) {
        float sourceChance = Source.Stats[StatName.OverloadChance];
        float targetChance = target == null ? 0f : target.Stats[StatName.OverloadRecieveChance];
        float skillChance = ParentAbility == null ? 0f : ParentAbility.Stats[StatName.OverloadChance];

        float totalChance = sourceChance + targetChance + skillChance;

        float roll = Random.Range(0f, 1f);

        //Debug.Log("Roll: " + roll + " Chance: " + totalChance);

        if (roll < totalChance) {
            return true;
        }

        return false;
    }

    public virtual void Remove(Entity target) {

        //if (Stats.Contains(StatName.MaxStackCount) == false) {
        //    if (EntityTargets.Contains(target) == true) {
        //        Debug.LogWarning("Remove a psudo stack from " + Data.effectName);
        //    }
        //}


        EntityTargets.RemoveIfContains(target);
    }

    public virtual bool ApplyToEffect(Effect target) {

        if (EvaluateEffectTargetConstraints(target) == false) {

            //Debug.LogWarning(Data.effectName + " failed to pass target constraints");

            return false;
        }

        //Debug.LogWarning("Applying: " + Data.effectName + " to " + target.Data.effectName);

        EffectTargets.AddUnique(target);

        return true;
    }

    public virtual void RemoveFromEffect(Effect target) {
        EffectTargets.RemoveIfContains(target);
    }

    public virtual bool ApplyToAbility(Ability target) {

        if (EvaluateAbilityTargetConstraints(target) == false)
            return false;

        AbilityTargets.AddUnique(target);

        return true;
    }

    public virtual void RemoveFromAbility(Ability target) {
        AbilityTargets.RemoveIfContains(target);
    }

    public void RemoveFromAllTargets() {
        for (int i = EntityTargets.Count - 1; i >= 0; i--) {
            Remove(EntityTargets[i]);
        }

        for (int i = AbilityTargets.Count - 1; i >= 0; i--) {
            RemoveFromAbility(AbilityTargets[i]);
        }

        for (int i = EffectTargets.Count - 1; i >= 0; i--) {
            RemoveFromEffect(EffectTargets[i]);
        }

        EntityTargets.Clear();
        AbilityTargets.Clear();
        EffectTargets.Clear();


        //Rider Experiments

        for (int i = 0; i < RiderEffects.Count; i++) {
            RiderEffects[i].RemoveFromAllTargets();
        }

        //Lingering Effect Zones

        for (int i = 0; i < ActiveEffectZones.Count; i++) {
            if (ActiveEffectZones[i] == null)
                continue;

            ActiveEffectZones[i].CleanUp();
        }

        ActiveEffectZones.Clear();

    }

    public virtual void RegisterEvents() {
        RegisterRiderEvents();
        //EventManager.RegisterListener(GameEvent.AbilityStatAdjusted, OnAbilityStatChanged);
    }

    public virtual void UnregisterEvents() {
        UnRegisterRiderEvents();
        EventManager.RemoveMyListeners(this);
    }

    protected virtual void OnAbilityStatChanged(EventData data) {
        if (ParentAbility == null)
            return;

        Ability ability = data.GetAbility("Ability");

        if (ability != ParentAbility) {
            
            if(Data.effectName == "Apply Arcane Vulnerable Status")
                Debug.LogWarning(ability.Data.abilityName + " is not " + ParentAbility.Data.abilityName);
            
            return;
        }

        StatName stat = (StatName)data.GetInt("Stat");

        if (Stats.Contains(stat) == false)
            return;

        Stats.SetStatValue(stat, ParentAbility.Stats[stat], ParentAbility);

        //Debug.Log("Updating: " + stat + " on " + Data.effectName + " because it changed on the parent ability: " + ability.Data.abilityName + ". Value: " + Stats[stat]);



    }

    protected void UnRegisterRiderEvents() {
        for (int i = 0; i < RiderEffects.Count; i++) {
            EventManager.RemoveMyListeners(RiderEffects[i]);
        }
    }

    protected void RegisterRiderEvents() {
        for (int i = 0; i < RiderEffects.Count; i++) {

            RiderEffects[i].RegisterRiderOnEventApplied();
        }
    }

    protected void RegisterRiderOnEventApplied() {
        if (parentEffect != null) {
            //Debug.Log("Registering a rider event");
            EventManager.RegisterListener(GameEvent.EffectApplied, OnEffectApplied);
            //EventManager.RegisterListener(GameEvent.AbilityStatAdjusted, OnAbilityStatChanged);

        }
    }

    protected virtual void OnEffectApplied(EventData data) {
        Effect parent = data.GetEffect("Effect");

        //Debug.LogWarning("Recieveing an on effect applied event from: " + parent.Data.effectName);

        if (parent != parentEffect) {
            return;
        }

        //Debug.Log("Rider effect is firing: " + Data.effectName + " from " + parentEffect.Data.effectName);

        Apply(parentEffect.LastTarget);
        //SendEffectAppliedEvent();
    }

    public void SendEffectAppliedEvent() {
        EventData data = new EventData();
        data.AddEffect("Effect", this);
        data.AddAbility("Ability", ParentAbility);
        data.AddEntity("Source", Source);

        data.AddEffect("Parent Effect", parentEffect);

        //Debug.Log(Data.effectName + " has been applied from the Ability: " + ParentAbility.Data.abilityName);

        EventManager.SendEvent(GameEvent.EffectApplied, data);
    }

    public void SendOverloadEvent(Entity target) {
        EventData data = new EventData();
        data.AddAbility("Ability", ParentAbility);
        data.AddEffect("Effect", this);
        data.AddEntity("Target", target);
        data.AddEntity("Source", Source);

        EventManager.SendEvent(GameEvent.OverloadTriggered, data);
    }

    public void CreateVFX(Entity currentTarget) {

        if (ZoneInfo.applyVFX == null)
            return;


        Vector2 spawnLocation = Data.spawnLocation switch {
            DeliverySpawnLocation.Source => Source.transform.position,
            DeliverySpawnLocation.Trigger => targeter.ActivationInstance.TriggeringEntity.transform.position,
            DeliverySpawnLocation.Cause => targeter.ActivationInstance.CauseOfTrigger.transform.position,
            DeliverySpawnLocation.MousePointer => Input.mousePosition,
            DeliverySpawnLocation.AITarget => throw new NotImplementedException(),
            DeliverySpawnLocation.FixedLocations => throw new NotImplementedException(),
            DeliverySpawnLocation.RandomViewportPosition => targeter.GetRandomViewportPosition(),
            DeliverySpawnLocation.WorldPositionSequence => throw new NotImplementedException(),
            DeliverySpawnLocation.AbilityLastPayloadLocation => targeter.GetLastAbilityPayloadLocation(),
            DeliverySpawnLocation.LastEffectZoneLocation => targeter.ActivationInstance.SavedLocation,
            _ => Vector2.zero
        };



        GameObject activeVFX = VFXUtility.SpawnVFX(ZoneInfo.applyVFX, spawnLocation, TargetUtilities.GetRotationTowardTarget(currentTarget.transform.position, spawnLocation), null, 1f);


        ElectricArcEffect arc = activeVFX.GetComponent<ElectricArcEffect>();

        if(arc != null) {
            arc.SetPositions(spawnLocation, currentTarget.transform.position);
        }



    }

    public virtual void Stack(Status status) {

    }

    public void TrackActiveDelivery(Projectile delivery) {
        activeDelivery = delivery;
    }


    public bool IsTargetInOtherZone(EffectZone zone, Entity target) {

        for (int i = 0; i < ActiveEffectZones.Count; i++) {
            if (ActiveEffectZones[i] == null || ActiveEffectZones[i] == zone)
                continue;

            if (ActiveEffectZones[i].IsTargetAlreadyAffected(target) == true)
                return true;
        }


        return false;
    }

    public virtual string GetTooltip() {


        return Data.effectDescription;
    }

    public string GetProjectileStatsTooltip() {
        StringBuilder builder = new StringBuilder();

        if (Stats.Contains(StatName.ProjectileChainCount) && Stats[StatName.ProjectileChainCount] > 0) {
            string chainCount = TextHelper.FormatStat(StatName.ProjectileChainCount, Stats[StatName.ProjectileChainCount]);

            builder.AppendLine("Chains up to: " + chainCount + " times");
        }

        if (Stats.Contains(StatName.ProjectilePierceCount) && Stats[StatName.ProjectilePierceCount] > 0) {
            string pierceCount = TextHelper.FormatStat(StatName.ProjectilePierceCount, Stats[StatName.ProjectilePierceCount]);

            builder.AppendLine("Pierces up to: " + pierceCount + " times");
        }

        if (Stats.Contains(StatName.ProjectileSplitCount) && Stats[StatName.ProjectileSplitCount] > 0) {
            string splitCount = TextHelper.FormatStat(StatName.ProjectileSplitCount, Stats[StatName.ProjectileSplitCount]);

            builder.AppendLine("Splits up to: " + splitCount + " times");
        }


        return builder.ToString();
    }

    public bool EffectResolvedCallback() {
        return true;
    }



}

public class EmptyEffect : Effect {
    public override EffectType Type => EffectType.None;

    public EmptyEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        return true;
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        return true;
    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;


        return true;
    }
}

public class ModifiyElapsedCooldownEffect : Effect {
    public override EffectType Type => EffectType.ModifyElapsedCooldown;

    public ModifiyElapsedCooldownEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        if (target is EntityPlayer) {
            List<Ability> allAbilities = target.AbilityManager.GetAllAbilities();

            float coolDownProgress = Data.scaleFromAbilityLevel == false ? Data.cooldownElapsedModifier : Data.cooldownElapsedModifier * ParentAbility.AbilityLevel;

            for (int i = 0; i < allAbilities.Count; i++) {
    
                allAbilities[i].ModifyCooldownElasped(coolDownProgress);
            }
        }
        else {
            Debug.LogError("Elasped cooldown mod is not yet supported for NPCs");
            return false;
        }

        return true;
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        float coolDownProgress = Data.scaleFromAbilityLevel == false ? Data.cooldownElapsedModifier : Data.cooldownElapsedModifier * ParentAbility.AbilityLevel;

        target.ModifyCooldownElasped(coolDownProgress);


        return true;
    }

    public override bool ApplyToEffect(Effect target) {

        Debug.LogError("Tried to modify the cooldown of an effect: " + target.Data.effectName + " this is not supported");
        return false;
    }


    public override string GetTooltip() {
        float coolDownProgress = Data.scaleFromAbilityLevel == false ? Data.cooldownElapsedModifier : Data.cooldownElapsedModifier * ParentAbility.AbilityLevel;

        string colorized = TextHelper.ColorizeText(coolDownProgress.ToString(), Color.yellow);

        string replacement = Data.effectDescription.Replace("{}", colorized);

        return replacement;


    }
}

public class EffectChangePayaload : Effect {
    public override EffectType Type => EffectType.ChangePayload;

    private Dictionary<Effect, Entity> trackedPayloads = new Dictionary<Effect, Entity>();


    public EffectChangePayaload(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        Debug.LogError("Changing Payloads at the Entity level is not yet supported");
        return false;
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;


        Debug.LogError("Changing Payloads at the Ability level is not yet supported");
        return false;


    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;


        if(TrackChangedPayload(target) == true) {
            target.PayloadPrefab = Data.newPayloadPrefab;
        }


        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        if(trackedPayloads.TryGetValue(target, out Entity payload) == true) {

            if(target.PayloadPrefab == payload) {
                Debug.LogError(target.Data.effectName + " already has the payload tracked by " + Data.effectName);
                return;
            }

            target.PayloadPrefab = payload;
            trackedPayloads.Remove(target);
        }
        else {
            Debug.LogError(target.Data.effectName + " is not tracked by a change payload effect: " + Data.effectName);
        }

       
    }


    private bool TrackChangedPayload(Effect target) {
        if(trackedPayloads.TryGetValue(target, out Entity trackedPayload)== true) {
            if(trackedPayload == Data.newPayloadPrefab) {
                Debug.LogError("Trying to reapply the same changed payload to: " + target.Data.effectName);
                return false;
            }

            trackedPayloads[target] = target.PayloadPrefab;
            
        }
        else {
            trackedPayloads.Add(target, target.PayloadPrefab);
        }

        return true;
    }
}

public class EffectChangeEffectZpme : Effect {
    public override EffectType Type => EffectType.ChangeEffectZone;

    private Dictionary<Effect, EffectZoneInfo> trackedEffectZones = new Dictionary<Effect, EffectZoneInfo>();


    public EffectChangeEffectZpme(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        Debug.LogError("Changing Payloads at the Entity level is not yet supported");
        return false;
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;


        Debug.LogError("Changing Payloads at the Ability level is not yet supported");
        return false;


    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;


        if (TrackChangedEffectZone(target) == true) {
            target.ZoneInfo = Data.effectZoneInfo;
        }


        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        if (trackedEffectZones.TryGetValue(target, out EffectZoneInfo effectZone) == true) {

            if (target.EffectZonePrefab == effectZone.effectZonePrefab) {
                Debug.LogError(target.Data.effectName + " already has the effect zone tracked by " + Data.effectName);
                return;
            }

            target.ZoneInfo = effectZone;
            trackedEffectZones.Remove(target);
        }
        else {
            Debug.LogError(target.Data.effectName + " is not tracked by a change effect zone effect: " + Data.effectName);
        }


    }


    private bool TrackChangedEffectZone(Effect target) {
        if (trackedEffectZones.TryGetValue(target, out EffectZoneInfo trackedEffectZone) == true) {
            if (trackedEffectZone.effectZonePrefab == Data.effectZoneInfo.effectZonePrefab) {
                Debug.LogError("Trying to reapply the same changed effect zone to: " + target.Data.effectName);
                return false;
            }

            trackedEffectZones[target] = target.ZoneInfo;

        }
        else {
            trackedEffectZones.Add(target, target.ZoneInfo);
        }

        return true;
    }
}

public class NPCStateChangeEffect : Effect {
    public override EffectType Type => EffectType.NPCStateChange;

    private string previousStateName;


    public NPCStateChangeEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        NPC npc = target as NPC;
        if (npc != null) {
            previousStateName = npc.Brain.ForceStateChange(Data.targetStateName);
            return true;
        }


        return false;
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        if (string.IsNullOrEmpty(previousStateName) == true) {
            Debug.LogWarning("No previous state name saved when removing a forced state change");
            return;
        }

        NPC npc = target as NPC;
        if (npc != null) {
            npc.Brain.ForceStateChange(previousStateName);
        }
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        return true;
    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;


        return true;
    }
}

public class ActivateAbilityEffect : Effect {
    public override EffectType Type => EffectType.ActivateOtherAbility;

    public ActivateAbilityEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        Ability targetAbility = target.GetAbilityByName(Data.nameOfAbilityToActivate, AbilityCategory.Any);

        if (targetAbility != null) {
            targetAbility.ForceActivate();
        }


        return true;
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        target.ForceActivate();

        return true;
    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;


        return true;
    }


    public override string GetTooltip() {
        string baseDesc = base.GetTooltip();

        string replacement = baseDesc.Replace("{PR}", TextHelper.FormatStat(StatName.ProcChance, ParentAbility.Stats[StatName.ProcChance]));
        return replacement;
    }
}

public class ForcedMovementEffect : Effect {

    public override EffectType Type => EffectType.Movement;


    public ForcedMovementEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        //Debug.Log("Applying a: " + Data.targetDestination + " effect");

        switch (Data.targetDestination) {
            case MovementDestination.SourceForward:
                ApplySourceForward(target);
                break;
            case MovementDestination.SourceCurrentVelocity:
                break;
            case MovementDestination.MousePosition:
                ApplyTowardMouse(target);
                break;
            case MovementDestination.AwayFromSource:
                ApplyForceAwayFromSource(target);
                break;
            default:
                break;

        }

        return true;
    }


    private void ApplyTowardMouse(Entity target) {
        Vector2 directionToMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Source.transform.position;
        Vector2 force = directionToMouse.normalized * Stats[StatName.Knockback];

        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();

        if (targetBody != null) {

            if (Data.resetMovement == true)
                targetBody.velocity = Vector2.zero;

            targetBody.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void ApplySourceForward(Entity target) {
        Vector2 force = target.GetOriginPoint().up.normalized * Stats[StatName.Knockback];

        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();

        if (targetBody != null) {
            targetBody.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void ApplyForceAwayFromSource(Entity target) {
        Vector2 direction = target.transform.position - Source.transform.position;

        Vector2 resultingForce = direction.normalized * Stats[StatName.Knockback];

        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();

        if (targetBody != null) {
            targetBody.AddForce(resultingForce, ForceMode2D.Impulse);
        }
    }
}

public class TeleportEffect : Effect {

    public override EffectType Type => EffectType.Teleport;


    public TeleportEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        switch (Data.teleportDestination) {
            case TeleportDestination.MousePointer:
                TeleportToMousePointer(target);
                break;
            case TeleportDestination.RandomViewport:
                break;
            case TeleportDestination.RandomNearTarget:
                break;
            case TeleportDestination.SourceForward:
                break;

            case TeleportDestination.OtherTarget:
                Entity other = targeter.GetLastTargetFromOtherEffect(Data.otherAbilityName, Data.otherEffectName, AbilityCategory.Any);
                TeleportToEntity(target, other);
                break;
            default:
                break;
        }




        return true;
    }

    private void TeleportToEntity(Entity target, Entity other) {

        if (other == null)
            return;

        VFXUtility.SpawnVFX(Data.teleportVFX, target.transform, null, 1f);
        SendTeleportInitiatedEvent(target);

        target.transform.position = other.transform.position;

        VFXUtility.SpawnVFX(Data.teleportVFX, target.transform, null, 1f);
        SendTeleportConcludedEvent(target);
    }

    private void TeleportToMousePointer(Entity target) {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 viewportCheck = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //Debug.Log(viewportCheck + " is the viewport checker");

        if (viewportCheck.x < 0.05f || viewportCheck.x > 0.95f) {
            ParentAbility.RecoveryCharge(1);
            return;

        }

        if (viewportCheck.y < 0.22f || viewportCheck.y > 0.95f) {
            ParentAbility.RecoveryCharge(1);
            return;
        }


        VFXUtility.SpawnVFX(Data.teleportVFX, target.transform, null, 1f);
        SendTeleportInitiatedEvent(target);

        target.transform.position = mousePos;

        VFXUtility.SpawnVFX(Data.teleportVFX, target.transform, null, 1f);
        SendTeleportConcludedEvent(target);

    }

    private void ApplySourceForward(Entity target) {

    }


    private void SendTeleportInitiatedEvent(Entity target) {
        EventData data = new EventData();
        data.AddEntity("Target", target);
        data.AddEffect("Effect", this);
        data.AddAbility("Ability", ParentAbility);
        data.AddVector3("Position", target.transform.position);

        EventManager.SendEvent(GameEvent.TeleportInitiated, data);
    }

    private void SendTeleportConcludedEvent(Entity target) {
        EventData data = new EventData();
        data.AddEntity("Target", target);
        data.AddEffect("Effect", this);
        data.AddAbility("Ability", ParentAbility);
        data.AddVector3("Position", target.transform.position);

        EventManager.SendEvent(GameEvent.TeleportConcluded, data);
    }


}

public class SuppressEffect : Effect {
    public override EffectType Type => EffectType.SuppressEffect;

    private List<Effect> trackedEffects = new List<Effect>();

    public SuppressEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;

        target.Suppressed = true;
        trackedEffects.Add(target);

        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        if (trackedEffects.Contains(target) == false) {
            Debug.LogError("An effect: " + target.Data.effectName + " is not tracked by a suppress effect and is trying to be removed");
            return;
        }

        target.Suppressed = false;

        trackedEffects.Remove(target);
    }
}

public class AddTagEffect : Effect {

    public override EffectType Type => EffectType.AddTag;

    private Dictionary<Ability, List<AbilityTag>> trackedTags = new Dictionary<Ability, List<AbilityTag>>();

    public AddTagEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        for (int i = 0; i < Data.tagsToAdd.Count; i++) {

            if (TrackTag(target, Data.tagsToAdd[i]) == false)
                continue;
            
            target.AddTag(Data.tagsToAdd[i]);
        }

        //TrackTags(target);

        return true;
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        if (trackedTags.TryGetValue(target, out List<AbilityTag> tags) == true) {
            for (int i = 0; i < tags.Count; i++) {
                target.RemoveTag(tags[i]);
            }

            trackedTags.Remove(target);
        }
    }


    private bool TrackTag(Ability target, AbilityTag tag) {
        
        if(trackedTags.TryGetValue(target, out List<AbilityTag> tags) == true) {
            if (target.Tags.Contains(tag) == false) {
                tags.Add(tag);
                return true;
            }
        }
        else {
            if (target.Tags.Contains(tag) == false) {
                trackedTags.Add(target, new List<AbilityTag> { tag });
                return true;
            }
        }
        return false;
        
    }

    private void TrackTags(Ability target) {
        if (trackedTags.ContainsKey(target) == false) {
            trackedTags.Add(target, Data.tagsToAdd);
        }
    }
}

public class RemoveTagEffect : Effect {

    public override EffectType Type => EffectType.RemoveTag;

    private Dictionary<Ability, List<AbilityTag>> trackedTags = new Dictionary<Ability, List<AbilityTag>>();

    public RemoveTagEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        for (int i = 0; i < Data.tagsToRemove.Count; i++) {

            if (TrackTag(target, Data.tagsToRemove[i]) == false)
                continue;
            
            target.RemoveTag(Data.tagsToRemove[i]);
        }

        //TrackTags(target);

        return true;
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        if (trackedTags.TryGetValue(target, out List<AbilityTag> tags) == true) {
            for (int i = 0; i < tags.Count; i++) {
                target.AddTag(tags[i]);
            }

            trackedTags.Remove(target);
        }
    }

    private bool TrackTag(Ability target, AbilityTag tag) {

        if (trackedTags.TryGetValue(target, out List<AbilityTag> tags) == true) {
            if (target.Tags.Contains(tag) == true) {
                tags.Add(tag);
                return true;
            }
        }
        else {
            if (target.Tags.Contains(tag) == true) {
                trackedTags.Add(target, new List<AbilityTag> { tag });
                return true;
            }
        }
        return false;

    }

    private void TrackTags(Ability target) {
        if (trackedTags.ContainsKey(target) == false) {
            trackedTags.Add(target, Data.tagsToRemove);
        }
    }
}


public class AddStatScalerEffect : Effect {

    public override EffectType Type => EffectType.AddStatScaler;

    private Dictionary<Effect, List<StatScaler>> trackedScalers = new Dictionary<Effect, List<StatScaler>>();

    public AddStatScalerEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        Debug.LogError("Stat scallers cannot be added to entity. Change your Effect to Target Effects");

        return false;
    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;

        StatAdjustmentEffect adj = target as StatAdjustmentEffect;

        if (adj == null) {
            Debug.LogError("Target: " + target.Type + " cannot accept stat scalers. Make sure you're targeting a Stat Adjustment Effect");
            return false;
        }

        for (int i = 0; i < Data.statScalersToAdd.Count; i++) {
            adj.AddScaler(Data.statScalersToAdd[i]);

            TrackScaler(target, Data.statScalersToAdd[i]);
        }

        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        StatAdjustmentEffect adj = target as StatAdjustmentEffect;

        if (trackedScalers.TryGetValue(target, out List<StatScaler> list) == true) {
            for (int i = 0; i < list.Count; i++) {
                adj.RemoveScaler(list[i]);
            }

            trackedScalers.Remove(target);
        }

    }

    private void TrackScaler(Effect target, StatScaler scaler) {
        if (trackedScalers.TryGetValue(target, out List<StatScaler> list) == true) {
            list.Add(scaler);
        }
        else {
            trackedScalers.Add(target, new List<StatScaler> { scaler });
        }
    }

    public override string GetTooltip() {
        //return base.GetTooltip();

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("The Base Skill now Scales from: ");

        for (int i = 0; i < Data.statScalersToAdd.Count; i++) {

            StatScaler scaler = Data.statScalersToAdd[i];

            string formatted = TextHelper.FormatStat(scaler.targetStat, scaler.statScaleBaseValue);

            builder.Append(formatted + "of " + TextHelper.PretifyStatName(scaler.targetStat));

            if (i != Data.statScalersToAdd.Count - 1)
                builder.AppendLine();

        }

        return builder.ToString();
    }

}

public class ApplyOtherEffect : Effect {

    public override EffectType Type => EffectType.ApplyOtherEffect;


    public ApplyOtherEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        if (Data.applyTriggeringEffect == true) {
            Effect triggerEffect = currentTriggerInstance.TriggeringEffect;

            if (triggerEffect != null) {
                triggerEffect.Apply(target);
            }

            return true;
        }


        Tuple<Ability, Effect> abilityEffece = AbilityUtilities.GetAbilityAndEffectByName(Data.targetOtherEffectParentAbilityName, Data.targetOtherEffectName, Source, AbilityCategory.Any);

        if (abilityEffece.Item2 != null) {
            abilityEffece.Item2.Apply(target);
        }
        else {
            Debug.LogError("Couldn't find the right ability on: " + Source.EntityName);
        }

        return true;
    }
}

public class ModifyProjectileEffect : Effect {

    public override EffectType Type => EffectType.ModifyProjectile;




    public ModifyProjectileEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

       Projectile projectile = target as Projectile;
        if (projectile == null) {
            Debug.LogError("A Modify Projectile Effect is targeting a non Projectile: " + target.EntityName);
            return false;
        }


        ProjectileMovement movement = projectile.Movement as ProjectileMovement;

        movement.ChangeBehaviour(Data.modifiedMovementBehavior, Data.modifiedSeekDuration, Data.modifiedDrunkInterval);


        return true;
    }


    public override void Remove(Entity target) {
        base.Remove(target);

        Debug.LogError("Removing Projectile Modifications is not yet supported");
    }
}


public class AddRiderEffect : Effect {

    public override EffectType Type => EffectType.AddRider;
    private List<Effect> activeDisplayEffects = new List<Effect>();
    private Dictionary<Effect, List<Effect>> effectTrackedRiderEffects = new Dictionary<Effect, List<Effect>>();

    public AddRiderEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        for (int i = 0; i < data.ridersToAdd.Count; i++) {
            Effect template = AbilityFactory.CreateEffect(data.ridersToAdd[i].effectData, source, ParentAbility);
            activeDisplayEffects.Add(template);
            //Debug.Log("Creating a display effect for: " + data.effectName + " by the name of: " + template.Data.effectName);
        }
    }

    public override bool Apply(Entity target) {

        if (base.Apply(target) == false)
            return false;

        Debug.LogError("Applying Riders to Entities is not yet supported");
        return false;

        //Ability targetAbility = target.GetAbilityByName(Data.targetAbilityToAddEffectsTo, AbilityCategory.Any);

        //if (targetAbility == null) {
        //    Debug.LogError("Could not find the ability: " + Data.targetAbilityToAddEffectsTo + " on the Entity: " + target.EntityName);
        //    return false;
        //}

        //for (int i = 0; i < Data.effectsToAdd.Count; i++) {
        //    Effect newEffect = AbilityFactory.CreateEffect(Data.effectsToAdd[i].effectData, Source, targetAbility);

        //    targetAbility.AddEffect(newEffect);
        //    TrackEffectsOnEntity(target, newEffect);
        //}

        //return true;
    }

    public override void Remove(Entity target) {
        base.Remove(target);
    }

    public override bool ApplyToEffect(Effect target) {
        if(base.ApplyToEffect(target) == false)
            return false;

        for (int i = 0; i < Data.ridersToAdd.Count; i++) {
            Effect newRider = target.AddRider(Data.ridersToAdd[i]);
            TrackEffectOnEffect(target, newRider);
        }

        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        if(effectTrackedRiderEffects.TryGetValue(target, out List<Effect> effects) == true) {

            for (int i = 0; i < effects.Count; i++) {
                target.RemoveRider(effects[i]);
            }
        }
    }

    private void TrackEffectOnEffect(Effect target, Effect newEffect) {
        if (effectTrackedRiderEffects.TryGetValue(target, out List<Effect> children) == true) {
            children.Add(newEffect);
        }
        else {
            effectTrackedRiderEffects.Add(target, new List<Effect> { newEffect });
        }
    }


    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        Debug.LogError("Applying Riders to Abilities is not yet supported");
        return false;

        //for (int i = 0; i < Data.effectsToAdd.Count; i++) {
        //    Effect newEffect = AbilityFactory.CreateEffect(Data.effectsToAdd[i].effectData, Source, target);

        //    target.AddEffect(newEffect);
        //    TrackEffectsOnAbility(target, newEffect);

        //}

        //return true;
    }

    //private void TrackEffectsOnAbility(Ability target, Effect newEffect) {
    //    if (abilityTrackedEffects.TryGetValue(target, out List<Effect> children) == true) {
    //        children.Add(newEffect);
    //    }
    //    else {
    //        abilityTrackedEffects.Add(target, new List<Effect> { newEffect });
    //    }
    //}

    //private void TrackEffectsOnEntity(Entity target, Effect newEffect) {
    //    if (entityTrackedEffects.TryGetValue(target, out List<Effect> children) == true) {
    //        children.Add(newEffect);
    //    }
    //    else {
    //        entityTrackedEffects.Add(target, new List<Effect> { newEffect });
    //    }
    //}

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        //if (abilityTrackedEffects.TryGetValue(target, out List<Effect> effectsAdded) == true) {
        //    for (int i = 0; i < effectsAdded.Count; i++) {
        //        target.RemoveEffect(effectsAdded[i]);
        //    }

        //    abilityTrackedEffects.Remove(target);
        //}

    }

    public override string GetTooltip() {
        StringBuilder builder = new StringBuilder();

        //Debug.Log("Showing a tooltip for an Add Effect Effect On " + Data.effectName + ". " + activeEffects.Count + " effects found to add");

        for (int i = 0; i < activeDisplayEffects.Count; i++) {

            string effectTooltip = activeDisplayEffects[i].GetTooltip();

            if (string.IsNullOrEmpty(effectTooltip) == false)
                builder.Append(effectTooltip);

            if (i != activeDisplayEffects.Count - 1)
                builder.AppendLine();
        }

        return builder.ToString();
    }
}

public class RemoveRiderEffect : Effect {

    public override EffectType Type => EffectType.RemoveRider;

    private Dictionary<Effect, List<EffectData>> effectTrackedRiderEffects = new Dictionary<Effect, List<EffectData>>();

    public RemoveRiderEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        
    }

    public override bool Apply(Entity target) {

        if (base.Apply(target) == false)
            return false;

        Debug.LogError("Applying Riders to Entities is not yet supported");
        return false;
    }

    public override void Remove(Entity target) {
        base.Remove(target);
    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;

        for (int i = 0; i < Data.ridersToRemove.Count; i++) {
            EffectData removedData = target.RemoveRider(Data.ridersToRemove[i].effectData.effectName);
            

            if(removedData != null)
                TrackEffectOnEffect(target, removedData);
        }

        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        if (effectTrackedRiderEffects.TryGetValue(target, out List<EffectData> effects) == true) {

            for (int i = 0; i < effects.Count; i++) {
                target.AddRider(effects[i]);
            }
        }
    }

    private void TrackEffectOnEffect(Effect target, EffectData removedData) {
        if (effectTrackedRiderEffects.TryGetValue(target, out List<EffectData> children) == true) {
            children.Add(removedData);
        }
        else {
            effectTrackedRiderEffects.Add(target, new List<EffectData> { removedData });
        }
    }


    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        Debug.LogError("Removeing Riders from Abilities is not yet supported");
        return false;
    }



    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);
    }

    public override string GetTooltip() {
        return base.GetTooltip();
        
    }
}

public class AddEffectEffect : Effect {

    public override EffectType Type => EffectType.AddEffect;

    private Dictionary<Entity, List<Effect>> entityTrackedEffects = new Dictionary<Entity, List<Effect>>();
    private Dictionary<Ability, List<Effect>> abilityTrackedEffects = new Dictionary<Ability, List<Effect>>();
    private List<Effect> activeDisplayEffects = new List<Effect>();

    public AddEffectEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        for (int i = 0; i < data.effectsToAdd.Count; i++) {
            Effect template = AbilityFactory.CreateEffect(data.effectsToAdd[i].effectData, source, ParentAbility);
            activeDisplayEffects.Add(template);
            //Debug.Log("Creating a display effect for: " + data.effectName + " by the name of: " + template.Data.effectName);
        }
    }

    public override bool Apply(Entity target) {

        if (base.Apply(target) == false)
            return false;

        Ability targetAbility = target.GetAbilityByName(Data.targetAbilityToAddEffectsTo, AbilityCategory.Any);

        if (targetAbility == null) {
            Debug.LogError("Could not find the ability: " + Data.targetAbilityToAddEffectsTo + " on the Entity: " + target.EntityName);
            return false;
        }

        for (int i = 0; i < Data.effectsToAdd.Count; i++) {
            Effect newEffect = AbilityFactory.CreateEffect(Data.effectsToAdd[i].effectData, Source, targetAbility);

            targetAbility.AddEffect(newEffect);
            TrackEffectsOnEntity(target, newEffect);
        }

        return true;
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        if (entityTrackedEffects.TryGetValue(target, out List<Effect> effectsAdded) == true) {

            Ability targetAbility = target.GetAbilityByName(Data.targetAbilityToAddEffectsTo, AbilityCategory.Any);

            if (targetAbility == null) {
                Debug.LogError("Could not find the ability: " + Data.targetAbilityToAddEffectsTo + " on the Entity: " + target.EntityName);
                return;
            }

            for (int i = 0; i < effectsAdded.Count; i++) {
                targetAbility.RemoveEffect(effectsAdded[i]);
            }

            entityTrackedEffects.Remove(target);
        }
    }


    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        for (int i = 0; i < Data.effectsToAdd.Count; i++) {
            Effect newEffect = AbilityFactory.CreateEffect(Data.effectsToAdd[i].effectData, Source, target);

            target.AddEffect(newEffect);
            TrackEffectsOnAbility(target, newEffect);

        }

        return true;
    }

    private void TrackEffectsOnAbility(Ability target, Effect newEffect) {
        if (abilityTrackedEffects.TryGetValue(target, out List<Effect> children) == true) {
            children.Add(newEffect);
        }
        else {
            abilityTrackedEffects.Add(target, new List<Effect> { newEffect });
        }
    }

    private void TrackEffectsOnEntity(Entity target, Effect newEffect) {
        if (entityTrackedEffects.TryGetValue(target, out List<Effect> children) == true) {
            children.Add(newEffect);
        }
        else {
            entityTrackedEffects.Add(target, new List<Effect> { newEffect });
        }
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        if (abilityTrackedEffects.TryGetValue(target, out List<Effect> effectsAdded) == true) {
            for (int i = 0; i < effectsAdded.Count; i++) {
                target.RemoveEffect(effectsAdded[i]);
            }

            abilityTrackedEffects.Remove(target);
        }

    }

    public override string GetTooltip() {
        StringBuilder builder = new StringBuilder();

        //Debug.Log("Showing a tooltip for an Add Effect Effect On " + Data.effectName + ". " + activeEffects.Count + " effects found to add");

        for (int i = 0; i < activeDisplayEffects.Count; i++) {

            string effectTooltip = activeDisplayEffects[i].GetTooltip();

            if (string.IsNullOrEmpty(effectTooltip) == false)
                builder.Append(effectTooltip);

            if (i != activeDisplayEffects.Count - 1)
                builder.AppendLine();
        }

        return builder.ToString();
    }
}

public class RemoveEffectEffect : Effect {

    public override EffectType Type => EffectType.RemoveEffect;

    private Dictionary<Entity, List<Effect>> entityTrackedEffects = new Dictionary<Entity, List<Effect>>();
    private Dictionary<Ability, List<Effect>> abilityTrackedEffects = new Dictionary<Ability, List<Effect>>();
    //private List<Effect> activeEffects = new List<Effect>();

    public RemoveEffectEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        //for (int i = 0; i < data.effectsToAdd.Count; i++) {
        //    Effect template = AbilityFactory.CreateEffect(data.effectsToAdd[i].effectData, source);
        //    activeEffects.Add(template);
        //    //Debug.Log("Creating a display effect for: " + data.effectName + " by the name of: " + template.Data.effectName);
        //}
    }

    public override bool Apply(Entity target) {

        if (base.Apply(target) == false)
            return false;

        Ability targetAbility = target.GetAbilityByName(Data.targetAbilityToAddEffectsTo, AbilityCategory.Any);

        if (targetAbility == null) {
            Debug.LogError("Could not find the ability: " + Data.targetAbilityToAddEffectsTo + " on the Entity: " + target.EntityName);
            return false;
        }

        for (int i = 0; i < Data.effectsToRemove.Count; i++) {
            string targetName = Data.effectsToRemove[i];
            Effect targetEffect = targetAbility.GetEffectByName(targetName);

            if (targetEffect == null) {
                Debug.LogError("Could not find an effect: " + targetName + " on the ability " + targetAbility.Data.abilityName);
                continue;
            }

            targetAbility.RemoveEffect(targetEffect);
            TrackEffectsOnEntity(target, targetEffect);
        }

        return true;
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        if (entityTrackedEffects.TryGetValue(target, out List<Effect> effectsAdded) == true) {

            Ability targetAbility = target.GetAbilityByName(Data.targetAbilityToAddEffectsTo, AbilityCategory.Any);

            if (targetAbility == null) {
                Debug.LogError("Could not find the ability: " + Data.targetAbilityToAddEffectsTo + " on the Entity: " + target.EntityName);
                return;
            }

            for (int i = 0; i < effectsAdded.Count; i++) {
                targetAbility.AddEffect(effectsAdded[i]);
            }

            entityTrackedEffects.Remove(target);
        }
    }


    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        for (int i = 0; i < Data.effectsToRemove.Count; i++) {

            string targetName = Data.effectsToRemove[i];
            Effect targetEffect = target.GetEffectByName(targetName);

            if (targetEffect == null) {
                Debug.LogError("Could not find an effect: " + targetName + " on the ability " + target.Data.abilityName);
                continue;
            }

            target.RemoveEffect(targetEffect);
            TrackEffectsOnAbility(target, targetEffect);
        }

        return true;
    }

    private void TrackEffectsOnAbility(Ability target, Effect newEffect) {
        if (abilityTrackedEffects.TryGetValue(target, out List<Effect> children) == true) {
            children.Add(newEffect);
        }
        else {
            abilityTrackedEffects.Add(target, new List<Effect> { newEffect });
        }
    }

    private void TrackEffectsOnEntity(Entity target, Effect newEffect) {
        if (entityTrackedEffects.TryGetValue(target, out List<Effect> children) == true) {
            children.Add(newEffect);
        }
        else {
            entityTrackedEffects.Add(target, new List<Effect> { newEffect });
        }
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        if (abilityTrackedEffects.TryGetValue(target, out List<Effect> effectsRemoved) == true) {
            for (int i = 0; i < effectsRemoved.Count; i++) {
                target.AddEffect(effectsRemoved[i]);
            }

            abilityTrackedEffects.Remove(target);
        }

    }

    public override string GetTooltip() {
        return base.GetTooltip();


    }
}

public class AddAbilityEffect : Effect {

    public override EffectType Type => EffectType.AddAbility;


    private Dictionary<Entity, List<Ability>> trackedAbilities = new Dictionary<Entity, List<Ability>>();

    private List<Ability> activeAbilities = new List<Ability>();

    public AddAbilityEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        for (int i = 0; i < data.abilitiesToAdd.Count; i++) {
            Ability template = AbilityFactory.CreateAbility(data.abilitiesToAdd[i].AbilityData, source);
            activeAbilities.Add(template);
        }

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        for (int i = 0; i < Data.abilitiesToAdd.Count; i++) {

            NPC npcTarget = target as NPC;
            if(npcTarget != null) {
                Ability newAblity = npcTarget.AddAbility(Data.abilitiesToAdd[i].AbilityData);
                TrackAbilties(target, newAblity);

            }
            else {
                Ability newChild = target.AbilityManager.LearnAbility(Data.abilitiesToAdd[i].AbilityData, true);

                if (newChild == null) {
                    Debug.LogError("Alrady leared an ability, just unlock it: " + Data.abilitiesToAdd[i].AbilityData.abilityName);
                }

                TrackAbilties(target, newChild);

                Debug.Log("Adding new ability: " + newChild.Data.abilityName);
            }
        }

        return true;
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        if (trackedAbilities.TryGetValue(target, out List<Ability> abilitiesLearned) == true) {

            NPC npcTarget = target as NPC;
            if(npcTarget != null) {
                for (int i = 0; i < abilitiesLearned.Count; i++) {
                    target.RemoveAbility(abilitiesLearned[i]);
                }

            }
            else {
                for (int i = 0; i < abilitiesLearned.Count; i++) {
                    target.AbilityManager.UnlearnAbility(abilitiesLearned[i]);
                }
            }

            trackedAbilities.Remove(target);
        }
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        throw new NotImplementedException();
    }

    private void TrackAbilties(Entity target, Ability newAbility) {
        if (trackedAbilities.TryGetValue(target, out List<Ability> children) == true) {
            children.Add(newAbility);
        }
        else {
            trackedAbilities.Add(target, new List<Ability> { newAbility });
        }
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        throw new NotImplementedException();

    }

    public override string GetTooltip() {
        //return base.GetTooltip();

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < activeAbilities.Count; i++) {
            builder.Append(activeAbilities[i].GetTooltip(false));

            if (i != activeAbilities.Count - 1)
                builder.AppendLine();
        }

        return builder.ToString();
    }
}

public class AddChildAbilityEffect : Effect {

    public override EffectType Type => EffectType.AddChildAbility;


    private Dictionary<Ability, List<Ability>> trackedChildAbilities = new Dictionary<Ability, List<Ability>>();

    private List<Ability> activeAbilities = new List<Ability>();

    public AddChildAbilityEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        for (int i = 0; i < data.abilitiesToAdd.Count; i++) {
            Ability template = AbilityFactory.CreateAbility(data.abilitiesToAdd[i].AbilityData, source);
            activeAbilities.Add(template);
        }

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        throw new NotImplementedException();

    }

    public override void Remove(Entity target) {
        base.Remove(target);
    }


    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;


        //for (int i = 0; i < activeAbilities.Count; i++) {
        //    target.AddChildAbility(activeAbilities[i]);
        //    TrackChildAbilties(target, activeAbilities[i]);
        //}

        for (int i = 0; i < Data.abilitiesToAdd.Count; i++) {
            Ability newChild = target.AddChildAbility(Data.abilitiesToAdd[i]);
            TrackChildAbilties(target, newChild);

            //Debug.Log("Creating child ability: " + newChild.Data.abilityName);
        }

        return true;
    }

    private void TrackChildAbilties(Ability target, Ability child) {
        if (trackedChildAbilities.TryGetValue(target, out List<Ability> children) == true) {
            children.Add(child);
        }
        else {
            trackedChildAbilities.Add(target, new List<Ability> { child });
        }
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        if (trackedChildAbilities.TryGetValue(target, out List<Ability> children) == true) {

            for (int i = 0; i < children.Count; i++) {
                target.RemoveChildAbility(children[i]);
            }

            trackedChildAbilities.Remove(target);
        }

    }

    public override string GetTooltip() {
        //return base.GetTooltip();

        StringBuilder builder = new StringBuilder();

        //Debug.Log("Effect: " + Data.effectName + " : Showing tooltip for an Add Child Ability Effect with: " + activeAbilities.Count + " abilites to add");

        for (int i = 0; i < activeAbilities.Count; i++) {
            builder.Append(activeAbilities[i].GetTooltip(false));



            if (i != activeAbilities.Count - 1)
                builder.AppendLine();
        }

        return builder.ToString();
    }
}

public class ForceStatusTickEffect : Effect {

    public override EffectType Type => EffectType.ForceStatusTick;

    public ForceStatusTickEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if(base.Apply(target) == false)
            return false;

        //Debug.Log("Force Ticking Status on: " + target.EntityName);
        for (int i = 0; i < target.ActiveStatuses.Count; i++) {
            if (target.ActiveStatuses[i].ParentEffect.Data.effectDesignation == StatModifierData.StatModDesignation.PrimaryDamage) {
                target.ActiveStatuses[i].ForceTick();
            }
        }


        //Effect targetEffect = AbilityUtilities.GetEffectByName(
        //    Data.forceTickAbility.AbilityData.abilityName,
        //    Data.forceTickStatusEffect.effectData.effectName, Source, AbilityCategory.Any);

        //if (targetEffect == null) {
        //    Debug.LogError("Could not find effect: " + Data.forceTickStatusEffect.effectData.effectName);
        //    return false;
        //}

        //AddStatusEffect addStatusEffect = targetEffect as AddStatusEffect;

        //if (addStatusEffect == null) {
        //    Debug.LogError("an effect: " + Data.effectName + " tried to force a non-status to tick: " + targetEffect.Data.effectName);
        //    return false;
        //}

        //addStatusEffect.ForceTickOnTarget(target);

        return true;
    }


    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;

        AddStatusEffect addStatusEffect = target as AddStatusEffect;

        if (addStatusEffect == null) {
            Debug.LogError("an effect: " + Data.effectName + " tried to force a non-status to tick: " + target.Data.effectName);
            return false;
        }

        addStatusEffect.ForceTick();
        return true;
    }
}

public class AddStatusEffect : Effect {

    public override EffectType Type => EffectType.AddStatus;

    private Dictionary<Entity, List<Status>> activeStatusDict = new Dictionary<Entity, List<Status>>();

    public List<StatAdjustmentEffect> activeStatusEffects = new List<StatAdjustmentEffect>();

    public AddStatusEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

        //float duration = Stats.Contains(StatName.StatusLifetime) == true ? Stats[StatName.StatusLifetime] : data.statusToAdd[0].duration;
        //float interval = Stats.Contains(StatName.StatusInterval) == true ? Stats[StatName.StatusInterval] : data.statusToAdd[0].interval;

        //SimpleStat durationStat = new SimpleStat(StatName.StatusLifetime, duration /*data.statusToAdd[0].duration*/);
        //SimpleStat intervalStat = new SimpleStat(StatName.StatusInterval, interval /*data.statusToAdd[0].interval*/);

        float stackValue = data.statusToAdd[0].maxStacks > 0 ? data.statusToAdd[0].maxStacks : float.MaxValue;

        StatRange stacksStat = new StatRange(StatName.StackCount, 0, stackValue, data.statusToAdd[0].initialStackCount);

        Stats.AddStat(stacksStat);
        //Stats.AddStat(durationStat);
        //Stats.AddStat(intervalStat);

        //Debug.Log("creating an add status effect for " + parentAbility.Data.abilityName);
        for (int i = 0; i < data.statusToAdd.Count; i++) {
            Effect statusEffect = AbilityFactory.CreateEffect(data.statusToAdd[i].statusEffectDef.effectData, source, ParentAbility);
            activeStatusEffects.Add(statusEffect as StatAdjustmentEffect);

            //Debug.Log("Creating status adj effect for a status: " + ((StatAdjustmentEffect)statusEffect).GetTooltip());
        }
    }

    public float GetModifiedStatusDuration() {
        float effectDurationModifier = 1 + Source.Stats[StatName.GlobalStatusDurationModifier];

        return Stats[StatName.StatusLifetime] * effectDurationModifier;
    }

    public float GetModifiedIntervalDuration() {
        float effectIntervalModifier = 1 + Source.Stats[StatName.GlobalStatusIntervalModifier];

        return Stats[StatName.StatusInterval] * effectIntervalModifier;
    }

    public void ForceTickOnTarget(Entity target) {
        if(activeStatusDict.TryGetValue(target, out List<Status> activeStatuses) == true) {
            for (int i = 0; i < activeStatuses.Count; i++) {
                activeStatuses[i].ForceTick();
            }
        }
    }

    public void ForceTick() {

        for (int i = activeStatusDict.Count - 1; i >= 0; i--) {
            List<Status> checks = activeStatusDict.ElementAt(i).Value;

            for (int j = checks.Count - 1; j >= 0; j--) {
                checks[j].ForceTick();
            }
        }

        //foreach (var entry in activeStatusDict) {
        //    for (int i = entry.Value.Count -1; i >= 0; i--) {
        //        entry.Value[i].ForceTick();
        //    }
        //}
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        if (activeStatusDict.TryGetValue(target, out List<Status> statusList) == true) {

            for (int j = 0; j < statusList.Count; j++) {
                statusList[j].Stack();
            }

        }
        else {
            ApplyNewStatus(target);
            //Debug.Log("applying a status to: " + target.EntityName);
        }

        return true;
    }


    private void ApplyNewStatus(Entity target) {
        for (int i = 0; i < activeStatusEffects.Count; i++) {

            StatAdjustmentEffect statusEffect = new StatAdjustmentEffect(activeStatusEffects[i].Data, activeStatusEffects[i].Source, activeStatusEffects[i], activeStatusEffects[i].ParentAbility);

            Status newStatus = new Status(Data.statusToAdd[i], target, Source, statusEffect, this);
            TrackActiveStatus(target, newStatus);
        }
    }

    private void TrackActiveStatus(Entity target, Status status) {
        if (activeStatusDict.TryGetValue(target, out List<Status> statusList)) {
            statusList.Add(status);
        }
        else {
            List<Status> newList = new List<Status>() { status };
            activeStatusDict.Add(target, newList);
        }
    }

    public void OnAffectedTargetDies(Entity target, Entity cause, Status status) {

        EventData eventData = new EventData();
        eventData.AddEntity("Victim", target);
        eventData.AddEntity("Killer", cause);
        eventData.AddAbility("Ability", ParentAbility);
        eventData.AddEffect("Effect", this);

        EventManager.SendEvent(GameEvent.UnitDiedWithStatus, eventData);

    }

    public void CleanUp(Entity target, Effect activeEffect) {
        if (activeStatusDict.ContainsKey(target)) {
            activeStatusDict.Remove(target);
        }

        //activeStatusEffects.Remove(activeEffect);
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        if (activeStatusDict.TryGetValue(target, out List<Status> statusList)) {
            for (int i = 0; i < statusList.Count; i++) {
                statusList[i].Remove(true);
            }

            activeStatusDict.Remove(target);
        }
        //else {
        //    Debug.LogError("[ADD STATUS EFFECT] A target: " + target.gameObject.name + " is not tracked.");
        //}
    }


    public string GetStatusStackingTooltip() {
        StringBuilder builder = new StringBuilder();
        
        float maxStacks = Stats.GetStatRangeMaxValue(StatName.StackCount);

        if (maxStacks < float.MaxValue && 
            Data.statusToAdd[0].stackMethod != StackMethod.None && 
            Data.statusToAdd[0].stackMethod != StackMethod.Infinite) {
            builder.Append("Stacks up to " + Stats.GetStatRangeMaxValue(StatName.StackCount) + " times").AppendLine();
        }
        if (Data.statusToAdd[0].stackMethod == StackMethod.Infinite) {
            builder.Append("Stacks Infinitely");
        }

        if (Data.statusToAdd[0].stackMethod == StackMethod.None) {
            builder.Append("Doesn't Stack");
        }

        return builder.ToString();
    }

    public override string GetTooltip() {
        //return base.GetTooltip();

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < activeStatusEffects.Count; i++) {
            //StatusData statusData = Data.statusToAdd[i];
            //EffectData effectData = Data.statusToAdd[i].statusEffectDef.effectData;

            switch (activeStatusEffects[i].Data.effectDesignation) {
                case StatModifierData.StatModDesignation.None:
                    //builder.AppendLine();

                    //Debug.Log("Showing a tooltip for a non damage status");
                    //Debug.Log(activeStatusEffects[i].GetTooltip());

                    builder.AppendLine(activeStatusEffects[i].GetTooltip());

                    float duration = GetModifiedStatusDuration();

                    if (duration > 0) {
                        builder.AppendLine();
                        builder.AppendLine("Duration: " + TextHelper.ColorizeText(duration.ToString(), Color.yellow) + " seconds");
                    }


                    builder.Append(GetStatusStackingTooltip());


                    //float maxStacks = Stats.GetStatRangeMaxValue(StatName.StackCount);

                    //if(maxStacks < float.MaxValue && Data.statusToAdd[0].stackMethod != StackMethod.None) {
                    //    builder.Append("Stacks up to " + Stats.GetStatRangeMaxValue(StatName.StackCount) + " times").AppendLine();
                    //}
                    //if(Data.statusToAdd[0].stackMethod == StackMethod.Infinite) {
                    //    builder.Append("Stacks Infinitely");
                    //}

                    //if (Data.statusToAdd[0].stackMethod == StackMethod.None) {
                    //    builder.Append("Doesn't Stack");
                    //}

                   


                    break;
                case StatModifierData.StatModDesignation.PrimaryDamage:

                    //builder.AppendLine();


                    if(Data.showScalers == true) {
                        string scalarTooltip = activeStatusEffects[i].ScalarTooltip();
                        builder.AppendLine("Scales From: ");
                        builder.Append(scalarTooltip).AppendLine();
                    }

                    float damageRatio = activeStatusEffects[i].GetWeaponScaler();
                    //TextHelper.ColorizeText((damagePercent * 100).ToString() + "%", Color.green)

                    string durationText = TextHelper.ColorizeText(GetModifiedStatusDuration().ToString(), Color.yellow) + " seconds";
                    string intervalText = TextHelper.ColorizeText(GetModifiedIntervalDuration().ToString(), Color.yellow) + " seconds";

                    string durationReplacement = GetModifiedStatusDuration() > 0f ? durationText : TextHelper.ColorizeText("Eternity", Color.yellow);


                    if (damageRatio > 0) {
                        builder.Append("Causes " + TextHelper.ColorizeText((damageRatio * 100).ToString() + "%", Color.green)
                       + " of Weapon Damage every " + intervalText + " for "
                       + durationReplacement);

                    }
                    else {
                        builder.Append(activeStatusEffects[i].GetTooltip() + "for " + durationText);
                    }


                    if (Data.statusToAdd[0].maxStacks > 0) {
                        builder.AppendLine();
                        builder.Append(GetStatusStackingTooltip());
                        builder.AppendLine();
                        //builder.Append("Stacks up to " + Stats.GetStatRangeMaxValue(StatName.StackCount) + " times").AppendLine();
                    }

                    if (activeStatusEffects[i].Data.canOverload == true) {
                        float overloadChance = ParentAbility != null ? ParentAbility.GetAbilityOverloadChance() : Source.Stats[StatName.OverloadChance];

                        builder.AppendLine();
                        builder.Append("Overload Chance: " + TextHelper.ColorizeText(TextHelper.FormatStat(StatName.OverloadChance, overloadChance), Color.green));
                    }



                   

                    string projectileStats = GetProjectileStatsTooltip();
                    if (string.IsNullOrEmpty(projectileStats) == false) {
                        builder.AppendLine(projectileStats);
                    }


                    break;
                case StatModifierData.StatModDesignation.SecondaryDamage:
                    break;
                case StatModifierData.StatModDesignation.ShotCount:
                    break;
                default:
                    break;
            }

            //for (int j = 0; j < Data.statusToAdd[i].statusEffectDef.effectData.modData.Count; j++) {

            //}
        }

        return builder.ToString();

        //StatModifierData statusModData = Data.statusToAdd[0].statusEffectDef.effectData.modData[0];

        //string formated = TextHelper.FormatStat(statusModData.targetStat, statusModData.value);

        //string replacement = Data.effectDescription.Replace("{}", formated);

        //return replacement;

    }



}


public class AddStatusScalingEffect : Effect {

    public override EffectType Type => EffectType.AddStatusScaling;

    public Dictionary<Effect, List<StatModifierData.StatusModifier>> activeStatusModifiers = new Dictionary<Effect, List<StatModifierData.StatusModifier>>();

    public AddStatusScalingEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        //Debug.Log("Creating a status scaling effect: " + data.statusScalingData.Count);
    }

    public override bool Apply(Entity target) {
        Debug.LogError("Add Status Scaling doesn't yet support targeting Entities");
        return false;
    }

    public override void RegisterEvents() {
        base.RegisterEvents();
        EventManager.RegisterListener(GameEvent.AbilityLevelChanged, OnAbilityLevelChanged);

    }

    private void OnAbilityLevelChanged(EventData data) {
        Ability ability = data.GetAbility("Ability");

        if (ability != ParentAbility)
            return;

        ResetApplicationToEffects();
    }

    public void ResetApplicationToEffects() {
        for (int i = EffectTargets.Count - 1; i >= 0; i--) {
            Effect target = EffectTargets[i];

            RemoveFromEffect(target);
            ApplyToEffect(target);
        }
    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;

        Debug.Log("Applyin: " + Data.effectName);

        StatAdjustmentEffect adjustmentEffect = target as StatAdjustmentEffect;

        if(adjustmentEffect == null) {
            Debug.LogError("Tried to add status scaling to a non-stat adjustment effect");
            return false;
        }

        for (int i = 0; i < Data.statusScalingData.Count; i++) {
            
            if(Data.scaleFromAbilityLevel == true) {
                float modValue = Data.statusScalingData[i].modifierValue * ParentAbility.AbilityLevel;
                StatModifierData.StatusModifier modifier = new StatModifierData.StatusModifier(Data.statusScalingData[i].status, modValue);
                TrackStatusScaling(target, modifier);
                adjustmentEffect.statusModifiers.Add(modifier);

            }
            else {
                TrackStatusScaling(target, Data.statusScalingData[i]);
                adjustmentEffect.statusModifiers.Add(Data.statusScalingData[i]);
            }
        }

        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        StatAdjustmentEffect adjustmentEffect = target as StatAdjustmentEffect;

        if (activeStatusModifiers.TryGetValue(target, out List<StatModifierData.StatusModifier> modifierList)) {
            for (int i = 0; i < modifierList.Count; i++) {
                adjustmentEffect.statusModifiers.Remove(modifierList[i]);
            }
        }

        activeStatusModifiers.Remove(target);

    }

    private void TrackStatusScaling(Effect target, StatModifierData.StatusModifier modifier) {
        if (activeStatusModifiers.TryGetValue(target, out List<StatModifierData.StatusModifier> list) == true) {
            list.Add(modifier);
        }
        else {
            activeStatusModifiers.Add(target, new List<StatModifierData.StatusModifier> { modifier });
        }

    }



    public override string GetTooltip() {
        StringBuilder builder = new StringBuilder();

        //string bonusColor = UnityEngine.ColorUtility.ToHtmlStringRGB(new Color(.439f, .839f, 0.11f));
        //string penaltyColor = UnityEngine.ColorUtility.ToHtmlStringRGB(new Color(0.839f, 0.235f, 0.11f));

        List<string> results = new List<string>();
        foreach (var entry in Data.statusScalingData) {
            string status = TextHelper.ColorizeText(entry.status.ToString(), Color.magenta);

            float damageValue = Data.scaleFromAbilityLevel == false ? entry.modifierValue : entry.modifierValue * ParentAbility.AbilityLevel;

            string damage = TextHelper.ColorizeText((damageValue * 100).ToString(), new Color(.439f, .839f, 0.11f));

            string result = Data.effectDescription + " deals " + damage + "% more damage to " + status + " targets.";

            results.Add(result);
        }

        for (int i = 0; i < results.Count; i++) {
            builder.AppendLine(results[i]);
        }



        return builder.ToString();

    }


}

public class SpawnProjectileEffect : Effect {

    public override EffectType Type => EffectType.SpawnProjectile;

    public SpawnProjectileEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        List<Projectile> activeTokens = CreateTokens(target);

        for (int i = 0; i < activeTokens.Count; i++) {

            SetupChildProjectile(activeTokens[i], target);
        }

        return true;
    }

    private void SetupChildProjectile(Projectile child, Entity target) {

        if (child == null) {
            Debug.LogError("A Null projectile was sent to SetupChildProjectile");
            return;
        }

        child.Setup(Source, null, null);
        child.IgnoreCollision(target);
        SetupChildRotation(child, target);
    }

    private void SetupChildRotation(Projectile child, Entity firstTarget) {
        List<GameObject> otherTargets = GatherNewTargets(firstTarget);

        if (otherTargets.Count > 0) {
            int randomIndex = Random.Range(0, otherTargets.Count);
            GameObject randomTarget = otherTargets[randomIndex];

            Quaternion rot = TargetUtilities.GetRotationTowardTarget(randomTarget.transform, child.transform);
            child.transform.transform.rotation = rot;
        }
    }


    private List<GameObject> GatherNewTargets(Entity firstTarget) {
        List<GameObject> results = new List<GameObject>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(Source.transform.position, Data.overlapCircleRadius, Data.overlapLayerMask);

        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i].gameObject != firstTarget.gameObject) {
                results.Add(colliders[i].gameObject);
            }
        }

        return results;
    }

    private List<Projectile> CreateTokens(Entity target) {

        List<Projectile> results = new List<Projectile>();

        for (int i = 0; i < Data.tokenPrefabs.Count; i++) {
            Projectile token = GameObject.Instantiate(Data.tokenPrefabs[i]);
            switch (Data.tokenSpawnLocation) {
                case DeliverySpawnLocation.Source:
                    token.transform.localPosition = Source.transform.position;
                    break;
                case DeliverySpawnLocation.MousePointer:
                    Vector3 mousPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    token.transform.localPosition = mousPos;
                    break;
                case DeliverySpawnLocation.AITarget:
                    token.transform.localPosition = target.transform.position;
                    break;

                case DeliverySpawnLocation.Trigger:
                    token.transform.localPosition = targeter.ActivationInstance.TriggeringEntity.transform.position;
                    break;
                default:
                    token.transform.localPosition = Source.transform.position;
                    break;
            }

            results.Add(token);
        }

        return results;
    }


}

public class SpawnEntityEffect : Effect {
    public override EffectType Type => EffectType.SpawnEntity;


    public List<Entity> activeSpawns = new List<Entity>();

    public SpawnEntityEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {



    }


    public override void RegisterEvents() {
        base.RegisterEvents();
        EventManager.RegisterListener(GameEvent.UnitDied, OnUnitDied);
        EventManager.RegisterListener(GameEvent.EffectStatAdjusted, OnStatChanged);
    }

    public override void UnregisterEvents() {
        base.UnregisterEvents();
        EventManager.RemoveMyListeners(this);
    }

    private void OnStatChanged(EventData data) {
        Effect target = data.GetEffect("Effect");

        if (target != this)
            return;

        StatName stat = (StatName)data.GetInt("Stat");

        if (stat == StatName.MaxMinionCount) {
            if (activeSpawns.Count > Stats[StatName.MaxMinionCount]) {
                //Debug.LogError("Too many spawns");
                activeSpawns[0].ForceDie(null);
            }
        }
    }
    private void OnUnitDied(EventData data) {
        //Entity cause =  data.GetEntity("Killer");
        Ability causingAbility = data.GetAbility("Ability Cause");
        if (causingAbility == ParentAbility)
            return;

        Entity victim = data.GetEntity("Victim");

        activeSpawns.RemoveIfContains(victim);

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;


        int maxSpawns = Stats[StatName.MaxMinionCount] > 0 ? (int)Stats[StatName.MaxMinionCount] : -1;

        if (Source.Stats[StatName.MaxMinionCount] > 0 && maxSpawns > 0) {
            maxSpawns += (int)Source.Stats[StatName.MaxMinionCount];
        }

        int spawnCount = Stats[StatName.MinionSpawnCount] > 0 ? (int)Stats[StatName.MinionSpawnCount] : 1;

        if (maxSpawns > 0 && activeSpawns.Count >= maxSpawns) {
            //Debug.LogWarning("Spawn count reached");

            if (Data.destroyPreviousSummonAtCap == true) {
                Entity oldest = activeSpawns[0];

                Projectile projectile = oldest as Projectile;
                if (projectile != null) {
                    activeSpawns.Remove(projectile);
                    projectile.StartCleanUp();
                }
                else {
                    activeSpawns.Remove(oldest);
                    oldest.ForceDie(Source, ParentAbility);

                }
            }
            else {
                return false;
            }

        }



        int unitsToSpawn = spawnCount; /*maxSpawns > 0 ? spawnCount - activeSpawns.Count : spawnCount;*/

        if (maxSpawns > 0 && spawnCount + activeSpawns.Count > maxSpawns) {
            unitsToSpawn = spawnCount - activeSpawns.Count;
        }

        //Debug.Log((spawnCount - activeSpawns.Count).ToString() + " is how many I should spawn");

        //Debug.Log("Spawning: " + unitsToSpawn + " units. Max: " + maxSpawns + " Current: " + activeSpawns.Count + " SpawnCount: " + spawnCount);

        for (int i = 0; i < unitsToSpawn; i++) {
            Entity spawn = PerformSpawn(target);
            spawn.ownerType = Source.ownerType;
            spawn.entityType = Source.entityType;
            if (spawn is NPC) {
                ((NPC)spawn).MinionMaster = Source;
            }

            if (Data.inheritParentLayer == true)
                spawn.gameObject.layer = Source.gameObject.layer;

            spawn.subtypes.Add(Entity.EntitySubtype.Minion);

            if (spawn.innerSprite != null)
                VFXUtility.DesaturateSprite(spawn.innerSprite, 0.4f);

            EntityPlayer player = Source as EntityPlayer;
            if (player != null) {
                float averageDamage = player.GetAverageDamageRoll() * Data.percentOfPlayerDamage;
                float modifiedDamage = averageDamage * (1 + player.Stats[StatName.MinionDamageModifier]);
                spawn.Stats.SetStatValue(StatName.AbilityWeaponCoefficicent, modifiedDamage, Source);
            }
            else {
                float baseDamage = Source.Stats[StatName.AbilityWeaponCoefficicent] * Data.percentOfPlayerDamage;
                float modifiedDamage = baseDamage * (1 + Source.Stats[StatName.MinionDamageModifier]);
                spawn.Stats.SetStatValue(StatName.AbilityWeaponCoefficicent, modifiedDamage, Source);
            }

            //NPC npc = spawn as NPC;
            //if(npc != null) {
            //    npc.Brain.Sensor.RemoveFromDetectionMask(Source.gameObject.layer);
            //}

            EntityTargets.Add(spawn);
            LastTarget = spawn;
            activeSpawns.Add(spawn);


            //Debug.Log("Spawning: " + spawn.EntityName);
            //Debug.Log("Health of Spawn: " + spawn.Stats[StatName.Health]);

            SendMinionSpawnedEvent(spawn);
        }

        return true;
    }

    private void SendMinionSpawnedEvent(Entity minion) {
        EventData data = new EventData();
        data.AddEntity("Minion", minion);
        data.AddEntity("Cause", Source);
        data.AddEffect("Causing Effect", this);
        data.AddAbility("Causing Ability", ParentAbility);

        EventManager.SendEvent(GameEvent.MinionSummoned, data);
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        for (int i = 0; i < activeSpawns.Count; i++) {
            if (activeSpawns[i] != null)
                activeSpawns[i].ForceDie(Source, ParentAbility);
        }

        activeSpawns.Clear();

    }

    private Entity PerformSpawn(Entity target) {



        Entity result = Data.spawnType switch {
            EntitySpawnType.Manual => GameObject.Instantiate(Data.entityPrefab, GetSpawnLocation(), Quaternion.identity),
            EntitySpawnType.Clone => GameObject.Instantiate(NPCDataManager.GetNPCPrefabByName(target.EntityName), GetSpawnLocation(), Quaternion.identity),
            EntitySpawnType.Series => throw new NotImplementedException(),
            _ => null,
        };

        result.SpawningAbility = ParentAbility;

        return result;

    }

    private Vector2 GetSpawnLocation() {

        Vector2 location = targeter.GetPayloadSpawnLocation();

        Vector2 nearby = location + Random.insideUnitCircle * Random.Range(2f, 4f);

        return nearby;
    }


    public override string GetTooltip() {

        StringBuilder builder = new StringBuilder();

        int maxSpawns = Stats[StatName.MaxMinionCount] > 0 ? (int)Stats[StatName.MaxMinionCount] : -1;

        if (Source.Stats[StatName.MaxMinionCount] > 0 && maxSpawns > 0) {
            maxSpawns += (int)Source.Stats[StatName.MaxMinionCount];
        }


        string replacement = Data.effectDescription.Replace("{}", TextHelper.ColorizeText(maxSpawns.ToString(), Color.green));

        string weaponPercent = TextHelper.ColorizeText(TextHelper.RoundTimeToPlaces(Data.percentOfPlayerDamage * 100f, 2) + "%", Color.green);

        string damageReplacement = replacement.Replace("{P}", weaponPercent);

        builder.AppendLine(damageReplacement);

        return builder.ToString();
    }

}

public class StatAdjustmentEffect : Effect {

    public override EffectType Type => EffectType.StatAdjustment;

    private Dictionary<Entity, List<StatModifier>> trackedEntityMods = new Dictionary<Entity, List<StatModifier>>();
    private Dictionary<Ability, List<StatModifier>> trackedAbilityMods = new Dictionary<Ability, List<StatModifier>>();
    private Dictionary<Effect, List<StatModifier>> trackedEffectMods = new Dictionary<Effect, List<StatModifier>>();

    private List<StatModifierData> modData = new List<StatModifierData>();

    public List<StatModifierData.StatusModifier> statusModifiers = new List<StatModifierData.StatusModifier>();


    public StatAdjustmentEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

        modData = new List<StatModifierData>();

        for (int i = 0; i < data.modData.Count; i++) {
            StatModifierData clonedModdata = new StatModifierData(data.modData[i]);
            modData.Add(clonedModdata);
        }

        for (int i = 0; i < modData.Count; i++) {
            modData[i].SetupEffectStats();
        }

        statusModifiers = new List<StatModifierData.StatusModifier>();
        for (int i = 0; i < data.statusModifiers.Count; i++) {
            this.statusModifiers.Add(new StatModifierData.StatusModifier(data.statusModifiers[i]));
        }

    }

    public StatAdjustmentEffect(EffectData data, Entity source, StatAdjustmentEffect clone, Ability parentAbility = null) : base(data, source, parentAbility) {

        for (int i = 0; i < clone.modData.Count; i++) {

            StatModifierData clonedModData = new StatModifierData(clone.modData[i]);
            modData.Add(clonedModData);
            modData[i].CloneEffectStats(clone.modData[i]);
        }
    }


    public override void RegisterEvents() {
        base.RegisterEvents();
        EventManager.RegisterListener(GameEvent.AbilityLevelChanged, OnAbilityLevelChanged);
        
    }

    private void OnAbilityLevelChanged(EventData data) {
        Ability ability = data.GetAbility("Ability");

        if (ability != ParentAbility)
            return;

        ResetApplicationToAbilities();
        ResetApplicationToEffects();
    }

    public override void Stack(Status status) {
        for (int i = 0; i < modData.Count; i++) {

            modData[i].Stats.RemoveAllModifiersFromSource(status);

            StatName targetStat = StatName.Vitality;
            if (modData[i].modValueSetMethod == StatModifierData.ModValueSetMethod.DerivedFromMultipleSources) {
                targetStat = StatName.AbilityWeaponCoefficicent;

                modData[i].RemoveAllscalerModsFromSource(targetStat, status);

                StatModifier stackMultiplier1 = new StatModifier(status.StackCount - 1, StatModType.PercentAdd, targetStat, status, modData[i].variantTarget);
                modData[i].AddScalerMod(targetStat, stackMultiplier1);

                return;
            }

            if (modData[i].modValueSetMethod == StatModifierData.ModValueSetMethod.Manual) {
                targetStat = StatName.StatModifierValue;
            }

            StatModifier stackMultiplier = new StatModifier(status.StackCount - 1, StatModType.PercentAdd, targetStat, status, modData[i].variantTarget);
            modData[i].Stats.AddModifier(stackMultiplier.TargetStat, stackMultiplier);

            //Status with no Interval don't tick past the first time, so they need to be updated with the new value
            if(status.Data.interval == 0) {
                Remove(status.Target);
                status.ForceTick();
            }


            //Debug.Log("Stacking a stat mod for : " + modData[i].targetStat + ". Stack Count: " + (status.StackCount - 1));
            //Debug.Log("Stacking: " + Data.effectName + ". Value: " + modData[i].Stats[targetStat]);
        }
    }

    public void AddStatAdjustmentModifier(StatModifier mod) {
        for (int i = 0; i < modData.Count; i++) {
            StatAdjustmentManager.ApplyDataModifier(modData[i], mod);
            //Debug.LogWarning("Applying: " + mod.TargetStat + " Modifier: " + mod.Value + " to " + Data.effectName);
        }
    }

    public void RemoveStatAdjustmentModifier(StatModifier mod) {
        for (int i = 0; i < modData.Count; i++) {
            StatAdjustmentManager.RemoveDataModifiyer(modData[i], mod);
            //Debug.LogWarning("Removing: " + mod.TargetStat + " Modifier: " + mod.Value + " to " + Data.effectName);
        }
    }

    public void AddScaler(StatScaler scaler) {
        for (int i = 0; i < modData.Count; i++) {

            modData[i].AddScaler(scaler);

            //for (int j = 0; j < modData[i].scalers.Count; j++) {
            //    if (modData[i].scalers[j].targetStat == scaler.targetStat) {
            //        Debug.LogError("Duplicate stat scaler: " + scaler.targetStat + ". this is not supported");
            //        return;
            //    }
            //}

            //modData[i].scalers.Add(scaler);
            //scaler.InitStat();

            Debug.Log("Adding a scaler for: " + scaler.targetStat);
        }
    }

    public void RemoveScaler(StatScaler scaler) {
        for (int i = 0; i < modData.Count; i++) {
            modData[i].RemoveScaler(scaler);
        }
    }

    public void AddScalerMod(StatName targetStat, StatModifier mod) {
        for (int i = 0; i < modData.Count; i++) {
            modData[i].AddScalerMod(targetStat, mod);
        }
    }

    public void RemoveScalerMod(StatName targetStat, StatModifier mod) {
        for (int i = 0; i < modData.Count; i++) {
            modData[i].RemoveScalerMod(targetStat, mod);
        }
    }

    public float GetModifierValue(StatName name) {
        for (int i = 0; i < modData.Count; i++) {
            if (modData[i].targetStat == name)
                return modData[i].Stats[name];
        }

        return 0f;
    }

    public float GetWeaponScaler() {

        float result = -1f;

        for (int i = 0; i < modData.Count; i++) {

            result = modData[i].GetWeaponScaler();

            if (result > 0) {
                return result;
            }
        }

        return result;
    }

    public Dictionary<StatName, float> GetAllScalerValues() {
        Dictionary<StatName, float> results = new Dictionary<StatName, float>();

        for (int i = 0; i < modData.Count; i++) {

            Dictionary<StatName, float> values = modData[i].GetAllScalerValues();

            foreach (var entry in values) {
                results.Add(entry.Key, entry.Value);
            }
        }

        return results;
    }

    private float DeriveModValueFromOtherStat(StatModifierData modData, Entity entityTarget, Effect effectTarget, Ability abilityTarget) {

        float result = modData.deriveTarget switch {
            StatModifierData.DeriveFromWhom.Source => Source.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.Cause => currentTriggerInstance.CauseOfTrigger.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.Trigger => currentTriggerInstance.TriggeringEntity.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.OtherEntityTarget => throw new NotImplementedException(),
            StatModifierData.DeriveFromWhom.CurrentEntityTarget => entityTarget.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.CurrentAbilityTarget => abilityTarget.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.CurrentEffectTarget => GetModifiedStatValue(effectTarget.Stats, modData.derivedTargetStat), /*effectTarget.Stats[modData.derivedTargetStat],*/
            StatModifierData.DeriveFromWhom.OtherEffect => throw new NotImplementedException(),
            StatModifierData.DeriveFromWhom.OtherAbility => throw new NotImplementedException(),
            StatModifierData.DeriveFromWhom.SourceAbility => ParentAbility.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.SourceEffect => Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.TriggerAbility => currentTriggerInstance.TriggeringAbility.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.TriggerEffect => currentTriggerInstance.TriggeringEffect.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.CauseAbility => currentTriggerInstance.CausingAbility.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.CauseEffect => currentTriggerInstance.CausingEffect.Stats[modData.derivedTargetStat],
            StatModifierData.DeriveFromWhom.WeaponDamage when Source is EntityPlayer => EntityManager.ActivePlayer.CurrentDamageRoll * modData.Stats[StatName.AbilityWeaponCoefficicent],
            StatModifierData.DeriveFromWhom.WeaponDamage when Source is NPC => Source.Stats[StatName.AbilityWeaponCoefficicent],

            _ => 0f,
        };

        result *= modData.deriveStatMultiplier;

        Debug.Log("Mod result: " + result);

        return modData.invertDerivedValue == false ? result : -result;
    }

    private float GetModifiedStatValue(StatCollection stats, StatName stat, bool checkProjectile = false) {

        float statValue = checkProjectile == true && activeDelivery != null ? activeDelivery.Stats[stat] : stats[stat];

        //Debug.Log(stat + " Value: " + statValue);

        float modifier = stat switch {
            StatName.EffectSize => Source.Stats[StatName.GlobalEffectSizeModifier],
            StatName.ProjectileSize => Source.Stats[StatName.GlobalProjectileSizeModifier],
            _ => 1f,
        };

        //Debug.Log("Global Mod: " + modifier);

        return statValue * (1f + modifier);

    }

    private float GetTotalDerivedValue(Entity entityTarget, Effect effectTarget, Ability abilityTarget, StatModifierData modData) {
        float totalDerivedValue = 0f;
        //float projectileStatContrabution = 0f;
        foreach (var entry in modData.scalersDict) {
            float result = entry.Value.deriveTarget switch {
                StatModifierData.DeriveFromWhom.Source => Source.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.Cause => currentTriggerInstance.CauseOfTrigger.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.Trigger => currentTriggerInstance.TriggeringEntity.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.OtherEntityTarget => throw new NotImplementedException(),
                StatModifierData.DeriveFromWhom.CurrentEntityTarget => entityTarget.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.CurrentAbilityTarget => abilityTarget.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.CurrentEffectTarget => GetModifiedStatValue(effectTarget.Stats, entry.Value.targetStat), /*effectTarget.Stats[modData.derivedTargetStat],*/
                StatModifierData.DeriveFromWhom.OtherEffect => throw new NotImplementedException(),
                StatModifierData.DeriveFromWhom.OtherAbility => throw new NotImplementedException(),
                StatModifierData.DeriveFromWhom.SourceAbility => ParentAbility.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.SourceEffect => GetModifiedStatValue(Stats, entry.Value.targetStat, true),
                StatModifierData.DeriveFromWhom.TriggerAbility => currentTriggerInstance.TriggeringAbility.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.TriggerEffect => currentTriggerInstance.TriggeringEffect.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.CauseAbility => currentTriggerInstance.CausingAbility.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.CauseEffect => currentTriggerInstance.CausingEffect.Stats[entry.Value.targetStat],
                StatModifierData.DeriveFromWhom.WeaponDamage when Source is EntityPlayer => EntityManager.ActivePlayer.CurrentDamageRoll /* modData.Stats[StatName.AbilityWeaponCoefficicent]*/,
                StatModifierData.DeriveFromWhom.WeaponDamage when Source is NPC => Source.Stats[StatName.AbilityWeaponCoefficicent],
                StatModifierData.DeriveFromWhom.AbilityLevel => ParentAbility.AbilityLevel,
                _ => 0f,
            };

            result *= entry.Value.scalerStat.ModifiedValue;

            //if (Source is EntityPlayer) {
            //    Debug.Log(entry.Value.scalerStat.ModifiedValue + " is the scaler for: " + entry.Key + " Effect: " + Data.effectName);

            //    //Debug.Log("Mods on scaler found: " + entry.Value.scalerStat.ModCount);

            //    Debug.Log(result + " is the after scaler value for: " + entry.Value.targetStat);
            //}

            totalDerivedValue += result;

            //projectileStatContrabution += GetProjectileStatContrabution(entry.Value.targetStat, entry.Value.scalerStat.ModifiedValue);

            //Debug.Log("Projectile " + entry.Value.targetStat + " contrabution: " + projectileStatContrabution);

            //totalDerivedValue += projectileStatContrabution;
        }

        return totalDerivedValue;
    }

    private float SetModValues(Entity entityTarget, Effect effectTarget, Ability abilityTarget, StatModifier activeMod, StatModifierData modData) {

        float targetValue = modData.modValueSetMethod switch {
            StatModifierData.ModValueSetMethod.Manual => modData.Stats[StatName.StatModifierValue],
            StatModifierData.ModValueSetMethod.DeriveFromOtherStats => DeriveModValueFromOtherStat(modData, entityTarget, effectTarget, abilityTarget),
            StatModifierData.ModValueSetMethod.DeriveFromNumberOfTargets => throw new System.NotImplementedException(),
            StatModifierData.ModValueSetMethod.HardSetValue => throw new System.NotImplementedException(),
            StatModifierData.ModValueSetMethod.HardReset => throw new System.NotImplementedException(),
            StatModifierData.ModValueSetMethod.DeriveFromWeaponDamage => EntityManager.ActivePlayer.CurrentDamageRoll * modData.Stats[StatName.AbilityWeaponCoefficicent],
            StatModifierData.ModValueSetMethod.DerivedFromMultipleSources => GetTotalDerivedValue(entityTarget, effectTarget, abilityTarget, modData),
            _ => 0f,
        };

        if(modData.scaleFromAbilityLevel == true) {
            float levelModifier = ParentAbility.AbilityLevel * modData.abilityLevelCoefficient;
            targetValue *= levelModifier;
        }

        if (activeMod.TargetStat == StatName.Health) {

            if (activeDelivery != null) {
                float projectileContrabution = 1f + activeDelivery.Stats[StatName.ProjectileEffectContrabution];
                targetValue *= projectileContrabution;
            }
        }

        return modData.invertDerivedValue == false ? targetValue : -targetValue;
    }

    private void ApplyToEntity(Entity target, StatModifier activeMod) {

        float globalDamageMultiplier = GetDamageModifier(activeMod, target);

        bool maxHealthAdj = activeMod.TargetStat == StatName.Health && activeMod.VariantTarget == StatModifierData.StatVariantTarget.RangeMax;

        if (activeMod.TargetStat != StatName.Health || maxHealthAdj) {
            StatAdjustmentManager.ApplyStatAdjustment(target, activeMod, activeMod.TargetStat, activeMod.VariantTarget, ParentAbility, globalDamageMultiplier, Data.addMissingStatIfNotPresent, activeDelivery);
            return;

        }

        //Debug.LogWarning("applying a stat adjustment: " + activeMod.Value + " from " + ParentAbility.Data.abilityName);


        float vulnerabilityMod = GetVulnerabilityModifier(target, activeMod.Value );

        float incommingDamage = activeMod.Value * vulnerabilityMod * globalDamageMultiplier;

        //if (target is NPC) {
        //    Debug.Log("Raw Incoming Damage: " + MathF.Round(activeMod.Value, 1));
        //    Debug.Log("Vulnerability Mod: " + vulnerabilityMod);
        //    Debug.Log("Total: " + MathF.Round(incommingDamage, 1));
        //}


        float damageAfterArmor = HandleArmor(target, incommingDamage);

        float damageAfterManaShield = CheckManaShield(target, damageAfterArmor);

        activeMod.UpdateModValue(damageAfterManaShield);

        float modValueResult = StatAdjustmentManager.ApplyStatAdjustment(target, activeMod, activeMod.TargetStat, activeMod.VariantTarget, ParentAbility, 1f, false, activeDelivery);

        ShowFloatingtext(activeMod, modValueResult, target.transform.position);
    }

    private float CheckManaShield(Entity target, float incomingdamage) {
        float targetManaShield = target.Stats[StatName.EssenceShield];

        if (targetManaShield > 0f) {
            float leftoverDamage = target.HandleManaShield(incomingdamage, targetManaShield);

            if (leftoverDamage < 0f) {
                //Debug.Log("Damage after mana shield: " + leftoverDamage);
                return leftoverDamage;
            }
            else {
                //Debug.Log("Mana shield absorbed all damage");
                return 0f;
            }
        }

        return incomingdamage;
    }

    private float HandleArmor(Entity target, float incomingDamage) {

        if (incomingDamage > 0f)
            return incomingDamage;

        float targetArmor = target.Stats[StatName.Armor];

        if (targetArmor == 0f) {
            return incomingDamage;
        }

        float softCap = targetArmor;

        if (targetArmor > 0.75f) {
            softCap = 0.75f;
        }

        float armorModifier = 1 - softCap;


        float result = incomingDamage * armorModifier;

        //Debug.Log("Armor is modifing " + incomingDamage + " damage to: " + (armorModifier * 100) + "% . Resulting Damage: " + result);

        return result;
    }

    private float GetVulnerabilityModifier(Entity target, float incomingDamage) {
        if (incomingDamage > 0f)
            return 1f;

        List<StatName> vulnerabilities = AbilityUtilities.ConvertTagsToStats(ParentAbility);

        //Debug.LogWarning(ParentAbility.Data.abilityName + " is an ability checking for vulnerabilities");

        float totalVlunerability = 0f;
        for (int i = 0; i < vulnerabilities.Count; i++) {
            float value = target.Stats[vulnerabilities[i]];

            //Debug.LogWarning(target.EntityName + " is weak to: " + vulnerabilities[i] + " by: " + value);

            //if (value > 0f) {
            //    Debug.LogWarning(target.EntityName + " is weak to: " + vulnerabilities[i] + " by: " + value);
            //}

            totalVlunerability += value;
        }

        float result = 1 + totalVlunerability;

        //if(result > 1f)
        //    Debug.LogWarning("Total Vulnerability Modifier: " + result);

        return result;
    }

    private float HandleResistance(Entity target, float incomingDamage, List<AbilityTag> damageTypes) {

        if (incomingDamage > 0f)
            return incomingDamage;

        float dividedDamageByType = incomingDamage / damageTypes.Count;

        for (int i = 0; i < damageTypes.Count; i++) {
            
        }



        return 0f;
    }

    private void ShowFloatingtext(StatModifier activeMod, float modValueResult, Vector2 position) {
        if (activeMod.TargetStat == StatName.Health && Data.hideFloatingText == false && modValueResult != 0f) {
            //Debug.LogWarning("Damage dealt: " + modValueResult + " : " + Data.effectName);

            FloatingText text = FloatingTextManager.SpawnFloatingText(position, modValueResult.ToString(), 0.75f, isOverloading);

            Gradient targetGrad = isOverloading == false ? Data.floatingTextColor : Data.overloadFloatingTextColor;
            text.SetColor(targetGrad);
        }
    }

    private void RemoveFromEntity(Entity target, StatModifier activeMod) {
        StatAdjustmentManager.RemoveStatAdjustment(target, activeMod, activeMod.VariantTarget, Source, ParentAbility);
    }

    public void ResetApplicationToAbilities() {
        for (int i = AbilityTargets.Count -1; i >= 0; i--) {
            Ability target = AbilityTargets[i];

            RemoveFromAbility(target);
            ApplyToAbility(target);
        }
    }

    public void ResetApplicationToEffects() {
        for (int i = EffectTargets.Count - 1; i >= 0; i--) {
            Effect target = EffectTargets[i];

            RemoveFromEffect(target);
            ApplyDirectlyToEffect(target);
        }
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        for (int i = 0; i < modData.Count; i++) {

            StatModifier activeMod = PrepareStatMod(modData[i], target.Source, null, target);

            if (activeMod.VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackAbilityStatAdjustment(target, activeMod);
            }
            StatAdjustmentManager.AddAbilityModifier(target, activeMod, Data.addMissingStatIfNotPresent);
            //Debug.Log("Applying a : " + modData[i].targetStat + " mod to " + target.Data.abilityName);
        }

        return true;
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        if (trackedAbilityMods.TryGetValue(target, out List<StatModifier> modList)) {
            for (int i = modList.Count - 1; i >= 0; i--) {
                //Debug.LogWarning("Removing a " + modList[i].TargetStat + " mod from " + target.Data.abilityName);
                StatAdjustmentManager.RemoveAbilityModifier(target, modList[i]);
            }

            trackedAbilityMods.Remove(target);
        }
        else {
            Debug.LogError("[Stat Adjustment EFFECT] An ability: " + target.Data.abilityName + " is not tracked.");
        }

    }


    private void ApplyDirectlyToEffect(Effect target) {

        EffectTargets.AddUnique(target);

        for (int i = 0; i < modData.Count; i++) {

            StatModifier activeMod = PrepareStatMod(modData[i], target.Source, target, null);

            if (activeMod.VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackEffectStatAdjustment(target, activeMod);
                //Debug.Log("Tracking a mod: " + activeMod.TargetStat + " on " + target.Data.effectName);
            }

            //if (modData[i].targetStat != StatName.Health) {
            //Debug.Log("Applying a mod of " + modData[i].targetStat + " to " + target.Data.effectName);
            //}

            switch (Data.subTarget) {
                case EffectSubTarget.Effect:
                    StatAdjustmentManager.AddEffectModifier(target, activeMod, Data.addMissingStatIfNotPresent);
                    break;
                case EffectSubTarget.StatModifier:
                    StatAdjustmentEffect adj = target as StatAdjustmentEffect;
                    adj.AddStatAdjustmentModifier(activeMod);
                    break;
                case EffectSubTarget.StatScalerMod:
                    StatAdjustmentEffect adj2 = target as StatAdjustmentEffect;
                    adj2.AddScalerMod(activeMod.TargetStat, activeMod);
                    break;
            }



            //if (Data.subTarget == EffectSubTarget.StatModifier) {
            //    //Debug.LogWarning("Applying a modifier mod: " + activeMod.Value + " to " + target.Data.effectName);
            //    StatAdjustmentEffect adj = target as StatAdjustmentEffect;
            //    adj.AddStatAdjustmentModifier(activeMod);



            //}
            //else {
            //    //Debug.LogWarning("Applying an effect mod");
            //    StatAdjustmentManager.AddEffectModifier(target, activeMod);
            //}
        }
    }

    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;


        if (target is AddStatusEffect) {
            AddStatusEffect statusEffect = target as AddStatusEffect;
            //Debug.Log("Applying a stat adjustment to a status effect: " +Data.effectName);

            if (Data.subTarget == EffectSubTarget.StatModifier || Data.subTarget == EffectSubTarget.StatScalerMod) {


                for (int i = 0; i < statusEffect.activeStatusEffects.Count; i++) {
                    //Debug.Log("Applying a stat adjustment to a status effect's damage : " + Data.effectName);

                    ApplyDirectlyToEffect(statusEffect.activeStatusEffects[i]);
                }
                return true;
            }
        }

        ApplyDirectlyToEffect(target);

        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        if (trackedEffectMods.TryGetValue(target, out List<StatModifier> modList)) {
            for (int i = 0; i < modList.Count; i++) {

                //Debug.Log("Removing an effect. Stat: " + modList[i].TargetStat);

                switch (Data.subTarget) {
                    case EffectSubTarget.Effect:
                        StatAdjustmentManager.RemoveEffectModifier(target, modList[i]);
                        break;
                    case EffectSubTarget.StatModifier:
                        StatAdjustmentEffect adj = target as StatAdjustmentEffect;
                        adj.RemoveStatAdjustmentModifier(modList[i]);
                        break;
                    case EffectSubTarget.StatScalerMod:
                        StatAdjustmentEffect adj2 = target as StatAdjustmentEffect;
                        adj2.RemoveScalerMod(modList[i].TargetStat, modList[i]);
                        break;
                }

                //if (Data.subTarget == EffectSubTarget.StatModifier) {
                //    StatAdjustmentEffect adj = target as StatAdjustmentEffect;
                //    adj.RemoveStatAdjustmentModifier(modList[i]);
                //}
                //else {
                //    StatAdjustmentManager.RemoveEffectModifier(target, modList[i]);
                //}
            }

            trackedEffectMods.Remove(target);
        }
        else {
            Debug.LogError("[Stat Adjustment EFFECT] An effect: " + target.Data.effectName + " is not tracked.");
        }
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        for (int i = 0; i < modData.Count; i++) {
            StatModifier activeMod = PrepareStatMod(modData[i], target, null, null);

            if (activeMod.VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackEntityStatAdjustment(target, activeMod);
            }

            ApplyToEntity(target, activeMod);
        }

        return true;
    }

    private StatModifier PrepareStatMod(StatModifierData modData, Entity target, Effect effectTaget, Ability abilityTarget) {
        StatModifier activeMod = new StatModifier(modData.value, modData.modifierType, modData.targetStat, Source, modData.variantTarget);
        float baseModValue = SetModValues(target, effectTaget, abilityTarget, activeMod, modData);
        activeMod.UpdateModValue(baseModValue);

        return activeMod;
    }

    private float GetDamageModifier(StatModifier mod, Entity target) {

        if (mod.TargetStat != StatName.Health)
            return 1f;

        if (mod.ModType != StatModType.Flat)
            return 1f;

        if (mod.Value > 0f) //Do healing mods here
            return 1f;

        float globalDamageMultiplier = 1 + Source.Stats[StatName.GlobalDamageModifier];

        //Debug.Log(globalDamageMultiplier + " is the global mod");

        if (ParentAbility != null) {
            foreach (AbilityTag tag in ParentAbility.Tags) {
                float value = tag switch {
                    AbilityTag.None => 0f,
                    AbilityTag.Fire => Source.Stats[StatName.FireDamageModifier],
                    AbilityTag.Poison => Source.Stats[StatName.PoisonDamageModifier],
                    AbilityTag.Healing => 0f,
                    AbilityTag.Melee => Source.Stats[StatName.MeleeDamageModifier],
                    AbilityTag.Time => Source.Stats[StatName.TimeDamageModifier],
                    AbilityTag.Arcane => Source.Stats[StatName.ArcaneDamageModifier],
                    AbilityTag.Space => Source.Stats[StatName.SpatialDamageModifier],
                    AbilityTag.Void => Source.Stats[StatName.VoidDamageModifier],

                    _ => 0f,
                };

                globalDamageMultiplier += value;
            }
        }

        if(statusModifiers != null && statusModifiers.Count > 0) {
            float statusModValue = 1f;
            for (int i = 0; i < statusModifiers.Count; i++) {
                if (target.HasStatus(statusModifiers[i].status) == true) {
                    statusModValue += statusModifiers[i].modifierValue;
                }
            }

            globalDamageMultiplier *= statusModValue;
        }



        if (isOverloading == true) {
            float overloadDamageMod = 1f + Source.Stats[StatName.OverloadDamageModifier];
            globalDamageMultiplier *= overloadDamageMod;
        }
        return globalDamageMultiplier;
    }

    private void TrackEntityStatAdjustment(Entity target, StatModifier mod) {
        if (trackedEntityMods.TryGetValue(target, out List<StatModifier> modList)) {
            modList.Add(mod);
        }
        else {
            List<StatModifier> newList = new List<StatModifier>() { mod };
            trackedEntityMods.Add(target, newList);
        }
    }

    private void TrackEffectStatAdjustment(Effect target, StatModifier mod) {
        if (trackedEffectMods.TryGetValue(target, out List<StatModifier> modList)) {
            modList.Add(mod);
        }
        else {
            List<StatModifier> newList = new List<StatModifier>() { mod };
            trackedEffectMods.Add(target, newList);
        }
    }

    private void TrackAbilityStatAdjustment(Ability target, StatModifier mod) {
        if (trackedAbilityMods.TryGetValue(target, out List<StatModifier> modList)) {
            modList.Add(mod);
        }
        else {
            List<StatModifier> newList = new List<StatModifier>() { mod };
            trackedAbilityMods.Add(target, newList);
        }
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        if (trackedEntityMods.TryGetValue(target, out List<StatModifier> modList)) {
            for (int i = modList.Count - 1; i >= 0; i--) {
                RemoveFromEntity(target, modList[i]);
            }

            trackedEntityMods.Remove(target);
        }
        //else {
        //    Debug.LogWarning("[Stat Adjustment EFFECT] A target: " + target.gameObject.name + " is not tracked.");
        //}

    }

    public string ScalarTooltip() {
        Dictionary<StatName, float> scalers = GetAllScalerValues();


        if (scalers.Count == 0) {
            return "No Scalers Found";
        }


        StringBuilder builder = new StringBuilder();

        foreach (var item in scalers) {

            string formatted = TextHelper.FormatStat(item.Key, item.Value);

            builder.AppendLine(formatted + "of " + TextHelper.PretifyStatName(item.Key));

        }

        return builder.ToString();
    }

    public override string GetTooltip() {

        //Debug.Log("Showing a Tooltip for: " + Data.effectName + " on " + ParentAbility.Data.abilityName);

        if(ZoneInfo.applyOnInterval == true) {
            return GetDamageOverTimeTooltip();
        }

        StringBuilder builder = new StringBuilder();

        float value = modData[0].scaleFromAbilityLevel == false ? modData[0].Stats[StatName.StatModifierValue] : modData[0].Stats[StatName.StatModifierValue] * ParentAbility.AbilityLevel;

        string formated = TextHelper.FormatStat(modData[0].targetStat, value);

        string replacement = Data.effectDescription.Replace("{}", formated);

        if (ParentAbility != null) {
            float duration = ParentAbility.GetDuration();

            if (duration > 0) {

                string roundedTime = TextHelper.ColorizeText(TextHelper.RoundTimeToPlaces(duration, 2), Color.yellow);

                string timeReplacment = replacement.Replace("{X}", roundedTime);

                builder.Append(timeReplacment);
                return timeReplacment;
            }
        }

        if (Stats.Contains(StatName.EffectLifetime)) {
            string effectTime = TextHelper.ColorizeText(Stats[StatName.EffectLifetime].ToString(), Color.yellow);

            string durationReplacement = replacement.Replace("{D}", effectTime);
            builder.Append(durationReplacement);

            return builder.ToString();
        }

        builder.Append(replacement);
        return builder.ToString();
    }

    public string GetDamageOverTimeTooltip() {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine();

        //string scalarTooltip = ScalarTooltip();

        //builder.AppendLine("Scales From: ");

        //builder.Append(scalarTooltip).AppendLine();

        float damageRatio = GetWeaponScaler();

        float effectDurationModifier = 1 + Source.Stats[StatName.GlobalEffectDurationModifier];
        float duration = Stats[StatName.EffectLifetime] * effectDurationModifier;

        float intervalDurationModifier = 1 + Source.Stats[StatName.GlobalEffectIntervalModifier];
        float interval = Stats[StatName.EffectInterval] * intervalDurationModifier;
  
        string durationText = TextHelper.ColorizeText(duration.ToString(), Color.yellow) + " seconds";
        string intervalText = TextHelper.ColorizeText(interval.ToString(), Color.yellow) + " seconds";


        if (damageRatio > 0) {
            builder.Append("Causes " + TextHelper.ColorizeText((damageRatio * 100).ToString() + "%", Color.green)
           + " of Weapon Damage every " + intervalText + " for "
           + durationText);

        }
        else {
            builder.Append(GetTooltip() + "for " + durationText);
        }

        if (Data.canOverload == true) {
            float overloadChance = ParentAbility != null ? ParentAbility.GetAbilityOverloadChance() : Source.Stats[StatName.OverloadChance];

            builder.AppendLine();
            builder.Append("Overload Chance: " + TextHelper.ColorizeText(TextHelper.FormatStat(StatName.OverloadChance, overloadChance), Color.green));
        }

        return builder.ToString();
    }

}
