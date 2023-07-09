using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static AbilityTrigger;
using static UnityEngine.GraphicsBuffer;
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

    public StatCollection Stats { get; protected set; }

    public Effect(EffectData data, Entity source, Ability parentAbility = null) {
        this.Data = data;
        this.ParentAbility = parentAbility;
        this.Targeting = data.targeting;
        this.Source = source;
        SetupStats();
        SetupTargetConstraints();
        targeter = new EffectTargeter(this);
    }

    protected void SetupStats() {
        Stats = new StatCollection(this);
        SimpleStat effectShotCount = new SimpleStat(StatName.ShotCount, Data.payloadCount);
        SimpleStat pierceCount = new SimpleStat(StatName.ProjectilePierceCount, Data.projectilePierceCount);
        Stats.AddStat(effectShotCount);
        Stats.AddStat(pierceCount);
    }

    protected void SetupTargetConstraints() {
        for (int i = 0; i < Data.targetConstraints.Count; i++) {
            AbilityConstraint constraint = AbilityFactory.CreateAbilityConstraint(Data.targetConstraints[i], Source, ParentAbility);
            targetConstraints.Add(constraint);
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
            if (targetConstraints[i].Evaluate(target) == false) {
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
            if (targetConstraints[i].Evaluate(target) == false) {
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


        if (EvaluateTargetConstraints(target) == false)
            return false;


        if (EntityTargets.Contains(target) == false) {
            EntityTargets.Add(target);
        }
        //else {
        //    Debug.LogError(target.EntityName + " was already in the list of targets for an effect: " + Data.effectName + " on the source: " + Source.EntityName);
        //}

        LastTarget = target;

        return true;
    }

    public virtual void Remove(Entity target) {
        EntityTargets.RemoveIfContains(target);
    }

    public virtual bool ApplyToEffect(Effect target) {

        if (EvaluateEffectTargetConstraints(target) == false)
            return false;

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



    protected void BeginDelivery() {

        Weapon ownerWeapon = Source.GetComponent<Weapon>();
        //ownerWeapon.payload = Data.payloadPrefab;

        switch (Data.spawnLocation) {
            case DeliverySpawnLocation.Source:

                break;
            case DeliverySpawnLocation.Trigger:

                break;
            case DeliverySpawnLocation.Cause:

                break;
            case DeliverySpawnLocation.MousePointer:

                break;
        }

    }

    public void RemoveFromAllTargets() {
        for (int i = 0; i < EntityTargets.Count; i++) {
            Remove(EntityTargets[i]);
        }

        for (int i = 0; i < AbilityTargets.Count; i++) {
            RemoveFromAbility(AbilityTargets[i]);
        }

        for (int i = 0; i < EffectTargets.Count; i++) {
            RemoveFromEffect(EffectTargets[i]);
        }

        EntityTargets.Clear();
        AbilityTargets.Clear();
        EffectTargets.Clear();
    }

    public void SendEffectAppliedEvent() {
        EventData data = new EventData();
        data.AddEffect("Effect", this);
        data.AddAbility("Ability", ParentAbility);
        data.AddEntity("Source", Source);


        //Debug.Log(effectName + " has been applied from the card: " + ParentAbility.Source.cardName);

        EventManager.SendEvent(GameEvent.EffectApplied, data);
    }

    public void CreateVFX(Entity currentTarget) {

    }

    public virtual string GetTooltip() {


        return Data.effectDescription;
    }


}

public class ForcedMovementEffect : Effect {

    public override EffectType Type => EffectType.Movement;


    public ForcedMovementEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;


        Vector2 direction = target.transform.position - Source.transform.position;

        Vector2 resultingForce = direction.normalized * Data.moveForce;

        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();

        if (target != null) {
            targetBody.AddForce(resultingForce, ForceMode2D.Impulse);
        }


        return true;
    }


}

public class AddStatusEffect : Effect {

    public override EffectType Type => EffectType.AddStatus;

    private Dictionary<Entity, List<Status>> activeStatusDict = new Dictionary<Entity, List<Status>>();

    public AddStatusEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        for (int i = 0; i < Data.statusToAdd.Count; i++) {
            StatusStatAdjustment newStatus = new StatusStatAdjustment(Data.statusToAdd[i], target, Source);
            StatusManager.AddStatus(target, newStatus);
            TrackActiveStatus(target, newStatus);
        }

        return true;
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

    public override void Remove(Entity target) {
        base.Remove(target);

        if (activeStatusDict.TryGetValue(target, out List<Status> statusList)) {
            for (int i = 0; i < statusList.Count; i++) {
                StatusManager.RemoveStatus(target, statusList[i]);
            }

            activeStatusDict.Remove(target);
        }
        else {
            Debug.LogError("[ADD STATUS EFFECT] A target: " + target.gameObject.name + " is not tracked.");
        }
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

public class StatAdjustmentEffect : Effect {

    public override EffectType Type => EffectType.StatAdjustment;

    private Dictionary<Entity, List<StatModifier>> trackedEntityMods = new Dictionary<Entity, List<StatModifier>>();
    private Dictionary<Ability, List<StatModifier>> trackedAbilityMods = new Dictionary<Ability, List<StatModifier>>();
    private Dictionary<Effect, List<StatModifier>> trackedEffectMods = new Dictionary<Effect, List<StatModifier>>();

    private List<StatModifierData> modData = new List<StatModifierData>();

    public StatAdjustmentEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        modData = new List<StatModifierData>(data.modData);

        for (int i = 0; i < modData.Count; i++) {
            modData[i].SetupEffectStats();
        }

    }

    public void AddStatAdjustmentModifier(StatModifier mod) {
        //List<StatModifierData> targetData = GetModDataByDesignation(designation);

        for (int i = 0; i < modData.Count; i++) {
            //modData[i].Stats.AddModifier(mod.TargetStat, mod);

            StatAdjustmentManager.ApplyDataModifier(modData[i], mod);
            //Debug.LogWarning("Applying: " + mod.TargetStat + " Modifier: " + mod.Value + " to " + Data.effectName);
        }
    }

    public void RemoveStatAdjustmentModifier(StatModifier mod) {
        //List<StatModifierData> targetData = GetModDataByDesignation(designation);

        for (int i = 0; i < modData.Count; i++) {
            StatAdjustmentManager.RemoveDataModifiyer(modData[i], mod);
            //modData[i].Stats.RemoveModifier(mod.TargetStat, mod);
        }
    }

    //private List<StatModifierData> GetModDataByDesignation(StatModifierData.StatModDesignation designation) {
    //    List<StatModifierData> results = new List<StatModifierData>();

    //    for (int i = 0; i < modData.Count; i++) {
    //        if (modData[i].modDesignation == designation)
    //            results.Add(modData[i]);
    //    }

    //    return results;

    //}

    public float GetModifierValue(StatName name) {
        //List<StatModifierData> targetData = GetModDataByDesignation(designation);

        for (int i = 0; i < modData.Count; i++) {
            if (modData[i].targetStat == name)
                return modData[i].Stats[name];
        }

        return 0f;
    }

    public float GetBaseWeaponPercent() {
        for (int i = 0; i < modData.Count; i++) {
            if (modData[i].modValueSetMethod == StatModifierData.ModValueSetMethod.DeriveFromWeaponDamage) {
                return modData[i].Stats[StatName.AbilityWeaponCoefficicent];
            }
        }
        return -1f;
    }

    private float SetModValues(Entity target, StatModifier activeMod, StatModifierData modData) {

        float targetValue = modData.modValueSetMethod switch {
            StatModifierData.ModValueSetMethod.Manual => modData.Stats[StatName.StatModifierValue],
            StatModifierData.ModValueSetMethod.DeriveFromOtherStats => throw new System.NotImplementedException(),
            StatModifierData.ModValueSetMethod.DeriveFromNumberOfTargets => throw new System.NotImplementedException(),
            StatModifierData.ModValueSetMethod.HardSetValue => throw new System.NotImplementedException(),
            StatModifierData.ModValueSetMethod.HardReset => throw new System.NotImplementedException(),
            StatModifierData.ModValueSetMethod.DeriveFromWeaponDamage => EntityManager.ActivePlayer.CurrentDamageRoll * modData.Stats[StatName.AbilityWeaponCoefficicent],
            _ => 0f,
        };

        return modData.invertDerivedValue == false ? targetValue : -targetValue;
    }

    private void ApplyToEntity(Entity target, StatModifier activeMod) {

        float globalDamageMultiplier = GetDamageModifier(activeMod);
        float modValueResult = StatAdjustmentManager.ApplyStatAdjustment(target, activeMod, activeMod.TargetStat, activeMod.VariantTarget, globalDamageMultiplier);

        if (activeMod.TargetStat == StatName.Health) {
            FloatingText text = FloatingTextManager.SpawnFloatingText(target.transform.position, modValueResult.ToString());
            text.SetColor(Data.floatingTextColor);
        }
    }

    private void RemoveFromEntity(Entity target, StatModifier activeMod) {
        StatAdjustmentManager.RemoveStatAdjustment(target, activeMod, activeMod.VariantTarget, Source);
    }

    public override bool ApplyToAbility(Ability target) {
        if (base.ApplyToAbility(target) == false)
            return false;

        for (int i = 0; i < modData.Count; i++) {

            StatModifier activeMod = PrepareStatMod(modData[i], target.Source);

            if (activeMod.VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackAbilityStatAdjustment(target, activeMod);
            }

            target.Stats.AddModifier(activeMod.TargetStat, activeMod);
        }


        return true;
    }

    public override void RemoveFromAbility(Ability target) {
        base.RemoveFromAbility(target);

        if (trackedAbilityMods.TryGetValue(target, out List<StatModifier> modList)) {
            for (int i = 0; i < modList.Count; i++) {
                target.Stats.RemoveModifier(modList[i].TargetStat, modList[i]);
            }

            trackedAbilityMods.Remove(target);
        }
        else {
            Debug.LogError("[Stat Adjustment EFFECT] An ability: " + target.Data.abilityName + " is not tracked.");
        }

    }

    //private void ApplyToAbility(Entity entity, string abilityName, StatModifier mod) {
    //    Ability target = AbilityUtilities.GetAbilityByName(abilityName, entity, AbilityCategory.KnownSkill);

    //    if (target != null) {
    //        Debug.Log("Applying an Ability stat adjustment: " + mod.TargetStat);
    //        target.Stats.AddModifier(mod.TargetStat, mod);
    //    }
    //}

    //private void RemoveFromAbility(Entity entity, string abilityName, StatModifier mod) {
    //    Ability target = AbilityUtilities.GetAbilityByName(abilityName, entity, AbilityCategory.KnownSkill);

    //    if (target != null) {
    //        target.Stats.RemoveModifier(mod.TargetStat, mod);
    //    }
    //}


    public override bool ApplyToEffect(Effect target) {
        if (base.ApplyToEffect(target) == false)
            return false;

        //Debug.Log("Applying an effect: " + Data.effectName + " to " + target.Data.effectName);


        for (int i = 0; i < modData.Count; i++) {

            StatModifier activeMod = PrepareStatMod(modData[i], target.Source);

            if (activeMod.VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackEffectStatAdjustment(target, activeMod);
            }

            if(Data.subTarget == EffectSubTarget.StatModifier) {
                //Debug.LogWarning("Applying a modifier mod: " + activeMod.Value);

                StatAdjustmentEffect adj = target as StatAdjustmentEffect;
                adj.AddStatAdjustmentModifier(activeMod);
            }
            else {
                //Debug.LogWarning("Applying an effect mod");
                StatAdjustmentManager.AddEffectModifier(target, activeMod);
            }
        }

        return true;
    }

    public override void RemoveFromEffect(Effect target) {
        base.RemoveFromEffect(target);

        if (trackedEffectMods.TryGetValue(target, out List<StatModifier> modList)) {
            for (int i = 0; i < modList.Count; i++) {

                if (Data.subTarget == EffectSubTarget.StatModifier) {
                    StatAdjustmentEffect adj = target as StatAdjustmentEffect;
                    adj.RemoveStatAdjustmentModifier(modList[i]);
                }
                else {
                    StatAdjustmentManager.RemoveEffectModifier(target, modList[i]);
                } 
            }

            trackedEffectMods.Remove(target);
        }
        else {
            Debug.LogError("[Stat Adjustment EFFECT] An effect: " + target.Data.effectName + " is not tracked.");
        }
    }


    //private void ApplyToEffect(Entity entity, string abilityName, string effectName, StatModifier mod, AbilityCategory category) {

    //    Effect targetEffect = AbilityUtilities.GetEffectByName(abilityName, effectName, entity, category);
    //    StatAdjustmentManager.AddEffectModifier(targetEffect, mod);


    //    //if(Data.applyToOtherStatAdjustment == true) {
    //    //    if (targetEffect is StatAdjustmentEffect) {
    //    //        StatAdjustmentEffect adjustment = targetEffect as StatAdjustmentEffect;

    //    //        if (adjustment.Data.effectDesignation == Data.effectDesignation)
    //    //            adjustment.AddStatAdjustmentModifier(mod);
    //    //    }
    //    //}
    //    //else {
    //    //    //Debug.Log("adding an effect: " + Data.effectName);
    //    //    StatAdjustmentManager.AddEffectModifier(targetEffect, mod);
    //    //}

    //}

    //private void RemoveFromEffect(Entity entity, string abilityName, string effectName, StatModifier mod, AbilityCategory category) {

    //    //StatAdjustmentEffect adj = AbilityUtilities.GetAbilityAndEffectByName(abilityName, effectName, entity).Item2 as StatAdjustmentEffect;
    //    //adj.RemoveStatAdjustmentModifier(mod, designation);

    //    Effect targetEffect = AbilityUtilities.GetEffectByName(abilityName, effectName, entity, category);
    //    StatAdjustmentManager.RemoveEffectModifier(targetEffect, mod);


    //    //if (Data.applyToOtherStatAdjustment == true) {
    //    //    if (targetEffect is StatAdjustmentEffect) {
    //    //        StatAdjustmentEffect adjustment = targetEffect as StatAdjustmentEffect;

    //    //        if (adjustment.Data.effectDesignation == Data.effectDesignation)
    //    //            adjustment.RemoveStatAdjustmentModifier(mod);
    //    //    }
    //    //}
    //    //else {
    //    //    //Debug.Log("removeing an effect: " + Data.effectName);
    //    //    StatAdjustmentManager.RemoveEffectModifier(targetEffect, mod);
    //    //}
    //}


    //private void ApplyToStatModifier(Entity entity, string abilityName, string effectName, StatModifier mod, StatModifierData.StatModDesignation designation, AbilityCategory category) {

    //    Effect targetEffect = AbilityUtilities.GetEffectByName(abilityName, effectName, entity, category);

    //    if (targetEffect is StatAdjustmentEffect) {
    //        StatAdjustmentEffect adjustment = targetEffect as StatAdjustmentEffect;

    //        if (adjustment.Data.effectDesignation == designation)
    //            adjustment.AddStatAdjustmentModifier(mod);
    //    }
    //}

    //private void RemoveFromStatModifier(Entity entity, string abilityName, string effectName, StatModifier mod, StatModifierData.StatModDesignation designation, AbilityCategory category) {
    //    Effect targetEffect = AbilityUtilities.GetEffectByName(abilityName, effectName, entity, category);

    //    if (targetEffect is StatAdjustmentEffect) {
    //        StatAdjustmentEffect adjustment = targetEffect as StatAdjustmentEffect;

    //        if (adjustment.Data.effectDesignation == designation)
    //            adjustment.RemoveStatAdjustmentModifier(mod);
    //    }
    //}

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;


        for (int i = 0; i < modData.Count; i++) {

            //StatModifier activeMod = new StatModifier(modData[i].value, modData[i].modifierType, modData[i].targetStat, Source);
            //float baseModValue = SetModValues(target, activeMod, modData[i]);
            //activeMod.UpdateModValue(baseModValue);

            //Debug.LogWarning("preparing a mod fomm data: " + modData[i].value);

            StatModifier activeMod = PrepareStatMod(modData[i], target);

            if (activeMod.VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackEntityStatAdjustment(target, activeMod);
            }

            ApplyToEntity(target, activeMod);


            //switch (Data.subTarget) {
            //    case EffectSubTarget.Entity:
            //    case EffectSubTarget.None:
            //        ApplyToEntity(target, activeMod);
            //        break;
            //    case EffectSubTarget.Effect:
            //        ApplyToEffect(target, Data.otherAbilityName, Data.otherEffectName, activeMod, AbilityCategory.Any);

            //        break;
            //    case EffectSubTarget.StatModifier:
            //        ApplyToStatModifier(target, Data.otherAbilityName, Data.otherEffectName, activeMod, Data.effectDesignation, AbilityCategory.Any);
            //        break;
            //    case EffectSubTarget.Ability:
            //        //ApplyToAbility(target, Data.otherAbilityName, activeMod);
            //        break;
            //}
        }


        return true;
    }

    private StatModifier PrepareStatMod(StatModifierData modData, Entity target) {
        StatModifier activeMod = new StatModifier(modData.value, modData.modifierType, modData.targetStat, Source, modData.variantTarget);
        float baseModValue = SetModValues(target, activeMod, modData);
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


        if (ParentAbility != null) {
            foreach (AbilityTag tag in ParentAbility.Tags) {
                float value = tag switch {
                    AbilityTag.None => 0f,
                    AbilityTag.Fire => throw new System.NotImplementedException(),
                    AbilityTag.Poison => throw new System.NotImplementedException(),
                    AbilityTag.Healing => 0f,
                    AbilityTag.Melee => Source.Stats[StatName.MeleeDamageModifier],
                    _ => 0f,
                };

                globalDamageMultiplier += value;
            }

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
            for (int i = 0; i < modList.Count; i++) {


                RemoveFromEntity(target, modList[i]);

                //switch (Data.subTarget) {
                //    case EffectSubTarget.None:
                //    case EffectSubTarget.Entity:
                //        RemoveFromEntity(target, modList[i]);
                //        break;
                //    case EffectSubTarget.Effect:
                //        RemoveFromEffect(target, Data.otherAbilityName, Data.otherEffectName, modList[i], AbilityCategory.Any);
                //        break;
                //    case EffectSubTarget.StatModifier:
                //        RemoveFromStatModifier(target, Data.otherAbilityName, Data.otherEffectName, modList[i], Data.effectDesignation, AbilityCategory.Any);
                //        break;
                //    case EffectSubTarget.Ability:
                //        //RemoveFromAbility(target, Data.otherAbilityName, modList[i]);
                //        break;
                //    default:
                //        break;
                //}
            }

            trackedEntityMods.Remove(target);
        }
        else {
            Debug.LogError("[Stat Adjustment EFFECT] A target: " + target.gameObject.name + " is not tracked.");
        }

    }


    public override string GetTooltip() {
        //return base.GetTooltip();

        string formated = TextHelper.FormatStat(modData[0].targetStat, modData[0].Stats[StatName.StatModifierValue]);

        string replacement = Data.effectDescription.Replace("{}", formated);

        return replacement;

    }


}
