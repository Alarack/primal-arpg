using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum StatName
{
    Health,
    Vitality,
    MoveSpeed,
    RotationSpeed,
    BaseDamage,
    Stamina,
    Mana,
    Money,
    DetectionRange,
    Knockback,
    ProjectileLifetime,
    EffectLifetime,
    EffectIntensity_Percent,
    Cooldown,
    ShotCount,
    FireDelay,
    Accuracy,
    DashSpeed,
    DashDuration,
    EffectInterval,
    AbilityCharge,
    CooldownReduction,
    GlobalDamageModifier,
    GlobalEffectDurationModifier,
    MeleeDamageModifier,
    OverloadChance,
    OverloadDamageModifier,
    StatModifierValue,
    AbilityWeaponCoefficicent,
    ProjectilePierceCount,
    AbilityRuneSlots,
    StackCount,
    GlobalEffectIntervalModifier,
    DashCooldown,
    ProjectileChainCount,
    ProjectileSplitCount,
    ProjectileSplitQuantity,
    ProjectileEffectContrabution,
    EffectMaxTargets
}


public class SimpleStat : BaseStat
{

    public float BaseValue { get; protected set; }
    public override float ModifiedValue { get { return GetModifiedValue(); } }

    protected bool isDirty;
    protected float lastModifiedValue;

    public SimpleStat (StatName name, float baseValue) : base (name)
    {
        BaseValue = baseValue;
        isDirty = true;

        modDictionary = new Dictionary<object, List<StatModifier>>();
    }

    protected virtual float GetModifiedValue(bool setDirty = false)
    {
        if(isDirty == false)
        {
            return lastModifiedValue;
        }

        float result = BaseValue;

        result += GetTotalFlatModifier();
        result *= GetTotalPercentAdditive();
        result *= GetTotalPercentMultiplicative();
        //result += staticFlatModifier;

        lastModifiedValue = result;
        isDirty = setDirty;

        return MathF.Round(result, 2);
    }


    #region MODIFIERS

    public virtual void SetStatValue(float value, object source) {
        float difference = value - ModifiedValue;

        modDictionary.Clear();

        BaseValue = value;

        isDirty = true;

        onValueChanged?.Invoke(this, source, difference);

    }

    public virtual void AddModifier(StatModifier mod)
    {
        //TODO: Force sources to not be null
        List<StatModifier> existingMods;

        if(modDictionary.TryGetValue(mod.Source, out existingMods) == true)
        {
            existingMods.Add(mod);
        }
        else
        {
            List<StatModifier> newModList = new List<StatModifier>();
            newModList.Add(mod);

            modDictionary.Add(mod.Source, newModList);
        }

        isDirty = true;

        onValueChanged?.Invoke(this, mod.Source, mod.Value);
    }

    public virtual void AddModifier(float value, StatModType modType, object source)
    {
        //TODO: Force sources to not be null
        StatModifier newMod = new StatModifier(value, modType, Name, source, StatModifierData.StatVariantTarget.Simple);
        AddModifier(newMod);
    }

    public virtual void RemoveModifier(StatModifier mod) 
    {

        List<StatModifier> existingMods;

        if (modDictionary.TryGetValue(mod.Source, out existingMods) == true)
        {
            int count = existingMods.Count;
            for (int i = 0; i < count; i++)
            {
                if (existingMods[i] == mod)
                {
                    existingMods.Remove(mod);
                    isDirty = true;
                    onValueChanged?.Invoke(this, mod.Source, mod.Value);
                    break;
                }
            }
        }
        else
        {
            foreach (List<StatModifier> entry in modDictionary.Values)
            {
                int count = entry.Count;
                for (int i = 0; i < count; i++)
                {
                    if (entry[i] == mod)
                    {
                        entry.Remove(mod);
                        isDirty = true;
                        onValueChanged?.Invoke(this, mod.Source, mod.Value);
                        break;
                    }
                }
            }
        }
    }

    public override void RemoveAllModifiersFromSource(object source)
    {
        if(modDictionary.ContainsKey(source) == true)
        {
            modDictionary.Remove(source);
            isDirty = true;

            onValueChanged?.Invoke(this, source, 0f);
        }
    }

    public virtual void HardReset()
    {
        modDictionary.Clear();
        isDirty = true;

        onValueChanged(this, null, 0f);
    }


    #endregion




    #region GET MODIFIER METHODS

    protected float GetTotalFlatModifier()
    {
        float total = 0f;
        List<StatModifier> flatMods = GetAllModifiersOfType(StatModType.Flat);

        int count = flatMods.Count;
        for (int i = 0; i < count; i++)
        {
            total += flatMods[i].Value;
        }

        return total;
    }

    protected float GetTotalPercentAdditive()
    {
        float total = 1f;
        List<StatModifier> additiveMods = GetAllModifiersOfType(StatModType.PercentAdd);

        int count = additiveMods.Count;
        for (int i = 0; i < count; i++)
        {
            total += additiveMods[i].Value;
        }

        return total;
    }

    protected float GetTotalPercentMultiplicative()
    {
        float total = 1f;
        List<StatModifier> multiplicativeMods = GetAllModifiersOfType(StatModType.PercentMult);

        int count = multiplicativeMods.Count;
        for (int i = 0; i < count; i++)
        {
            float currentModValue = multiplicativeMods[i].Value + 1f;

            if (currentModValue < 0f)
                currentModValue = 0f;

            total *= currentModValue;
        }

        return total;
    }


    protected List<StatModifier> GetAllModifiersOfType(StatModType type)
    {
        List<StatModifier> results = new List<StatModifier>();

        foreach (KeyValuePair<object, List<StatModifier>> entry in modDictionary)
        {
            int count = entry.Value.Count;
            for (int i = 0; i < count; i++)
            {
                if(entry.Value[i].ModType == type)
                {
                    results.Add(entry.Value[i]);
                }
            }
        }

        return results;
    }


    #endregion


}
