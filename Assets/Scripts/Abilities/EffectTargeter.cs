using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

using Random = UnityEngine.Random;


using TriggerInstance = AbilityTrigger.TriggerInstance;
using static UnityEngine.GraphicsBuffer;
//using AbilityTriggerInstance = AbilityTrigger.AbilityTriggerInstance;

public class EffectTargeter {

    public TriggerInstance ActivationInstance { get; private set; }




    private Effect parentEffect;
    private List<Entity> entityTargets;
    private List<Effect> effectTargets;
    private List<Ability> abilityTargets;
    private int numberOfTargets = -1;

    public EffectTargeter(Effect parentEffect) {
        this.parentEffect = parentEffect;
        numberOfTargets = (int)parentEffect.Stats[StatName.EffectMaxTargets];
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
               // Debug.Log("Approved: " + effect.Data.effectName + ". parent: " + effect.ParentAbility.Data.abilityName);
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

    public List<Entity> GetOtherEffectEntityTargets(string abilityName, string effectName, AbilityCategory category) {
        Tuple<Ability, Effect> target = AbilityUtilities.GetAbilityAndEffectByName(abilityName, effectName, parentEffect.Source, category);

        
        if(target.Item2.EntityTargets.Count > 0) {
            return target.Item2.EntityTargets;
        }

        return null;
    }

    public Entity GetSourceTarget() {
        Entity sourceEntity = parentEffect.Source;

        if (parentEffect.EvaluateTargetConstraints(sourceEntity) == false) {
            if(parentEffect.Source != null)
                Debug.LogWarning(parentEffect.Source.EntityName + " is an invalid target for " + parentEffect.ParentAbility.Data.abilityName);
            else {
                Debug.LogWarning("The source of: " + parentEffect.Data.effectName + " is null, and failed a constraint because it didn't exist");
            }
            
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
        //AbilityTriggerInstance instance = ActivationInstance as AbilityTriggerInstance;

        //if(instance == null) {
        //    Debug.LogError("An effect: " + parentEffect.Data.effectName + " is trying to getting a triggering ability, but it's not an ability trigger instance");
        //    return null;
        //}

        //return instance.triggeringAbility;

        return ActivationInstance.TriggeringAbility;
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
            Debug.Log("An ability: "
                + parentEffect.ParentAbility.Data.abilityName
                + ":: on the entity: "
                + parentEffect.Source.EntityName
                + " triggered an effect: " + parentEffect.Data.effectName
                + " and had 0 valid targets");
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
            Debug.LogWarning(
                "An Effect: " 
                + parentEffect.Data.effectName
                + " on the Ability: "
                + parentEffect.ParentAbility.Data.abilityName
                + " :: on the entity: "
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


        if(parentEffect.Data.deliveryPayloadToTarget == false) {
            for (int i = 0; i < entityTargets.Count; i++) {
                parentEffect.Apply(entityTargets[i]);
            }
        }
        else {
            for (int i = 0; i < entityTargets.Count; i++) {
                ApplyPayloadDeliveryToTarget(entityTargets[i]);
            }
        }

       

        if (entityTargets.Count > 0)
            parentEffect.SendEffectAppliedEvent();

    }

    private void ApplyToLogicTargetedAbilities() {
        abilityTargets = GetLogicAbilityTargets();

        for (int i = 0; i < abilityTargets.Count; i++) {
            parentEffect.ApplyToAbility(abilityTargets[i]);
        }

        if (abilityTargets.Count > 0)
            parentEffect.SendEffectAppliedEvent();
    }

    private void ApplyToLogicTargetedEffects() {
        effectTargets = GetLogicEffectTargets();

        for (int i = 0; i < effectTargets.Count; i++) {
            parentEffect.ApplyToEffect(effectTargets[i]);
        }

        if (effectTargets.Count > 0)
            parentEffect.SendEffectAppliedEvent();
    }

    private void ApplyToRecentTarget() {
        Entity target = GetLastTargetFromOtherEffect(parentEffect.Data.otherAbilityName, parentEffect.Data.otherEffectName, AbilityCategory.Any);

        if (target != null) {
            parentEffect.Apply(target);
            parentEffect.SendEffectAppliedEvent();
        }
    }

    public void ApplyToOtherEntityTargets(List<Entity> targets) {

        for (int i = 0; i < targets.Count; i++) {
            parentEffect.Apply(targets[i]);
        }

        if(targets.Count > 0)
            parentEffect.SendEffectAppliedEvent();
    }


    public void ApplyPayloadDeliveryToTarget(Entity target) {
        new Task(DeliveryPayloadOnDelay(/*GetPayloadSpawnLocation(),*/ target));
    }

    public void ApplyPayloadDelivery() {
        //Vector2 targetLocation = parentEffect.Data.spawnLocation switch {
        //    DeliverySpawnLocation.Source => parentEffect.Source.transform.position,
        //    DeliverySpawnLocation.Trigger => ActivationInstance.TriggeringEntity.transform.position,
        //    DeliverySpawnLocation.Cause => ActivationInstance.CauseOfTrigger.transform.position,
        //    DeliverySpawnLocation.MousePointer => Camera.main.ScreenToWorldPoint(Input.mousePosition),
        //    DeliverySpawnLocation.Target => throw new NotImplementedException(),
        //    _ => throw new NotImplementedException(),
        //};

        new Task(DeliveryPayloadOnDelay(/*GetPayloadSpawnLocation())*/));
    }

    public Vector2 GetPayloadSpawnLocation() {

        if (parentEffect.Source == null) {
            //Debug.LogWarning("The source of: " + parentEffect.Data.effectName + " has been destroyed while casting");
            return Vector2.zero;
        }

        Vector2 targetLocation = parentEffect.Data.spawnLocation switch {
            DeliverySpawnLocation.Source => parentEffect.Source.GetOriginPoint().position,
            DeliverySpawnLocation.Trigger => ActivationInstance.TriggeringEntity.transform.position,
            DeliverySpawnLocation.Cause => ActivationInstance.CauseOfTrigger.transform.position,
            DeliverySpawnLocation.MousePointer => GetMouseLocationWithInaccuracy(),
            DeliverySpawnLocation.AITarget => GetAITargetPosition(),
            DeliverySpawnLocation.RandomViewportPosition => GetRandomViewportPosition(),
            DeliverySpawnLocation.AbilityLastPayloadLocation => GetLastAbilityPayloadLocation(),
            DeliverySpawnLocation.LastEffectZoneLocation => ActivationInstance.SavedLocation,
            _ => throw new NotImplementedException(),
        };

        return targetLocation;
    }

    private Vector2 GetMouseLocationWithInaccuracy() {
        Vector2 basePointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        float maxOffset = (1f - parentEffect.Source.Stats[StatName.Accuracy]) * 25f;

        //Debug.Log("Owner Accuracy:  " + parentEffect.Source.Stats[StatName.Accuracy] + " Max offset: " + maxOffset);
        
        Vector2 modifiedPosiiton = basePointerPos + Random.insideUnitCircle* Random.Range(0, maxOffset);


        return modifiedPosiiton;
    }

    public List<Vector2> GetPayloadSpawnlocationSequence(int count) {
        
        if (count < -2) {
            Debug.LogError("Two few points?");
        }
        
        List<Vector2> results = TargetHelper.GetWorldSpacePointSequence(parentEffect.Data.spawnLocationStart, parentEffect.Data.spawnLocationEnd, count, false);


        return results;
    }

    public Vector2 GetLastAbilityPayloadLocation() {
        Ability targetAbility = parentEffect.Source.GetAbilityByName(parentEffect.Data.targetAbilityForLastPayload, AbilityCategory.Any);

        return targetAbility.LastPayloadLocation;

    }



    public Vector2 GetRandomViewportPosition() {

        float randomX = Random.Range(parentEffect.Data.minViewportValues.x, parentEffect.Data.maxViewportValues.x);
        float randomY = Random.Range(parentEffect.Data.minViewportValues.y, parentEffect.Data.maxViewportValues.y);

        //Debug.Log(randomX + ", " + randomY);

        Vector2 convertedPoint = Camera.main.ViewportToWorldPoint(new Vector2(randomX, randomY));



        return convertedPoint;

    }

    private Vector2 GetAITargetPosition() {
        NPC npc = parentEffect.Source as NPC;
        if(npc == null) {
            Debug.LogError("Can't get an ai target from a non NPC " + parentEffect.Data.effectName);
            return Vector2.zero;
        }

        if(npc.Brain.Sensor.LatestTarget == null) { 
            return Vector2.zero;
        }

        return npc.Brain.Sensor.LatestTarget.transform.position;
    }

    private IEnumerator DeliveryPayloadOnDelay(Entity target = null) {
        WaitForSeconds waiter = new WaitForSeconds(parentEffect.Stats[StatName.FireDelay]);

        if(parentEffect.Source == null) {
            //Debug.LogWarning("The source of: " + parentEffect.Data.effectName + " has been destroyed while casting");
            yield break;
        }

        //Debug.Log(parentEffect.Stats[StatName.ShotCount] + " projectiles on " + parentEffect.ParentAbility.Data.abilityName);

        int baseShotCount = (int)parentEffect.Stats[StatName.ShotCount];

        if(baseShotCount == 0) {
            Debug.LogError("Shot Count of 0 on " + parentEffect.Data.effectName + " add a Shot Count Stat to it or its parent ability");
        }

        int totalShots = baseShotCount;



        if (parentEffect.ParentAbility.Tags.Contains(AbilityTag.Projectile)) {
            int ownerShotCount = (int)parentEffect.Source.Stats[StatName.ShotCount];
            //int abilityShotCount = (int)parentEffect.ParentAbility.Stats[StatName.ShotCount];

            totalShots += ownerShotCount;

            //totalShots += abilityShotCount;
        }



        if (parentEffect.Data.spawnLocation == DeliverySpawnLocation.WorldPositionSequence) {

            List<Vector2> deliveryPoints = GetPayloadSpawnlocationSequence(totalShots);

            //Debug.Log("Creating a world position sequence for: " + parentEffect.Data.effectName + ". " + totalShots + " shots");

            for (int i = 0; i < deliveryPoints.Count; i++) {
                CreateDeliveryPayload(target, deliveryPoints[i]);
                yield return waiter;
            }


            yield break;
        }



        for (int i = 0; i < totalShots; i++) {

            Vector2 payloadLocation = GetPayloadSpawnLocation();

            CreateDeliveryPayload(target, payloadLocation);

            yield return waiter;
        }

    }

    private void CreateDeliveryPayload(Entity target, Vector2 payloadLocation) {
        if(parentEffect.Source == null) {
            Debug.LogWarning("null source for: " + parentEffect.Data.effectName + " when creating delivery payload");
            return;
        }
        
        Entity delivery = GameObject.Instantiate(parentEffect.PayloadPrefab, payloadLocation, parentEffect.Source.FacingRotation);

        Projectile projectile = delivery as Projectile;
        if (projectile != null) {
            //projectile.Stats.SetParentCollection(parentEffect.Stats); removed because it messes up pierce / chain / split counts
            parentEffect.TrackActiveDelivery(projectile);
            projectile.Stats.AddMissingStats(parentEffect.Stats);
            projectile.Stats.AddMissingStats(parentEffect.ParentAbility.Stats);

            projectile.Setup(parentEffect.Source, parentEffect, parentEffect.Data.projectileHitMask, parentEffect.Data.maskTargeting);

            if (parentEffect.Data.spawnLocation == DeliverySpawnLocation.Trigger) {
                Entity triggeringEntity = ActivationInstance.TriggeringEntity;
                Entity causingEntity = ActivationInstance.CauseOfTrigger;
                if (triggeringEntity != null) {
                    projectile.IgnoreCollision(triggeringEntity);
                }

                if(causingEntity != null) {
                    projectile.IgnoreCollision(causingEntity);
                }
            }


            float sourceInaccuracy = (1f - parentEffect.Source.Stats[StatName.Accuracy]) * 360f;
            float projectileInaccuracy = (1f - projectile.Stats[StatName.Accuracy]) * 360f;
            float totalInaccuracy = (projectileInaccuracy + sourceInaccuracy) / 2f;


            if (target != null) {
                projectile.transform.rotation = TargetUtilities.GetRotationTowardTarget(target.transform, projectile.transform);
            }

            projectile.transform.eulerAngles += new Vector3(0f, 0f, UnityEngine.Random.Range(-totalInaccuracy, totalInaccuracy));

            //Debug.LogWarning("Creating: " + projectile.EntityName);
        }

        EffectZone effectZone = delivery as EffectZone;
        if (effectZone != null) {
            effectZone.Stats.AddMissingStats(parentEffect.Stats, null, parentEffect.Data.effectName, effectZone.EntityName);
            effectZone.Stats.AddMissingStats(parentEffect.ParentAbility.Stats);

            Transform parentTransform = null;
            if (parentEffect.ZoneInfo.parentToTarget == true)
                parentTransform = ActivationInstance.TriggeringEntity.transform;

            
            effectZone.Setup(parentEffect, parentEffect.ZoneInfo, parentTransform, null, parentEffect.Source.gameObject.layer, parentEffect.Data.maskTargeting);

            if(effectZone.Stats.Contains(StatName.Accuracy) == true) {
                float zoneInnaccuracy = (1f - effectZone.Stats[StatName.Accuracy]) * 360f;

                effectZone.transform.eulerAngles += new Vector3(0f, 0f, Random.Range(-zoneInnaccuracy, zoneInnaccuracy));
            }


            //Debug.LogWarning("Creating an effect zone payload delivery for: " + parentEffect.Data.effectName);
            //Debug.LogWarning("Location: " + parentEffect.Source.transform.position);
            //Debug.Log(effectZone.gameObject.name);
            parentEffect.ActiveEffectZones.Add(effectZone);

            parentEffect.ParentAbility.LastPayloadLocation = effectZone.transform.position;
        }

        delivery.SpawningAbility = parentEffect.ParentAbility;
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

        //if(parentEffect.Source.entityType == Entity.EntityType.Enemy) {
        //    Debug.Log("An effect: " + parentEffect.Data.effectName + " is being applied");
        //}
        

        Action applyAction = parentEffect.Targeting switch {
            EffectTarget.None => ApplyToSource,
            EffectTarget.Source => ApplyToSource,
            EffectTarget.Trigger => ApplyToTrigger,
            EffectTarget.Cause => ApplyToCause,
            //EffectTarget.UserSelected when parentEffect.Source.Owner == EntityData.Owner.Friendly => ActivateUserTargeting,
            //EffectTarget.UserSelected when parentEffect.Source.Owner == EntityData.Owner.Enemy => ApplyLogicTargeting,
            EffectTarget.LogicSelected => ApplyLogicTargeting,
            EffectTarget.OtherEffectTarget => throw new NotImplementedException(),
            EffectTarget.OtherMostRecentTarget => ApplyToRecentTarget,
            EffectTarget.PayloadDelivered => ApplyPayloadDelivery,
            EffectTarget.LogicSelectedEffect => ApplyToLogicTargetedEffects,
            EffectTarget.LogicSelectedAbility => ApplyToLogicTargetedAbilities,
            _ => null
        };

        applyAction?.Invoke();
    }


}
