using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;

public static class StatAdjustmentManager {


    public static void ApplyDataModifier(StatModifierData data, StatModifier mod) {
        data.Stats.AddModifier(mod.TargetStat, mod);
    }

    public static void RemoveDataModifiyer(StatModifierData data, StatModifier mod) {
        data.Stats.RemoveModifier(mod.TargetStat, mod);
    }

    public static void AddEffectModifier(Effect effect, StatModifier mod) {
        effect.Stats.AddModifier(mod.TargetStat, mod);
    }

    public static void RemoveEffectModifier(Effect effect, StatModifier mod) {
        effect.Stats.RemoveModifier(mod.TargetStat, mod);
    }


    public static float ApplyStatAdjustment(Entity target, StatModifierData modData, Entity source, float multiplier = 1f) {


        Action<StatName, float, StatModType, object> statModAction = modData.variantTarget switch {
            StatModifierData.StatVariantTarget.Simple => target.Stats.AddModifier,
            StatModifierData.StatVariantTarget.RangeCurrent => target.Stats.AdjustStatRangeCurrentValue,
            StatModifierData.StatVariantTarget.RangeMin => target.Stats.AddMinValueModifier,
            StatModifierData.StatVariantTarget.RangeMax => target.Stats.AddMaxValueModifier,
            _ => null,
        };


        float modValue = modData.value * multiplier;

        statModAction?.Invoke(modData.targetStat, modValue, modData.modifierType, source);


        SendStatChangeEvent(modData.targetStat, target, source, modValue);

        return modValue;
    }


    public static float ApplyStatAdjustment(Entity target, StatModifier mod, StatModifierData.StatVariantTarget variant, Entity source, float multiplier = 1f) {


        Action<StatName, StatModifier> statModAction = variant switch {
            StatModifierData.StatVariantTarget.Simple => target.Stats.AddModifier,
            StatModifierData.StatVariantTarget.RangeCurrent => target.Stats.AdjustStatRangeCurrentValue,
            StatModifierData.StatVariantTarget.RangeMin => target.Stats.AddMinValueModifier,
            StatModifierData.StatVariantTarget.RangeMax => target.Stats.AddMaxValueModifier,
            _ => null,
        };


        float modValue = mod.Value * multiplier;

        mod.UpdateModValue(modValue);

        statModAction?.Invoke(mod.TargetStat, mod);


        SendStatChangeEvent(mod.TargetStat, target, source, mod.Value);

        return modValue;
    }

    public static float RemoveStatAdjustment(Entity target, StatModifier mod, StatModifierData.StatVariantTarget variant, Entity source, bool removeRangeAdjsument = false) {

        Action<StatName, StatModifier> statModAction = variant switch {
            StatModifierData.StatVariantTarget.Simple => target.Stats.RemoveModifier,
            StatModifierData.StatVariantTarget.RangeCurrent => null, /*when removeRangeAdjsument == true => target.Stats.RemoveCurrentRangeAdjustment*/
            StatModifierData.StatVariantTarget.RangeMin => target.Stats.RemoveMinValueModifier,
            StatModifierData.StatVariantTarget.RangeMax => target.Stats.RemoveMaxValueModifier,
            _ => null,
        };


        statModAction?.Invoke(mod.TargetStat, mod);


        SendStatChangeEvent(mod.TargetStat, target, source, mod.Value);

        return mod.Value;
    }

    public static float ApplyStatAdjustment(Entity target, float value, StatName targetStat, StatModType modType, StatModifierData.StatVariantTarget statVariant, object source, float multiplier = 1f) {
        StatModifier mod = new StatModifier(value, modType, targetStat, source, statVariant);
        return ApplyStatAdjustment(target, mod, targetStat, statVariant, multiplier);
    }

    public static float DealDamageOrHeal(Entity target, float value, object source, float multiplier = 1f) {
        StatModifier mod = new StatModifier(value, StatModType.Flat, StatName.Health, source, StatModifierData.StatVariantTarget.RangeCurrent);
        return ApplyStatAdjustment(target, mod, StatName.Health, StatModifierData.StatVariantTarget.RangeCurrent, multiplier);
    }

    public static float AdjustCDR(Entity target, float value, object source, float multiplier = 1f) {
        StatModifier mod = new StatModifier(value, StatModType.Flat, StatName.CooldownReduction, source, StatModifierData.StatVariantTarget.Simple);
        return ApplyStatAdjustment(target, mod, StatName.CooldownReduction, StatModifierData.StatVariantTarget.RangeCurrent, multiplier);
    }

    public static float ApplyStatAdjustment(Entity target, StatModifier mod, StatName targetStat, StatModifierData.StatVariantTarget statVarient, float multiplier = 1f) {

        Action<StatName, StatModifier> statModAction = statVarient switch {
            StatModifierData.StatVariantTarget.Simple => target.Stats.AddModifier,
            StatModifierData.StatVariantTarget.RangeCurrent => target.Stats.AdjustStatRangeCurrentValue,
            StatModifierData.StatVariantTarget.RangeMin => target.Stats.AddMinValueModifier,
            StatModifierData.StatVariantTarget.RangeMax => target.Stats.AddMaxValueModifier,
            _ => null,
        };

        mod.UpdateModValue(mod.Value * multiplier);

        statModAction?.Invoke(targetStat, mod);

        if (mod.Source as Entity == null) {
            Debug.LogError("We're assuming all mod sources are Entities, but one is being sent in that isnt an entity");
        }

        SendStatChangeEvent(targetStat, target, (Entity)mod.Source, mod.Value);

        return mod.Value;

    }




    private static void SendStatChangeEvent(StatName targetStat, Entity target, Entity source, float changeValue) {
        EventData eventData = new EventData();
        eventData.AddEntity("Target", target);
        eventData.AddEntity("Source", source);
        eventData.AddFloat("Value", changeValue);
        eventData.AddInt("Stat", (int)targetStat);

        EventManager.SendEvent(GameEvent.UnitStatAdjusted, eventData);

        //Debug.Log("Adjusting a stat: " + targetStat + " Value: " + changeValue + " target: " + target.gameObject.name);
    }


}
