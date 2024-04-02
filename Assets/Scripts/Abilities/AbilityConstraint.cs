using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriggerInstance = AbilityTrigger.TriggerInstance;

public abstract class AbilityConstraint {

    public abstract ConstraintType Type { get; }

    protected bool inverse;
    protected ConstraintData data;
    protected Ability parentAbility;
    protected Effect parentEffect;
    protected Entity source;

    public AbilityConstraint(ConstraintData data, Entity source, Ability parentAbility = null) {
        this.data = data;
        this.parentAbility = parentAbility;
        this.inverse = data.inverse;
        this.source = source;
    }

    public void SetParentEffect(Effect parentEffect) {
        this.parentEffect = parentEffect;
    }

    public abstract bool Evaluate(Entity target, TriggerInstance triggerInstance);

    public virtual  bool Evaluate(Ability ability, TriggerInstance triggerInstance) {
        return false;
    }

    public virtual bool Evaluate(Effect effect, TriggerInstance triggerInstance) {
        return false;
    }

}


public class PrimaryTypeConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.PrimaryType;


    private Entity.EntityType targetType;

    public PrimaryTypeConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        this.targetType = data.targetPrimaryType;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        bool result = target.entityType == targetType;

        return inverse == false ? result : !result;
    }

}

public class SubtypeConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.Subtype;


    private Entity.EntitySubtype targetSubtype;

    public SubtypeConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        this.targetSubtype = data.targetSubtype;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        bool result = target.subtypes.Contains(targetSubtype);

        return inverse == false ? result : !result;
    }

}

public class IsMovingConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.UnitIsMoving;


    private Entity.EntitySubtype targetSubtype;

    public IsMovingConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        this.targetSubtype = data.targetSubtype;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        bool result = target.IsMoving >= data.movementMagnitudeLimit;

        return inverse == false ? result : !result;
    }

}

public class HasTargetConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.HasStatus;

    public HasTargetConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        if (target is NPC) {
            NPC npc = (NPC)target;

            bool result = npc.Brain.Sensor.LatestTarget != null;

            return inverse == false ? result : !result;
        }

        return false;
    }

}

public class StatChangedConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.StatChanged;

    private StatName targetStat;
    private GainedOrLost changeDirection;

    public StatChangedConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        this.targetStat = data.statChangeTarget;
        this.changeDirection = data.changeDirection;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        StatChangedTrigger.StatChangeTriggerInstance trigger = triggerInstance as StatChangedTrigger.StatChangeTriggerInstance;

        if (trigger == null) {
            Debug.LogError("A non stat change trigger has been passed to a stat change constraint.");
            return false;
        }

        if (targetStat != trigger.targetStat) {
            return false;
        }

        //Debug.Log(trigger.changeValue + " is the change value");


        GainedOrLost statDirection;

        if (trigger.changeValue > 0f) {
            statDirection = GainedOrLost.Gained;
        }
        else {
            statDirection = GainedOrLost.Lost;
        }

        bool result;
        if (changeDirection == statDirection)
            result = true;
        else
            result = false;


        //if(parentAbility != null) 
        //    Debug.LogWarning("Result for Unit Stat Changed on: " + parentAbility.Data.abilityName + " " + result);
        //else
        //    Debug.LogWarning("Result for Unit Stat Changed againt: " + target.EntityName + " " + result);

        return inverse == false ? result : !result;
    }

}

public class StatRatioConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.StatRatio;

    public StatRatioConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        StatRange targetStat = target.Stats.GetStatByName(data.statRatioTarget) as StatRange;

        if(targetStat == null) {
            Debug.LogError("A stat ratio constraint tried to get a non-stat range stat: " + data.statRatioTarget);
            return false;
        }

        

        bool result = targetStat.Ratio <= data.targetRatio;

        //Debug.Log("Stat ratio: " + targetStat.Ratio);
        //Debug.Log("Check Ratio: " + data.targetRatio);
        //Debug.Log("Ratio Check: " + target.EntityName + result);


        return inverse == false ? result : !result;
    }


    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {

        bool result = ability == triggerInstance.SourceAbility;

        //Debug.Log("Testing: " + ability.Data.abilityName + " against " + triggerInstance.sourceAbility.Data.abilityName + ". Result: " + result);

        return inverse == false ? result : !result;

    }
}


public class SourceOnlyConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.SourceOnly;

    public SourceOnlyConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        bool result = target == source;

        return inverse == false ? result : !result;
    }


    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {

        bool result = ability == triggerInstance.SourceAbility;

        //if(ability.Data.abilityName == "Swiftness") {
        //    Debug.Log("Testing: " + ability.Data.abilityName + " against " + triggerInstance.SourceAbility.Data.abilityName + ". Result: " + result);

        //}


        return inverse == false ? result : !result;

    }
}

public class AbilityOnHotbarConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.AbilityOnHotbar;

    public AbilityOnHotbarConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        Debug.LogError("An Ability On Hotbar Constraint is trying to evaluate an Entity. This is not supported");
        return false;
    }


    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {

        bool result = EntityManager.ActivePlayer.AbilityManager.IsAbilityOnHotbar(ability);

        //if(ability.Data.abilityName == "Swiftness") {
        //    Debug.Log("Testing: " + ability.Data.abilityName + " against " + triggerInstance.SourceAbility.Data.abilityName + ". Result: " + result);

        //}


        return inverse == false ? result : !result;

    }
}

public class OwnerConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.Owner;

    private OwnerConstraintType ownerTarget;

    public OwnerConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        ownerTarget = data.ownerTarget;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {


        bool result = ownerTarget switch {
            OwnerConstraintType.Friendly => target.ownerType == source.ownerType,
            OwnerConstraintType.Enemy => target.ownerType != source.ownerType,
            OwnerConstraintType.Ally => throw new System.NotImplementedException(),
            OwnerConstraintType.Neutral => throw new System.NotImplementedException(),
            _ => throw new System.NotImplementedException(),
        };


        //bool result = target.ownerType == ownerTarget;

        //Debug.Log(target.EntityName + " is owned by " + ownerTarget + ": " + result);

        return inverse == false ? result : !result;
    }

    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {
        
        
        return Evaluate(ability.Source, triggerInstance);
        
        //bool result = ability.Source.ownerType == ownerTarget;

        //return inverse == false ? result : !result;
    }

    public override bool Evaluate(Effect effect, TriggerInstance triggerInstance) {
        
        return Evaluate(effect.Source, triggerInstance);
        
        //bool result = effect.Source.ownerType == ownerTarget; 
        
        //return inverse == false ? result : !result;
    }
}

public class IsInStateConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.IsInState;


    private string targetStateName;

    public IsInStateConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        targetStateName = data.targetStateData.stateName;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        //Is the target in the correct state?
        if(target is NPC) {
            NPC npc = (NPC)target;

            bool result = npc.Brain.CurrentStateName == targetStateName;

            return inverse == false ? result : !result;
        }


        return false;
    }
}

public class RangeConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.Range;



    public RangeConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        float range = 0f;


        switch (data.rangeToWhat) {
            case EffectTarget.Source:
                range = Vector2.Distance(target.transform.position, source.transform.position);
                break;
            case EffectTarget.Trigger:
                range = Vector2.Distance(target.transform.position, triggerInstance.TriggeringEntity.transform.position);
                break;
            case EffectTarget.Cause:
                range = Vector2.Distance(target.transform.position, triggerInstance.CauseOfTrigger.transform.position);
                break;
            case EffectTarget.OtherEffectTarget:
                Debug.LogWarning("Range Constraint: Other Effect target is not yet setup");
                break;
            case EffectTarget.CurrentAIBrainTarget:
                NPC npc = parentAbility.Source as NPC;
                if(npc == null) {
                    Debug.LogError("A Range Constarint is set to Current AI Brain Target, but the source isn't an NPC: " + parentAbility.Data.abilityName);
                    return false;
                }

                Entity sensortarget = npc.Brain.Sensor.LatestTarget;
                if(sensortarget == null) {
                    //Debug.Log("No target");
                    return false;
                }

                range = Vector2.Distance(target.transform.position, sensortarget.transform.position);


                //Debug.Log(range + " is the current range");

                break;

            default:
                throw new System.NotImplementedException();
            
        }


        float effectRange = parentEffect != null ? parentEffect.Stats[StatName.EffectRange] : parentAbility.Stats[StatName.EffectRange];


        float maxrange = effectRange > 0 ? effectRange : data.maxRange; //parentEffect != null ? parentEffect.Stats[StatName.EffectRange] : parentAbility.Stats[StatName.EffectRange];

        bool result = range <= maxrange /*data.maxRange*/ && range >= data.minRange;

        //Debug.Log(result + " " + range);

        return inverse == false ? result : !result;
    }

}

public class DashingConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.Dashing;

    public DashingConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
     
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        if(target.Movement == null) 
            return false;

        bool result = target.Movement.IsDashing;

        return inverse == false ? result : !result;
    }

}

public class HasStatusConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.HasStatus;

    public HasStatusConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        bool result = target.HasStatus(data.targetStatus);

        //Debug.Log(target.EntityName + " has " + data.targetStatus + ": " + result);


        return inverse == false ? result : !result;
    }

}

public class EffectTypeConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.EffectType;

    public EffectTypeConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        Debug.LogError("A constraint of type: " + Type + " at trying to target an entity. This is not supported");


        return false;
    }

    public override bool Evaluate(Effect effect, TriggerInstance triggerInstance) {
        bool result = effect.Data.type == data.targetEffectType;


        return inverse == false ? result : !result;
    }

}

public class EffectAppliedToConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.EffectAppliedTo;

    public EffectAppliedToConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        Debug.LogError("A constraint of type: " + Type + " at trying to target an entity. This is not supported");


        return false;
    }

    public override bool Evaluate(Effect effect, TriggerInstance triggerInstance) {

        bool result = false;

        Entity targetEntity = triggerInstance.TriggeringEntity;

        result = effect.EntityTargets.Contains(targetEntity);


        //Effect otherEffect = AbilityUtilities.GetEffectByName(parentEffect.Data.otherEffectName, parentEffect.Data.otherAbilityName, parentEffect.Source, AbilityCategory.Any);
        //for (int i = 0; i < effect.EntityTargets.Count; i++) {
        //    if (otherEffect.EntityTargets.Contains(effect.EntityTargets[i])) {
        //        result = true;
        //        break;
        //    }
        //}
 

        return inverse == false ? result : !result;
    }
}


public class AbilityTagConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.AbilityTag;


    private AbilityTag targetTag;

    public AbilityTagConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        this.targetTag = data.targetAbilityTag;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        Debug.LogError("A constraint of type: " + Type + " at trying to target an entity. " +
            "This is not supported. Parent Effect: " + parentEffect.Data.effectName +
            "Source: " + parentEffect.Source.EntityName);

        return false;

    }

    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {
        bool result = ability.Tags.Contains(targetTag);

        //Debug.LogWarning("Testing: " + ability.Data.abilityName + " for " + targetTag + ". Result: " + result);
        return inverse == false ? result : !result;
    }

    public override bool Evaluate(Effect effect, TriggerInstance triggerInstance) {

        

        bool result = effect.ParentAbility == null ? false : effect.ParentAbility.Tags.Contains(targetTag);

        //Debug.Log("Checking an effect for a tag: " + effect.Data.effectName + " Result: " + result);

        return inverse == false ? result : !result;
    }

}

public class EffectDesignationConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.EffectDesignation;


    private StatModifierData.StatModDesignation designation;

    public EffectDesignationConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        this.designation = data.effectDesigantion;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {
        Debug.LogError("A constraint of type: " + Type + " at trying to target an entity. This is not supported");


        return false;
    }

    public override bool Evaluate(Effect effect, TriggerInstance triggerInstance) {
        bool result = effect.Data.effectDesignation == designation;

        return inverse == false ? result : !result;
    }

}

public class EffectNameConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.EffectName;

    public EffectNameConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {
        Debug.LogError("A constraint of type: " + Type + " at trying to target an entity. This is not supported");


        return false;
    }

    public override bool Evaluate(Effect effect, TriggerInstance triggerInstance) {
        bool result = effect.Data.effectName == data.targetEffectName;


        //if(result == true) {
        //    Debug.LogWarning("Testing Effect: " + effect.Data.effectName);
        //    Debug.LogWarning("Target Effect: " + data.targetEffectName);
        //    Debug.LogWarning("Parent Ability: " + parentAbility.Data.abilityName);
        //    Debug.LogWarning("Testing Effect Parent Ability: " + effect.ParentAbility.Data.abilityName);
        //}


        return inverse == false ? result : !result;
    }

}

public class AbilityNameConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.AbilityName;

    public AbilityNameConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {
        Debug.LogError("A constraint of type: " + Type + " at trying to target an entity. This is not supported");


        return false;
    }

    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {
        bool result = ability.Data.abilityName == data.targetAbiltyName;

        //Debug.Log("Result of a name check on: " + ability.Data.abilityName + " : " + result);

        return inverse == false ? result : !result;
    }

}

public class AbilityActiveConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.AbilityActive;

    public AbilityActiveConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {
        Debug.LogError("A constraint of type: " + Type + " at trying to target an entity. This is not supported");

        Ability targetAbility = target.GetAbilityByName(data.targetAbiltyName, AbilityCategory.Any);

        if(targetAbility == null) {
            Debug.LogWarning("Could not find an Ability: " + data.targetAbiltyName + " whne checking for an active ability on: " + target.EntityName);
            return false;
        }

        bool result = targetAbility.IsActive;

        return inverse == false ? result : !result;
    }

    public override bool Evaluate(Effect effect, TriggerInstance triggerInstance) {
        Ability targetAbility = effect.Source.GetAbilityByName(data.targetAbiltyName, AbilityCategory.Any);

        if (targetAbility == null) {
            Debug.LogWarning("Could not find an Ability: " + data.targetAbiltyName + " whne checking for an active ability on: " + effect.Data.effectName);
            return false;
        }

        bool result = targetAbility.IsActive;

        return inverse == false ? result : !result;
    }

    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {
        bool result = ability.IsActive;

        //Debug.Log("Result of a active check on: " + ability.Data.abilityName + " : " + result);

        return inverse == false ? result : !result;
    }

}

public class EntityNameConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.EntityName;

    public EntityNameConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {
       bool result = target.EntityName == data.targetEntityName;

        return inverse == false ? result : !result;
    }

   

}

