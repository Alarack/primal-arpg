using LL.Events;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static AbilityTrigger;
using static UnityEngine.GraphicsBuffer;


public abstract class Effect {

    public abstract EffectType Type { get; }
    public EffectTarget Targeting { get; protected set; }
    public Ability ParentAbility { get; protected set; }
    public EffectData Data { get; protected set; }
    public Entity Source { get; protected set; }
    public List<Entity> Targets { get; protected set; } = new List<Entity>();
    public List<Entity> ValidTargets { get { return targeter.GatherValidTargets(); } }
    public Entity LastTarget { get; protected set; }

    protected TriggerInstance currentTriggerInstance;
    protected List<AbilityConstraint> targetConstraints = new List<AbilityConstraint>();
    protected EffectTargeter targeter;

    public Effect(EffectData data, Entity source, Ability parentAbility = null) {
        this.Data = data;
        this.ParentAbility = parentAbility;
        this.Targeting = data.targeting;
        this.Source = source;
        SetupTargetConstraints();
        targeter = new EffectTargeter(this);
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


        if (Targets.Contains(target) == false) {
            Targets.Add(target);
        }
        //else {
        //    Debug.LogError(target.EntityName + " was already in the list of targets for an effect: " + Data.effectName + " on the source: " + Source.EntityName);
        //}

        LastTarget = target;

        return true;
    }

    public virtual void Remove(Entity target) {
        Targets.RemoveIfContains(target);
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
        for (int i = 0; i < Targets.Count; i++) {
            Remove(Targets[i]);
        }

        Targets.Clear();
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

        if(child == null) {
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

    private Dictionary<Entity, List<StatModifier>> statModDict = new Dictionary<Entity, List<StatModifier>>();

    public StatAdjustmentEffect(EffectData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    public override bool Apply(Entity target) {
        if (base.Apply(target) == false)
            return false;

        for (int i = 0; i < Data.modData.Count; i++) {

            StatModifier activeMod = new StatModifier(Data.modData[i].value, Data.modData[i].modifierType, Data.modData[i].targetStat, Source);

            if (Data.modData[i].variantTarget != StatModifierData.StatVariantTarget.RangeCurrent) {
                TrackStatAdjustment(target, activeMod);
            }



            float globalDamageMultiplier = GetDamageModifier(activeMod);


            float modValueResult = StatAdjustmentManager.ApplyStatAdjustment(target, activeMod, Data.modData[i].targetStat, Data.modData[i].variantTarget, globalDamageMultiplier);

            if (Data.modData[i].targetStat == StatName.Health) {
                FloatingText text = FloatingTextManager.SpawnFloatingText(target.transform.position, modValueResult.ToString());
                text.SetColor(Data.floatingTextColor);
            }
        }


        return true;
    }

    private float GetDamageModifier(StatModifier mod) {

        if (mod.TargetStat != StatName.Health)
            return 1f;

        if(mod.ModType != StatModType.Flat)
            return 1f;

        if(mod.Value > 0f) //Do helaing mods here
            return 1f;

        float globalDamageMultiplier = 1 + Source.Stats[StatName.GlobalDamageModifier];
        

        if(ParentAbility != null) {
            foreach(AbilityTag tag in ParentAbility.Tags) {
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

    private void TrackStatAdjustment(Entity target, StatModifier mod) {
        if (statModDict.TryGetValue(target, out List<StatModifier> modList)) {
            modList.Add(mod);
        }
        else {
            List<StatModifier> newList = new List<StatModifier>() { mod };
            statModDict.Add(target, newList);
        }
    }

    public override void Remove(Entity target) {
        base.Remove(target);

        if (statModDict.TryGetValue(target, out List<StatModifier> modList)) {
            for (int i = 0; i < modList.Count; i++) {
                target.Stats.RemoveModifier(modList[i].TargetStat, modList[i]);
            }

            statModDict.Remove(target);
        }
        else {
            Debug.LogError("[Stat Adjustment EFFECT] A target: " + target.gameObject.name + " is not tracked.");
        }

    }


}
