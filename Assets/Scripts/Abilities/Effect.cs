using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static AbilityTrigger;
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

    protected TriggerInstance currentTriggerInstance;
    protected List<AbilityConstraint> targetConstraints = new List<AbilityConstraint>();
    protected EffectTargeter targeter;

    protected Projectile activeDelivery;

    public StatCollection Stats { get; protected set; }

    [System.NonSerialized]
    protected List<Effect> riderEffects = new List<Effect>();
    protected Effect parentEffect;

    protected bool isOverloading;

    public Effect(EffectData data, Entity source, Ability parentAbility = null) {
        this.Data = data;
        this.ParentAbility = parentAbility;
        this.Targeting = data.targeting;
        this.Source = source;
        SetupStats();
        SetupTargetConstraints();
        SetupRiderEffects();
        targeter = new EffectTargeter(this);
    }

    protected void SetupStats() {
        Stats = new StatCollection(this, Data.payloadStatData);
        //Stats.AddMissingStats(Data.stat)
        SimpleStat effectShotCount = new SimpleStat(StatName.ShotCount, Data.payloadCount);
        SimpleStat shotDelay = new SimpleStat(StatName.FireDelay, Data.shotDelay);
        SimpleStat maxTargets = new SimpleStat(StatName.EffectMaxTargets, Data.numberOfTargets);
        //SimpleStat effectRange = new SimpleStat(StatName.EffectRange, Data.)
        Stats.AddStat(effectShotCount);
        Stats.AddStat(shotDelay);
        Stats.AddStat(maxTargets);

        //if(Data.effectName == "Sword Guy Swipe Damage") {
        //    Debug.Log("Effect Range: " + Stats[StatName.EffectRange]);
        //}
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
            Effect rider = AbilityFactory.CreateEffect(Data.riderEffects[i].effectData, Source, ParentAbility);
            rider.parentEffect = this;
            riderEffects.Add(rider);
            //Debug.Log("Creating a rider: " + rider.Data.effectName + " for " + Data.effectName);
        }
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

        if(target.IsDead == true && Data.canAffectDeadTargets == false) {
            //Debug.Log(ParentAbility.Data.abilityName + " is trying to affect a dead target");
            return false;
        }

        if (EvaluateTargetConstraints(target) == false)
            return false;


        if (EntityTargets.Contains(target) == false) {
            EntityTargets.Add(target);
        }
        //else {
        //    Debug.LogError(target.EntityName + " was already in the list of targets for an effect: " + Data.effectName + " on the source: " + Source.EntityName);
        //}

        LastTarget = target;


        if(Data.canOverload == true) {
            if(CheckOverload(target) == true) {
                
                isOverloading = true;
                SendOverloadEvent(target);
            }
            else {
                isOverloading = false;
            }
        }

        return true;
    }

    private bool CheckOverload(Entity target) {
        float sourceChance = Source.Stats[StatName.OverloadChance];
        float targetChance = target == null ? 0f : target.Stats[StatName.OverloadRecieveChance];
        float skillChance = ParentAbility == null ? 0f : ParentAbility.Stats[StatName.OverloadChance];

        float totalChance = sourceChance + targetChance + skillChance;

        float roll = Random.Range(0f, 1f);

        //Debug.Log("Roll: " + roll + " Chance: " + totalChance);

        if( roll < totalChance) {
            return true;
        }

        return false;
    }

    public virtual void Remove(Entity target) {
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

        for (int i = 0; i < riderEffects.Count; i++) {
            riderEffects[i].RemoveFromAllTargets();
        }


    }

    public virtual void RegisterEvents() {
        RegisterRiderEvents();
    }

    public virtual void UnregisterEvents() {
        UnRegisterRiderEvents();
    }

    protected  void UnRegisterRiderEvents() {
        for (int i = 0; i < riderEffects.Count; i++) {
            EventManager.RemoveMyListeners(riderEffects[i]);
        }
    }

    protected void RegisterRiderEvents() {
        for (int i = 0; i < riderEffects.Count; i++) {
            
            riderEffects[i].RegisterRiderOnEventApplied();
        }
    }

    protected void RegisterRiderOnEventApplied() {
        if(parentEffect != null) {
            //Debug.Log("Registering a rider event");
            EventManager.RegisterListener(GameEvent.EffectApplied, OnEffectApplied);
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
    }

    public void SendEffectAppliedEvent() {
        EventData data = new EventData();
        data.AddEffect("Effect", this);
        data.AddAbility("Ability", ParentAbility);
        data.AddEntity("Source", Source);


        //Debug.Log(effectName + " has been applied from the card: " + ParentAbility.Source.cardName);

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

    }

    public virtual void Stack(Status status) {

    }

    public void TrackActiveDelivery(Projectile delivery) {
        activeDelivery = delivery;
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
                break;
            case MovementDestination.AwayFromSource:
                ApplyForceAwayFromSource(target);
                break;
            default:
                break;

        }

        return true;
    }

    private void ApplySourceForward(Entity target) {
        Vector2 force = target.transform.up.normalized * Data.moveForce;

        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();

        if (targetBody != null) {
            targetBody.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void ApplyForceAwayFromSource(Entity target) {
        Vector2 direction = target.transform.position - Source.transform.position;

        Vector2 resultingForce = direction.normalized * Data.moveForce;

        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();

        if (targetBody != null) {
            targetBody.AddForce(resultingForce, ForceMode2D.Impulse);
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

        if(adj == null) {
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

            Debug.Log("Creating child ability: " + newChild.Data.abilityName);
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

        for (int i = 0; i < activeAbilities.Count; i++) {
            builder.Append(activeAbilities[i].GetTooltip());

            if (i != activeAbilities.Count - 1)
                builder.AppendLine();
        }

        return builder.ToString();
    }
}

public class ForceStatusTickEffect : Effect {

    public override EffectType Type =>EffectType.ForceStatusTick;

    public ForceStatusTickEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    
    }


    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;

        AddStatusEffect addStatusEffect = target as AddStatusEffect;

        if(addStatusEffect == null) {
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

        SimpleStat durationStat = new SimpleStat(StatName.EffectLifetime, data.statusToAdd[0].duration);
        SimpleStat intervalStat = new SimpleStat(StatName.EffectInterval, data.statusToAdd[0].interval);

        float stackValue = data.statusToAdd[0].maxStacks > 0 ? data.statusToAdd[0].maxStacks : float.MaxValue;

        StatRange stacksStat = new StatRange(StatName.StackCount, 0, stackValue, data.statusToAdd[0].initialStackCount);

        Stats.AddStat(stacksStat);
        Stats.AddStat(durationStat);
        Stats.AddStat(intervalStat);


        for (int i = 0; i < data.statusToAdd.Count; i++) {
            Effect statusEffect = AbilityFactory.CreateEffect(data.statusToAdd[i].statusEffectDef.effectData, source, ParentAbility);
            activeStatusEffects.Add(statusEffect as StatAdjustmentEffect);
        }
    }

    public float GetModifiedEffectDuration() {
        float effectDurationModifier = 1 + Source.Stats[StatName.GlobalEffectDurationModifier];

        return Stats[StatName.EffectLifetime] * effectDurationModifier;
    }

    public float GetModifiedIntervalDuration() {
        float effectIntervalModifier = 1 + Source.Stats[StatName.GlobalEffectIntervalModifier];

        return Stats[StatName.EffectInterval] * effectIntervalModifier;
    }

    public void ForceTick() {

        for (int i = activeStatusDict.Count -1; i >=0 ; i--) {
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
                //StatusManager.RemoveStatus(target, statusList[i]);
                statusList[i].Remove();
            }

            activeStatusDict.Remove(target);
        }
        //else {
        //    Debug.LogError("[ADD STATUS EFFECT] A target: " + target.gameObject.name + " is not tracked.");
        //}
    }

    public override string GetTooltip() {
        //return base.GetTooltip();

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < activeStatusEffects.Count; i++) {
            //StatusData statusData = Data.statusToAdd[i];
            //EffectData effectData = Data.statusToAdd[i].statusEffectDef.effectData;

            switch (activeStatusEffects[i].Data.effectDesignation) {
                case StatModifierData.StatModDesignation.None:
                    break;
                case StatModifierData.StatModDesignation.PrimaryDamage:

                    builder.AppendLine();

                    string scalarTooltip = activeStatusEffects[i].ScalarTooltip();

                    //Debug.Log("Ability Level: " + scalarTooltip);

                    builder.AppendLine("Scales From: ");

                    builder.Append(scalarTooltip).AppendLine();



                    float damageRatio = activeStatusEffects[i].GetWeaponScaler();
                    //TextHelper.ColorizeText((damagePercent * 100).ToString() + "%", Color.green)

                    string durationText = TextHelper.ColorizeText(GetModifiedEffectDuration().ToString(), Color.yellow) + " seconds";
                    string intervalText = TextHelper.ColorizeText(GetModifiedIntervalDuration().ToString(), Color.yellow) + " seconds";


                    if (damageRatio > 0) {
                        builder.Append("Causes " + TextHelper.ColorizeText((damageRatio * 100).ToString() + "%", Color.green)
                       + " of Weapon Damage every " + intervalText + " for "
                       + durationText);

                    }
                    else {
                        builder.Append(activeStatusEffects[i].GetTooltip() + "for " + durationText);
                    }

                    if (activeStatusEffects[i].Data.canOverload == true) {
                        float overloadChance = ParentAbility.GetAbilityOverloadChance();

                        builder.AppendLine();
                        builder.Append("Overload Chance: " + TextHelper.ColorizeText(TextHelper.FormatStat(StatName.OverloadChance, overloadChance), Color.green));
                    }
   


                    if (Data.statusToAdd[0].maxStacks > 0) {
                        builder.AppendLine().AppendLine();
                        builder.Append("Stacks up to " + Stats.GetStatRangeMaxValue(StatName.StackCount) + " times").AppendLine();
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
                case DeliverySpawnLocation.Target:
                    token.transform.localPosition = target.transform.position;
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
    }

    public override void UnregisterEvents() {
        base.UnregisterEvents();
        EventManager.RemoveMyListeners(this);
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
            return false;
        }



        int unitsToSpawn = spawnCount; /*maxSpawns > 0 ? spawnCount - activeSpawns.Count : spawnCount;*/

        if(maxSpawns > 0 && spawnCount + activeSpawns.Count > maxSpawns) { 
            unitsToSpawn = spawnCount - activeSpawns.Count;
        }

        //Debug.Log((spawnCount - activeSpawns.Count).ToString() + " is how many I should spawn");

        //Debug.Log("Spawning: " + unitsToSpawn + " units. Max: " + maxSpawns + " Current: " + activeSpawns.Count + " SpawnCount: " + spawnCount);

        for (int i = 0; i < unitsToSpawn; i++) {
            Entity spawn = PerformSpawn(target);
            spawn.ownerType = Source.ownerType;
            spawn.entityType = Source.entityType;
            spawn.gameObject.layer = Source.gameObject.layer;
            spawn.subtypes.Add(Entity.EntitySubtype.Minion);
            VFXUtility.DesaturateSprite(spawn.innerSprite, 0.4f);

            EntityPlayer player = Source as EntityPlayer;
            if(player != null) {
                float averageDamage = player.GetAverageDamageRoll() * Data.percentOfPlayerDamage;
                float modifiedDamage = averageDamage * (1 + player.Stats[StatName.MinionDamageModifier]);
                spawn.Stats.SetStatValue(StatName.AbilityWeaponCoefficicent, modifiedDamage, Source);
            }

            NPC npc = spawn as NPC;
            if(npc != null) {
                npc.Brain.Sensor.RemoveFromDetectionMask(Source.gameObject.layer);
            }

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
            EntitySpawnType.Clone => GameObject.Instantiate(target, GetSpawnLocation(), Quaternion.identity),
            EntitySpawnType.Series => throw new NotImplementedException(),
            _ => null,
        };

        return result;

    }

    private Vector2 GetSpawnLocation() {
        Vector2 nearby = (Vector2)Source.transform.position + (Random.insideUnitCircle * Random.Range(2f, 6f));

        return nearby;
    }


    public override string GetTooltip() {
        
        StringBuilder builder = new StringBuilder();

        int maxSpawns = Stats[StatName.MaxMinionCount] > 0 ? (int)Stats[StatName.MaxMinionCount] : -1;

        if (Source.Stats[StatName.MaxMinionCount] > 0 && maxSpawns > 0) {
            maxSpawns += (int)Source.Stats[StatName.MaxMinionCount];
        }


        string replacement = Data.effectDescription.Replace("{}", TextHelper.ColorizeText( maxSpawns.ToString(), Color.green));

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


    public StatAdjustmentEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

        modData = new List<StatModifierData>();

        for (int i = 0; i < data.modData.Count; i++) {
            StatModifierData clonedModdata = new StatModifierData(data.modData[i]);
            modData.Add(clonedModdata);
        }

        for (int i = 0; i < modData.Count; i++) {
            modData[i].SetupEffectStats();
        }

    }

    public StatAdjustmentEffect(EffectData data, Entity source, StatAdjustmentEffect clone, Ability parentAbility = null) : base(data, source, parentAbility) {

        for (int i = 0; i < clone.modData.Count; i++) {

            StatModifierData clonedModData = new StatModifierData(clone.modData[i]);
            modData.Add(clonedModData);
            modData[i].CloneEffectStats(clone.modData[i]);
        }
    }

    //public static StatAdjustmentEffect Clone(StatAdjustmentEffect clone) {
    //    StatAdjustmentEffect effect = new StatAdjustmentEffect(clone.Data, clone.Source, clone.ParentAbility);

    //    effect.modData.Clear();

    //    for (int i = 0; i < clone.modData.Count; i++) {
    //        StatModifierData clonedData = new StatModifierData(clone.modData[i]);
    //        clonedData.CloneEffectStats(clone.modData[i]);
    //        effect.modData.Add(clonedData);
    //    }


    //    return effect;
    //}

    public override void Stack(Status status) {

        for (int i = 0; i < modData.Count; i++) {

            modData[i].Stats.RemoveAllModifiersFromSource(status);
            

            StatName targetStat = StatName.Vitality;
            if (modData[i].modValueSetMethod == StatModifierData.ModValueSetMethod.DerivedFromMultipleSources) {
                targetStat = StatName.AbilityWeaponCoefficicent;

                modData[i].RemoveAllscalerModsFromSource(targetStat, status);

                StatModifier stackMultiplier1 = new StatModifier(status.StackCount - 1, StatModType.PercentAdd, targetStat, status, modData[i].variantTarget);
                modData[i].AddScalerMod(targetStat, stackMultiplier1);
                
                //AddScalerMod(StatName.AbilityWeaponCoefficicent, stackMultiplier1);


                //StatScaler targetScaler = modData[i].GetScalerByStat(StatName.AbilityWeaponCoefficicent);
                //targetScaler.scalerStat.RemoveAllModifiersFromSource(status);

                //StatModifier stackMultiplier1 = new StatModifier(status.StackCount - 1, StatModType.PercentAdd, targetStat, status, modData[i].variantTarget);
                //targetScaler.scalerStat.AddModifier(stackMultiplier1);

                return;


            }

            if (modData[i].modValueSetMethod == StatModifierData.ModValueSetMethod.Manual) {
                targetStat = StatName.StatModifierValue;
            }


         


            StatModifier stackMultiplier = new StatModifier(status.StackCount - 1, StatModType.PercentAdd, targetStat, status, modData[i].variantTarget);
            modData[i].Stats.AddModifier(stackMultiplier.TargetStat, stackMultiplier);
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
            
            //modData[i].scalers.RemoveIfContains(scaler);
        }
    }

    //public void AddScaler(Effect target, StatScaler scaler) {
    //    if (trackedScalers.TryGetValue(target, out List<StatScaler> list) == true) {

    //        for (int i = 0; i < list.Count; i++) {
    //            if (list[i].targetStat == scaler.targetStat) {
    //                Debug.LogError("Duplicate stat scaler: " + scaler.targetStat + ". this is not supported");
    //                return;
    //            }
    //        }

    //        list.Add(scaler);
    //    }
    //    else {
    //        trackedScalers.Add(target, new List<StatScaler> { scaler });
    //    }
    //}

    //public void RemoveScaler(Effect target, StatScaler scaler) {
    //    if (trackedScalers.TryGetValue(target, out List<StatScaler> list) == true) {
    //        list.Remove(scaler);

    //        if (list.Count == 0)
    //            trackedScalers.Remove(target);
    //    }

    //}


    public void AddScalerMod(StatName targetStat, StatModifier mod) {
        for (int i = 0; i < modData.Count; i++) {
            modData[i].AddScalerMod(targetStat, mod);
            
            //for (int j = 0; j < modData[i].scalers.Count; j++) {
            //    if (modData[i].scalers[j].targetStat == targetStat) {
            //        modData[i].scalers[j].AddScalerMod(mod);
            //    }
            //}
        }

    }

    public void RemoveScalerMod(StatName targetStat, StatModifier mod) {
        for (int i = 0; i < modData.Count; i++) {
            modData[i].RemoveScalerMod(targetStat, mod);
            
            //for (int j = 0; j < modData[i].scalers.Count; j++) {
            //    if (modData[i].scalers[j].targetStat == targetStat) {
            //        modData[i].scalers[j].RemoveScalerMod(mod);
            //    }
            //}
        }
    }

    public float GetModifierValue(StatName name) {
        for (int i = 0; i < modData.Count; i++) {
            if (modData[i].targetStat == name)
                return modData[i].Stats[name];
        }

        return 0f;
    }

    //public float GetBaseWeaponPercent() {
    //    for (int i = 0; i < modData.Count; i++) {
    //        if (modData[i].modValueSetMethod == StatModifierData.ModValueSetMethod.DeriveFromWeaponDamage) {
    //            return modData[i].Stats[StatName.AbilityWeaponCoefficicent];
    //        }
    //    }
    //    return -1f;
    //}

    public float GetWeaponScaler() {

        float result = -1f;

        for (int i = 0; i < modData.Count; i++) {
            
            result = modData[i].GetWeaponScaler();
            
            if(result > 0) {
               return result;
            }


            //for (int j = 0; j < modData[i].scalers.Count; j++) {
            //    if (modData[i].scalers[j].deriveTarget == StatModifierData.DeriveFromWhom.WeaponDamage) {
            //        return modData[i].scalers[j].scalerStat.ModifiedValue;
            //    }
            //}
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

            //results.AddRange(modData[i].GetAllScalerValues());

            //for (int j = 0; j < modData[i].scalers.Count; j++) {
            //    results.Add(modData[i].scalers[j].targetStat, modData[i].scalers[j].scalerStat.ModifiedValue);
            //}
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

    private float GetProjectileStatContrabution(StatName stat, float scalerMultiplier) {
        
        if (activeDelivery != null) {
            float projectileStatValue = activeDelivery.Stats[stat] * scalerMultiplier;

            if(Stats.Contains(stat) == true) {
                projectileStatValue -= (Stats[stat] * scalerMultiplier); //Hack to prevent double dipping on projectile stats on both effect and projectile
            }

            return projectileStatValue;
        }

        return 0f;
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
                StatModifierData.DeriveFromWhom.WeaponDamage when Source is EntityPlayer => EntityManager.ActivePlayer.CurrentDamageRoll /** modData.Stats[StatName.AbilityWeaponCoefficicent]*/,
                StatModifierData.DeriveFromWhom.WeaponDamage when Source is NPC => Source.Stats[StatName.AbilityWeaponCoefficicent],
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

        if (activeMod.TargetStat == StatName.Health) {

            if (activeDelivery != null) {
                float projectileContrabution = 1f + activeDelivery.Stats[StatName.ProjectileEffectContrabution];
                targetValue *= projectileContrabution;
            }

            //Debug.Log("Target value: " + targetValue);
        }


       

        return modData.invertDerivedValue == false ? targetValue : -targetValue;
    }

    private void ApplyToEntity(Entity target, StatModifier activeMod) {

        float globalDamageMultiplier = GetDamageModifier(activeMod);
        float modValueResult = StatAdjustmentManager.ApplyStatAdjustment(target, activeMod, activeMod.TargetStat, activeMod.VariantTarget, ParentAbility, globalDamageMultiplier);

        //Debug.Log("applying a mod of: " + activeMod.TargetStat + " to " + target.EntityName);


        if (activeMod.TargetStat == StatName.Health) {
            //Debug.LogWarning("Damage dealt: " + modValueResult + " : " + Data.effectName);

            FloatingText text = FloatingTextManager.SpawnFloatingText(target.transform.position, modValueResult.ToString(), 0.75f, isOverloading);

            Gradient targetGrad = isOverloading == false ? Data.floatingTextColor : Data.overloadFloatingTextColor;
            text.SetColor(targetGrad);
        }
    }

    private void RemoveFromEntity(Entity target, StatModifier activeMod) {
        StatAdjustmentManager.RemoveStatAdjustment(target, activeMod, activeMod.VariantTarget, Source, ParentAbility);
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        for (int i = 0; i < modData.Count; i++) {

            StatModifier activeMod = PrepareStatMod(modData[i], target.Source, null, target);

            if (activeMod.VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackAbilityStatAdjustment(target, activeMod);
            }
            StatAdjustmentManager.AddAbilityModifier(target, activeMod);
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
                    StatAdjustmentManager.AddEffectModifier(target, activeMod);
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



    private float GetDamageModifier(StatModifier mod) {

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
                    AbilityTag.Fire => 0f,
                    AbilityTag.Poison => throw new System.NotImplementedException(),
                    AbilityTag.Healing => 0f,
                    AbilityTag.Melee => Source.Stats[StatName.MeleeDamageModifier],
                    _ => 0f,
                };

                globalDamageMultiplier += value;
            }
        }

        if(isOverloading == true) {
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


        if(scalers.Count == 0) {
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

        StringBuilder builder = new StringBuilder();

        string formated = TextHelper.FormatStat(modData[0].targetStat, modData[0].Stats[StatName.StatModifierValue]);

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

        builder.Append(replacement);
        return builder.ToString();
    }

}
