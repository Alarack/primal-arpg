using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using static UnityEngine.GraphicsBuffer;

public static class StatAdjustmentManager {


    public static void ApplyDataModifier(StatModifierData data, StatModifier mod) {
        data.Stats.AddModifier(mod.TargetStat, mod);
    }

    public static void RemoveDataModifiyer(StatModifierData data, StatModifier mod) {
        data.Stats.RemoveModifier(mod.TargetStat, mod);
    }

    public static void AddEffectModifier(Effect effect, StatModifier mod, bool addMissingStat) {


        if (effect.Stats.Contains(mod.TargetStat) == false) {

            if (addMissingStat == false) {
                Debug.LogWarning(effect.Data.effectName + " does not have " + mod.TargetStat + " whem adding.");
                return;
            }

            SimpleStat newStat = new SimpleStat(mod.TargetStat, 0f);
            effect.Stats.AddStat(newStat);
        }


        effect.Stats.AddModifier(mod.TargetStat, mod);

        SendEffectStatChangeEvent(mod.TargetStat, effect, mod.Value);
    }

    public static void RemoveEffectModifier(Effect effect, StatModifier mod) {
        effect.Stats.RemoveModifier(mod.TargetStat, mod);

        SendEffectStatChangeEvent(mod.TargetStat, effect, mod.Value);
    }

    public static void AddAbilityModifier(Ability ability, StatModifier mod, bool addMissingStat) {

        if (ability.Stats.Contains(mod.TargetStat) == false) {

            if (addMissingStat == false) {
                Debug.LogWarning(ability.Data.abilityName + " does not have " + mod.TargetStat + " whem adding.");
                return;
            }

            SimpleStat newStat = new SimpleStat(mod.TargetStat, 0f);
            ability.Stats.AddStat(newStat);
        }


        ability.Stats.AddModifier(mod.TargetStat, mod);

        SendAbilityStatChangeEvent(mod.TargetStat, ability, mod.Value);
    }

    public static void RemoveAbilityModifier(Ability ability, StatModifier mod) {
        ability.Stats.RemoveModifier(mod.TargetStat, mod);

        SendAbilityStatChangeEvent(mod.TargetStat, ability, mod.Value);
    }


    public static float ApplyStatAdjustment(Entity target, StatModifierData modData, Entity source, Ability sourceAbility, float multiplier = 1f) {


        Action<StatName, float, StatModType, object> statModAction = modData.variantTarget switch {
            StatModifierData.StatVariantTarget.Simple => target.Stats.AddModifier,
            StatModifierData.StatVariantTarget.RangeCurrent => target.Stats.AdjustStatRangeCurrentValue,
            StatModifierData.StatVariantTarget.RangeMin => target.Stats.AddMinValueModifier,
            StatModifierData.StatVariantTarget.RangeMax => target.Stats.AddMaxValueModifier,
            _ => null,
        };


        float modValue = modData.value * multiplier;

        statModAction?.Invoke(modData.targetStat, modValue, modData.modifierType, source);


        SendStatChangeEvent(modData.targetStat, target, source, sourceAbility, modValue);

        return modValue;
    }


    public static float ApplyStatAdjustment(Entity target, StatModifier mod, StatModifierData.StatVariantTarget variant, Entity source, Ability sourceAbility, float multiplier = 1f) {


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


        SendStatChangeEvent(mod.TargetStat, target, source, sourceAbility, mod.Value);

        return modValue;
    }

    public static float RemoveStatAdjustment(Entity target, StatModifier mod, StatModifierData.StatVariantTarget variant, Entity source, Ability sourceAbility, bool removeRangeAdjsument = false) {

        if (target.Stats.Contains(mod.TargetStat) == false) {
            Debug.LogWarning(target.EntityName + " does not have " + mod.TargetStat + " whem removing.");
            return 0f;
        }

        Action<StatName, StatModifier> statModAction = variant switch {
            StatModifierData.StatVariantTarget.Simple => target.Stats.RemoveModifier,
            StatModifierData.StatVariantTarget.RangeCurrent => null, /*when removeRangeAdjsument == true => target.Stats.RemoveCurrentRangeAdjustment*/
            StatModifierData.StatVariantTarget.RangeMin => target.Stats.RemoveMinValueModifier,
            StatModifierData.StatVariantTarget.RangeMax => target.Stats.RemoveMaxValueModifier,
            _ => null,
        };

        //if(target.Stats.Contains(mod.TargetStat) == false) {
        //    Debug.LogError(target.EntityName + " does not have " + mod.TargetStat);
        //}


        statModAction?.Invoke(mod.TargetStat, mod);

        //Debug.Log("Removing: " + mod.TargetStat);


        //Debug.Log(mod.TargetStat + " " + mod.ModType + " With a value of: " + mod.Value + " removed from: " + target.EntityName);
        //Debug.Log("Resulting Value for : " + mod.TargetStat + " : " + target.Stats[mod.TargetStat]);


        SendStatChangeEvent(mod.TargetStat, target, source, sourceAbility, mod.Value, true);

        return mod.Value;
    }


    public static float AdjustStatRerolls(float value) {
        StatModifier mod = new StatModifier(value, StatModType.Flat, StatName.StatReroll, EntityManager.ActivePlayer, StatModifierData.StatVariantTarget.Simple);
        return ApplyStatAdjustment(EntityManager.ActivePlayer, mod, mod.TargetStat, mod.VariantTarget, null);
    }

    public static float ApplyStatAdjustment(Entity target, float value, StatName targetStat, StatModType modType, StatModifierData.StatVariantTarget statVariant, object source, Ability sourceAbility, float multiplier = 1f) {
        StatModifier mod = new StatModifier(value, modType, targetStat, source, statVariant);
        return ApplyStatAdjustment(target, mod, targetStat, statVariant, sourceAbility, multiplier);
    }

    //public static float DealDamageOrHeal(Entity target, float value, object source, Ability sourceAbility, float multiplier = 1f) {
    //    StatModifier mod = new StatModifier(value, StatModType.Flat, StatName.Health, source, StatModifierData.StatVariantTarget.RangeCurrent);
    //    return ApplyStatAdjustment(target, mod, StatName.Health, StatModifierData.StatVariantTarget.RangeCurrent, sourceAbility, multiplier);
    //}

    public static float AdjustCDR(Entity target, float value, object source, Ability sourceAbility, float multiplier = 1f) {
        StatModifier mod = new StatModifier(value, StatModType.Flat, StatName.CooldownReduction, source, StatModifierData.StatVariantTarget.Simple);
        return ApplyStatAdjustment(target, mod, StatName.CooldownReduction, StatModifierData.StatVariantTarget.RangeCurrent, sourceAbility, multiplier);
    }

    public static float ApplyStatAdjustment(Entity target, StatModifier mod, StatName targetStat, StatModifierData.StatVariantTarget statVarient, Ability sourceAbility, float multiplier = 1f, bool addMissingStat = false, Entity delivery = null) {

        if (target.Stats.Contains(mod.TargetStat) == false) {

            if (addMissingStat == false) {
                Debug.LogWarning(target.EntityName + " does not have " + mod.TargetStat + " whem adding.");
                return 0f;
            }

            //Debug.Log(target.EntityName + " does not have " + mod.TargetStat + ". Adding it as a Simple stat with 0 value.");
            SimpleStat newStat = new SimpleStat(targetStat, 0f);
            target.Stats.AddStat(newStat);
        }


        Action<StatName, StatModifier> statModAction = statVarient switch {
            StatModifierData.StatVariantTarget.Simple => target.Stats.AddModifier,
            StatModifierData.StatVariantTarget.RangeCurrent => target.Stats.AdjustStatRangeCurrentValue,
            StatModifierData.StatVariantTarget.RangeMin => target.Stats.AddMinValueModifier,
            StatModifierData.StatVariantTarget.RangeMax => target.Stats.AddMaxValueModifier,
            _ => null,
        };

        mod.UpdateModValue(mod.Value * multiplier);

        statModAction?.Invoke(targetStat, mod);

        //if(targetStat == StatName.Health && sourceAbility != null) {
        //    Debug.LogWarning(sourceAbility.Data.abilityName + " is changing health ");
        //}

        //if (targetStat == StatName.VulnerableTime) {
        //    Debug.Log(targetStat + " " + mod.ModType + " With a value of: " + mod.Value + " applied to: " + target.EntityName);
        //    //Debug.Log("Applying: " + targetStat);
        //    Debug.Log("Resulting Value for : " + targetStat + " : " + target.Stats[targetStat]);
        //}

        try {
            SendStatChangeEvent(targetStat, target, (Entity)mod.Source, sourceAbility, mod.Value, false, delivery);
            //string name = ((Entity)mod.Source).EntityName;
            //Debug.Log("Source: " +  name);
        }
        catch (System.Exception e) {
            Debug.LogError(e.Message);
            Debug.LogError("We're assuming all mod sources are Entities, but one is being sent in that isnt an entity");
            Debug.LogError("Souce: " + mod.Source.GetType().Name);
        }

        //SendStatChangeEvent(targetStat, target, (Entity)mod.Source, sourceAbility, mod.Value);

        return mod.Value;

    }


    private static void SendAbilityStatChangeEvent(StatName statName, Ability target, float changeValue) {
        EventData data = new EventData();
        data.AddAbility("Ability", target);
        data.AddInt("Stat", (int)statName);
        data.AddFloat("Value", changeValue);

        EventManager.SendEvent(GameEvent.AbilityStatAdjusted, data);
    }

    private static void SendEffectStatChangeEvent(StatName statName, Effect target, float changeValue) {
        EventData data = new EventData();
        data.AddEffect("Effect", target);
        data.AddInt("Stat", (int)statName);
        data.AddFloat("Value", changeValue);

        EventManager.SendEvent(GameEvent.EffectStatAdjusted, data);
    }

    private static void SendStatChangeEvent(StatName targetStat, Entity target, Entity source, Ability sourceAbility, float changeValue, bool isRemoveal = false, Entity delivery = null) {
        EventData eventData = new EventData();
        eventData.AddEntity("Target", target);
        eventData.AddEntity("Source", source);
        eventData.AddEntity("Delivery", delivery);
        eventData.AddAbility("Ability", sourceAbility);
        eventData.AddFloat("Value", changeValue);
        eventData.AddInt("Stat", (int)targetStat);
        eventData.AddBool("Removal", isRemoveal);

        EventManager.SendEvent(GameEvent.UnitStatAdjusted, eventData);

        //if(target is EntityPlayer)
        //    Debug.Log("Adjusting a stat: " + targetStat + " Value: " + changeValue + " target: " + target.gameObject.name);

    }


}
