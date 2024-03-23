using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
            TriggerType.AbilityLearned => new AbilityLearnedTrigger(data, source, parentAbility),
            TriggerType.RuneEquipped => new RuneEquippedTrigger(data, source, parentAbility),
            TriggerType.RuneUnequipped => new RuneUnequippedTrigger(data, source, parentAbility),
            TriggerType.Rider => new RiderTrigger(data, source, parentAbility),
            TriggerType.DashStarted => new DashStartedTrigger(data, source, parentAbility),
            TriggerType.ProjectilePierced => new ProjectilePiercedTrigger(data, source, parentAbility),
            TriggerType.ProjectileChained => new ProjectileChainedTrigger(data, source, parentAbility),
            TriggerType.UnitDiedWithStatus => new UnitDiedWithStatusTrigger(data, source, parentAbility),
            TriggerType.OverloadTriggered => new OverloadTrigger(data, source, parentAbility),
            TriggerType.ProjectileCreated => new ProjectileCreatedTrigger(data, source, parentAbility),
            TriggerType.AbilityResolved => new AbilityResolvedTrigger(data, source, parentAbility),
            TriggerType.AbilityInitiated => new AbilityInitiatedTrigger(data, source, parentAbility),
            TriggerType.AIActivated => new AIActivatedTrigger(data, source, parentAbility),
            TriggerType.UserCancelled => new UserCancelledTrigger(data, source, parentAbility),
            TriggerType.EntitySpawned => new EntitySpawnedTrigger(data, source, parentAbility),
            TriggerType.AbilityEnded => new AbilityEndedTrigger(data, source, parentAbility),
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
            ConstraintType.ParentAbilityTag => throw new NotImplementedException(),
            ConstraintType.Collision => throw new NotImplementedException(),
            ConstraintType.Range => new RangeConstraint(data, source, parentAbility),
            ConstraintType.StatMinimum => throw new NotImplementedException(),
            ConstraintType.StatMaximum => throw new NotImplementedException(),
            ConstraintType.EntityName => new EntityNameConstraint(data, source, parentAbility),
            ConstraintType.HasStatus => new HasStatusConstraint(data, source, parentAbility),
            ConstraintType.StateEntered => throw new NotImplementedException(),
            ConstraintType.StatedExited => throw new NotImplementedException(),
            ConstraintType.AbilityTag => new AbilityTagConstraint(data, source, parentAbility),
            ConstraintType.AbilityName =>new AbilityNameConstraint(data, source, parentAbility),
            ConstraintType.EffectName => new EffectNameConstraint(data, source, parentAbility),
            ConstraintType.EffectDesignation => new EffectDesignationConstraint(data, source, parentAbility),
            ConstraintType.EffectType => new EffectTypeConstraint(data, source, parentAbility),
            ConstraintType.AbilityActive => new AbilityActiveConstraint(data, source, parentAbility),
            ConstraintType.StatRatio => new StatRatioConstraint(data, source, parentAbility),
            ConstraintType.UnitIsMoving => new IsMovingConstraint(data, source, parentAbility),
            ConstraintType.AbilityOnHotbar => new AbilityOnHotbarConstraint(data, source, parentAbility),
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
            EffectType.AddChildAbility => new AddChildAbilityEffect(data, source, parentAbility),
            EffectType.ApplyOtherEffect => new ApplyOtherEffect(data, source, parentAbility),
            EffectType.AddStatScaler => new AddStatScalerEffect(data, source, parentAbility),
            EffectType.ForceStatusTick => new ForceStatusTickEffect(data, source, parentAbility),
            EffectType.SpawnEntity => new SpawnEntityEffect(data, source, parentAbility),
            EffectType.Teleport => new TeleportEffect(data, source, parentAbility),
            EffectType.None => new EmptyEffect(data, source, parentAbility),
            EffectType.ActivateOtherAbility => new ActivateAbilityEffect(data, source, parentAbility),
            EffectType.NPCStateChange => new NPCStateChangeEffect(data, source, parentAbility),
            EffectType.AddAbility => new AddAbilityEffect(data, source, parentAbility),
            EffectType.AddEffect => new AddEffectEffect(data, source, parentAbility),
            EffectType.AddTag => new AddTagEffect(data, source, parentAbility),
            EffectType.RemoveTag => new RemoveTagEffect(data, source, parentAbility),
            EffectType.SuppressEffect => new SuppressEffect(data, source, parentAbility),
            EffectType.RemoveEffect => new RemoveEffectEffect(data, source, parentAbility),
            _ => null,
        };

        if (effect == null) {
            Debug.LogError("An effect of Type: " + data.type + " does not exist. You probably need to add it to the factory");
        }


        return effect;

    }


}
