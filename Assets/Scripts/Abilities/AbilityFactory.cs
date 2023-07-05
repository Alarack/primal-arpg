using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Unity.VisualScripting.Member;

public static class AbilityFactory {



    public static Ability CreateAbility(AbilityData data, Entity source) {
        return new Ability(data, source);
    }


    public static AbilityTrigger CreateAbilityTrigger(TriggerData data, Entity source, Ability parentAbility = null) {


        AbilityTrigger trigger = data.type switch {

            TriggerType.UnitStatChanged => new StatChangedTrigger(data, source, parentAbility),
            TriggerType.UnitDied => new UnitDiedTrigger(data, source, parentAbility),
            TriggerType.UnitForgotten => new UnitForgottenTrigger(data, source, parentAbility),
            TriggerType.UnitDetected => new UnitDetectedTrigger(data, source, parentAbility),
            TriggerType.WeaponCooldownStarted => new WeaponCooldownStartedTrigger(data, source, parentAbility),
            TriggerType.WeaponCooldownFinished => new WeaponCooldownFinishedTrigger(data, source, parentAbility),
            TriggerType.Timed => new TimedTrigger(data, source, parentAbility),
            TriggerType.StateEntered => new StateEnteredTrigger(data, source, parentAbility),
            TriggerType.UserActivated => new UserActivatedTrigger(data, source, parentAbility),
            TriggerType.AbilityEquipped => new AbilityEquippedTrigger(data, source, parentAbility),
            TriggerType.AbilityUnequipped => new AbilityUnequippedTrigger(data, source, parentAbility),
            _ => null,
        };

        if (trigger == null) {
            Debug.LogError("A trigger of type: " + data.type + " does not exist. You probably need to add it to the factory");
        }

        return trigger;

    }

    public static AbilityConstraint CreateAbilityConstraint(ConstraintData data, Entity source, Ability parentAbility = null) {

        AbilityConstraint constraint = data.type switch {
            ConstraintType.StatChanged => new StatChangedConstraint(data, source, parentAbility),
            ConstraintType.Owner => new OwnerConstraint(data, source, parentAbility),
            ConstraintType.SourceOnly => new SourceOnlyConstraint(data, source, parentAbility),
            ConstraintType.Subtype => new SubtypeConstraint(data, source, parentAbility),
            ConstraintType.PrimaryType => new PrimaryTypeConstraint(data, source, parentAbility),
            ConstraintType.IsInState => new IsInStateConstraint(data, source, parentAbility),
            ConstraintType.HasTarget => new HasTargetConstraint(data, source, parentAbility),
            ConstraintType.Dashing => new DashingConstraint(data, source, parentAbility),
            _ => null,
        };

        if (constraint == null) {
            Debug.LogError("A constraint of type: " + data.type + " does not exist. You probably need to add it to the factory");
        }

        return constraint;

    }

    public static AbilityRecovery CreateAbilityRecovery(RecoveryData data, Entity source, Ability parentAbility) {
        AbilityRecovery recovery = data.type switch {
            RecoveryType.Timed => new AbilityRecoveryCooldown(data, source, parentAbility),
            _ => null,
        };

        return recovery;
    }

    public static Effect CreateEffect(EffectData data, Entity source, Ability parentAbility = null) {

        Effect effect = data.type switch {
            //EffectType.None => throw new NotImplementedException(),
            EffectType.StatAdjustment => new StatAdjustmentEffect(data, source, parentAbility),
            EffectType.SpawnProjectile => new SpawnProjectileEffect(data, source, parentAbility),
            EffectType.AddStatus => new AddStatusEffect(data, source, parentAbility),
            //EffectType.RemoveStatus => throw new NotImplementedException(),
            EffectType.Movement => new ForcedMovementEffect(data, source, parentAbility),
            _ => null,
        };

        if (effect == null) {
            Debug.LogError("An effect of Type: " + data.type + " does not exist. You probably need to add it to the factory");
        }


        return effect;

    }


}
