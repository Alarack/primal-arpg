using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using TriggerInstance = AbilityTrigger.TriggerInstance;
//using AbilityTriggerInstance = AbilityTrigger.AbilityTriggerInstance;

public abstract class AbilityConstraint {

    public abstract ConstraintType Type { get; }

    protected bool inverse;
    protected ConstraintData data;
    protected Ability parentAbility;
    protected Entity source;

    public AbilityConstraint(ConstraintData data, Entity source, Ability parentAbility = null) {
        this.data = data;
        this.parentAbility = parentAbility;
        this.inverse = data.inverse;
        this.source = source;
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

        //Debug.LogWarning("Result for Unit Stat Changed on: " + parentAbility.Data.abilityName + " " + result);


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

        //Debug.Log("Testing: " + ability.Data.abilityName + " against " + triggerInstance.sourceAbility.Data.abilityName + ". Result: " + result);

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

        bool result = target.ownerType == ownerTarget;

        //Debug.Log(target.EntityName + " is owned by " + ownerTarget + ": " + result);

        return inverse == false ? result : !result;
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
        }

        bool result = range <= data.maxRange && range >= data.minRange;



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


public class AbilityTagConstraint : AbilityConstraint {

    public override ConstraintType Type => ConstraintType.AbilityTag;


    private AbilityTag targetTag;

    public AbilityTagConstraint(ConstraintData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        this.targetTag = data.targetAbilityTag;
    }

    public override bool Evaluate(Entity target, TriggerInstance triggerInstance) {

        return false;

    }

    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {
        bool result = ability.Tags.Contains(targetTag);

        //Debug.LogWarning("Testing: " + ability.Data.abilityName + " for " + targetTag + ". Result: " + result);
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
        return false;
    }

    public override bool Evaluate(Ability ability, TriggerInstance triggerInstance) {
        bool result = ability.Data.abilityName == data.targetAbiltyName;

        //Debug.Log("Result of a name check on: " + ability.Data.abilityName + " : " + result);

        return inverse == false ? result : !result;
    }

}

