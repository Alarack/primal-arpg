using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


using TriggerInstance = AbilityTrigger.TriggerInstance;
using AbilityTriggerInstance = AbilityTrigger.AbilityTriggerInstance;

public class EffectTargeter {

    public TriggerInstance ActivationInstance { get; private set; }




    private Effect parentEffect;
    private List<Entity> entityTargets;
    private List<Effect> effectTargets;
    private List<Ability> abilityTargets;
    private int numberOfTargets = -1;
    //private List<Entity> previewTargets;

    public EffectTargeter(Effect parentEffect) {
        this.parentEffect = parentEffect;
        numberOfTargets = parentEffect.Data.numberOfTargets;
    }

    public void SetTriggerInstance(TriggerInstance activationInstance) {
        ActivationInstance = activationInstance;
    }

    #region GATHER TARGETS

    public List<Entity> GatherValidTargets() {
        List<Entity> results = new List<Entity>();

        foreach (List<Entity> entityList in EntityManager.ActiveEntities.Values) {
            for (int i = 0; i < entityList.Count; i++) {
                if (parentEffect.EvaluateTargetConstraints(entityList[i]) == true)
                    results.Add(entityList[i]);
            }
        }

        int targetsRequsted = numberOfTargets;
        if (numberOfTargets > results.Count)
            targetsRequsted = results.Count;

        if (numberOfTargets > 0 && results.Count > 0) {
            results.Shuffle();

            List<Entity> trunkatedResults = new List<Entity>();
            for (int i = 0; i < targetsRequsted; i++) {
                trunkatedResults.Add(results[i]);
            }

            return trunkatedResults;
        }

        return results;
    }

    public List<Ability> GatherValidAbilities() {
        List<Ability> results = new List<Ability>();
        List<Ability> allSourceAbilities = parentEffect.Source.AbilityManager.GetAllAbilities();

        foreach (Ability ability in allSourceAbilities) {

            if (parentEffect.EvaluateAbilityTargetConstraints(ability) == true)
                results.Add(ability);
        }

        int targetsRequsted = numberOfTargets;
        if (numberOfTargets > results.Count)
            targetsRequsted = results.Count;

        if (numberOfTargets > 0 && results.Count > 0) {
            results.Shuffle();

            List<Ability> trunkatedResults = new List<Ability>();
            for (int i = 0; i < targetsRequsted; i++) {
                trunkatedResults.Add(results[i]);
            }

            return trunkatedResults;
        }

        return results;
    }

    public List<Effect> GatherValidEffectTargets() {
        List<Effect> results = new List<Effect>();
        List<Effect> allSourceEffects = parentEffect.Source.AbilityManager.GetAllEffects();

        //Debug.Log("Found: " + allSourceEffects.Count + " effects");

        foreach (Effect effect in allSourceEffects) {

            //Debug.Log(effect.Data.effectName + " on " + effect.ParentAbility.Data.abilityName);

            if (effect == parentEffect) {
                //Debug.LogWarning(effect.Data.effectName + " is trying to affect itself");
                continue;

            }

            if (parentEffect.EvaluateEffectTargetConstraints(effect) == true) {
                //Debug.Log("Approved: " + effect.Data.effectName);
                results.Add(effect);
            }
        }

        int targetsRequsted = numberOfTargets;
        if (numberOfTargets > results.Count)
            targetsRequsted = results.Count;

        if (numberOfTargets > 0 && results.Count > 0) {
            results.Shuffle();

            List<Effect> trunkatedResults = new List<Effect>();
            for (int i = 0; i < targetsRequsted; i++) {
                trunkatedResults.Add(results[i]);
            }

            return trunkatedResults;
        }

        return results;
    }

    public Entity GetLastTargetFromOtherEffect(string abilityName, string effectName, AbilityCategory category) {

        Tuple<Ability, Effect> target = AbilityUtilities.GetAbilityAndEffectByName(abilityName, effectName, parentEffect.Source, category);

        if (target.Item1 == null) {
            Debug.LogWarning("Could not find: " + abilityName + " on " + parentEffect.ParentAbility.Source.EntityName);
            return null;
        }

        if (target.Item2 == null) {
            Debug.LogWarning("Could not find: " + effectName + " on " + parentEffect.ParentAbility.Data.abilityName);
            return null;
        }

        Effect targetEffect = target.Item2;

        if (targetEffect.EntityTargets.Count > 0)
            return targetEffect.LastTarget;
        else {
            Debug.LogWarning("0 Targets Found on : " + effectName + " on " + parentEffect.ParentAbility.Data.abilityName);
            return null;
        }


    }

    public Entity GetSourceTarget() {
        Entity sourceEntity = parentEffect.Source;

        if (parentEffect.EvaluateTargetConstraints(sourceEntity) == false) {
            Debug.LogWarning(parentEffect.Source.EntityName + " is an invalid target for " + parentEffect.ParentAbility.Data.abilityName);
            return null;
        }

        return sourceEntity;
    }

    public Entity GetTriggerTarget() {
        Entity triggeringEntity = ActivationInstance.TriggeringEntity;

        if (parentEffect.EvaluateTargetConstraints(triggeringEntity) == false) {
            Debug.LogWarning(triggeringEntity.EntityName + " is an invalid target for " + parentEffect.Data.effectName + " on: " + parentEffect.Source.EntityName);
            return null;
        }

        return triggeringEntity;
    }

    public Ability GetTriggeringAbility() {
        AbilityTriggerInstance instance = ActivationInstance as AbilityTriggerInstance;

        if(instance == null) {
            Debug.LogError("An effect: " + parentEffect.Data.effectName + " is trying to getting a triggering ability, but it's not an ability trigger instance");
            return null;
        }

        return instance.triggeringAbility;
    }

    public Entity GetCauseTarget() {
        Entity causeingEntity = ActivationInstance.CauseOfTrigger;

        if (parentEffect.EvaluateTargetConstraints(causeingEntity) == false) {
            Debug.LogWarning(causeingEntity.EntityName + " is an invalid target for " + parentEffect.Data.effectName + " on: " + parentEffect.Source.EntityName);
            return null;
        }

        return causeingEntity;
    }

    public List<Entity> GetLogicTargets() {
        List<Entity> validTargets = GatherValidTargets();
        List<Entity> results = new List<Entity>();

        if (validTargets.Count == 0) {
            Debug.LogWarning("An ability: "
                + parentEffect.ParentAbility.Data.abilityName
                + ":: on the entity: "
                + parentEffect.Source.EntityName
                + " triggered an effect and had 0 valid targets");
        }

        for (int i = 0; i < validTargets.Count; i++) {

            Entity currentTarget = validTargets[i];

            results.Add(currentTarget);
        }

        return results;
    }

    public List<Ability> GetLogicAbilityTargets() {
        List<Ability> validEffectTargets = GatherValidAbilities();
        List<Ability> results = new List<Ability>();

        if (validEffectTargets.Count == 0) {
            Debug.LogWarning("An ability: "
                + parentEffect.ParentAbility.Data.abilityName
                + ":: on the entity: "
                + parentEffect.Source.EntityName
                + " triggered an effect and had 0 valid targets");
        }

        for (int i = 0; i < validEffectTargets.Count; i++) {
            Ability currentTarget = validEffectTargets[i];
            results.Add(currentTarget);
        }

        return results;
    }

    public List<Effect> GetLogicEffectTargets() {
        List<Effect> validEffectTargets = GatherValidEffectTargets();
        List<Effect> results = new List<Effect>();

        if (validEffectTargets.Count == 0) {
            Debug.LogWarning("An ability: "
                + parentEffect.ParentAbility.Data.abilityName
                + ":: on the entity: "
                + parentEffect.Source.EntityName
                + " triggered an effect and had 0 valid targets");
        }

        for (int i = 0; i < validEffectTargets.Count; i++) {
            Effect currentTarget = validEffectTargets[i];
            results.Add(currentTarget);
        }

        return results;
    }


    //public List<Entity> GetTargetsForAI() {
    //    List<Entity> results;

    //    results = parentEffect.Targeting switch {
    //        EffectTarget.None => new List<Entity> { GetSourceTarget() },
    //        EffectTarget.Source => new List<Entity> { GetSourceTarget() },
    //        EffectTarget.Trigger => new List<Entity> { GetTriggerTarget() },
    //        EffectTarget.Cause => new List<Entity> { GetCauseTarget() },
    //        //EffectTarget.UserSelected => GetLogicTargets(),
    //        EffectTarget.LogicSelected => GetLogicTargets(),
    //        EffectTarget.OtherEffectTarget => throw new NotImplementedException(),
    //        EffectTarget.OtherMostRecentTarget => throw new NotImplementedException(),
    //        _ => new List<Entity>(),
    //    };

    //    previewTargets = results;
    //    return results;
    //}

    #endregion




    private void ApplyToSource() {

        Entity sourceEntity = GetSourceTarget();

        parentEffect.Apply(sourceEntity);
        parentEffect.SendEffectAppliedEvent();
    }

    private void ApplyToTrigger() {

        if(parentEffect.Data.subTarget == EffectSubTarget.Ability) {
            ApplyToTriggerAbility();
            return;
        }


        Entity triggeringEntity = GetTriggerTarget();

        parentEffect.Apply(triggeringEntity);
        parentEffect.SendEffectAppliedEvent();
    }

    private void ApplyToTriggerAbility() {
        Ability triggeringAbility = GetTriggeringAbility();

        parentEffect.ApplyToAbility(triggeringAbility);
        parentEffect.SendEffectAppliedEvent();
    }

    private void ApplyToCause() {

        Entity causeingEntity = GetCauseTarget();

        parentEffect.Apply(causeingEntity);
        parentEffect.SendEffectAppliedEvent();
    }

    private void ApplyLogicTargeting() {
        entityTargets = GetLogicTargets();

        for (int i = 0; i < entityTargets.Count; i++) {
            parentEffect.Apply(entityTargets[i]);
        }

        if (entityTargets.Count > 0)
            parentEffect.SendEffectAppliedEvent();

    }

    private void ApplyToSpecificAbility() {

    }

    private void ApplyToLogicTargetedAbilities() {
        abilityTargets = GetLogicAbilityTargets();

        for (int i = 0; i < abilityTargets.Count; i++) {
            parentEffect.ApplyToAbility(abilityTargets[i]);
        }
    }

    private void ApplyToSpecificEffect() {

    }

    private void ApplyToLogicTargetedEffects() {
        effectTargets = GetLogicEffectTargets();

        for (int i = 0; i < effectTargets.Count; i++) {
            parentEffect.ApplyToEffect(effectTargets[i]);
        }
    }


    //public void ApplyToPreviewedTargets() {
    //    for (int i = 0; i < previewTargets.Count; i++) {
    //        parentEffect.Apply(previewTargets[i]);
    //    }

    //    if (previewTargets.Count > 0)
    //        parentEffect.SendEffectAppliedEvent();
    //}

    public void ApplyPayloadDelivery() {
        Vector2 targetocation = parentEffect.Data.spawnLocation switch {
            DeliverySpawnLocation.Source => parentEffect.Source.transform.position,
            DeliverySpawnLocation.Trigger => ActivationInstance.TriggeringEntity.transform.position,
            DeliverySpawnLocation.Cause => ActivationInstance.CauseOfTrigger.transform.position,
            DeliverySpawnLocation.MousePointer => Camera.main.ScreenToWorldPoint(Input.mousePosition),
            DeliverySpawnLocation.Target => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

        new Task(DeliveryPayloadOnDelay(targetocation));
    }

    private IEnumerator DeliveryPayloadOnDelay(Vector2 location) {
        WaitForSeconds waiter = new WaitForSeconds(parentEffect.Data.shotDelay);

        //Debug.Log(parentEffect.Stats[StatName.ShotCount] + " projectiles on " + parentEffect.ParentAbility.Data.abilityName);

        for (int i = 0; i < parentEffect.Stats[StatName.ShotCount]; i++) {

            //Instantiate payload;
            Entity delivery = GameObject.Instantiate(parentEffect.Data.payloadPrefab, location, parentEffect.Source.transform.rotation);

            Projectile projectile = delivery as Projectile;
            if (projectile != null) {
                projectile.Setup(parentEffect.Source, parentEffect);

                if (parentEffect.Stats.Contains(StatName.ProjectilePierceCount) == true)
                    projectile.Stats.AddModifier(StatName.ProjectilePierceCount, parentEffect.Stats[StatName.ProjectilePierceCount], StatModType.Flat, parentEffect.Source);

                float sourceInaccuracy = (1f - parentEffect.Source.Stats[StatName.Accuracy]) * 360f;
                float projectileInaccuracy = (1f - projectile.Stats[StatName.Accuracy]) * 360f;
                float totalInaccuracy = (projectileInaccuracy + sourceInaccuracy) / 2f;

                projectile.transform.eulerAngles += new Vector3(0f, 0f, UnityEngine.Random.Range(-totalInaccuracy, totalInaccuracy));


            }

            EffectZone effectZone = delivery as EffectZone;
            if (effectZone != null) {
                effectZone.Setup(parentEffect, parentEffect.Data.effectZoneInfo);
            }


            yield return waiter;
        }

    }


    //private void ActivateUserTargeting() {
    //    targets = GatherValidTargets();

    //    if (targets.Count == 0) {
    //        Debug.LogWarning("[EFFECT TARGETER] a user-targeted effect has no valid targets: " + parentEffect.ParentAbility.abilityName);
    //        return;
    //    }

    //    TargetingManager.ActivateEntityTargeting(parentEffect);
    //}

    public void Apply() {

        Action applyAction = parentEffect.Targeting switch {
            EffectTarget.None => ApplyToSource,
            EffectTarget.Source => ApplyToSource,
            EffectTarget.Trigger => ApplyToTrigger,
            EffectTarget.Cause => ApplyToCause,
            //EffectTarget.UserSelected when parentEffect.Source.Owner == EntityData.Owner.Friendly => ActivateUserTargeting,
            //EffectTarget.UserSelected when parentEffect.Source.Owner == EntityData.Owner.Enemy => ApplyLogicTargeting,
            EffectTarget.LogicSelected => ApplyLogicTargeting,
            EffectTarget.OtherEffectTarget => throw new NotImplementedException(),
            EffectTarget.OtherMostRecentTarget => throw new NotImplementedException(),
            EffectTarget.PayloadDelivered => ApplyPayloadDelivery,
            EffectTarget.LogicSelectedEffect => ApplyToLogicTargetedEffects,
            EffectTarget.LogicSelectedAbility => ApplyToLogicTargetedAbilities,
            _ => null
        };

        applyAction?.Invoke();
    }


}
