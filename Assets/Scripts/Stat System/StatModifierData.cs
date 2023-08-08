using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifierData
{

    public enum StatVariantTarget {
        Simple,
        RangeCurrent,
        RangeMin,
        RangeMax,
        RangeCurrentAdj
    }

    public enum ModValueSetMethod {
        Manual,
        DeriveFromOtherStats,
        DeriveFromNumberOfTargets,
        HardSetValue,
        HardReset,
        DeriveFromWeaponDamage,
        DerivedFromMultipleSources,
        AddScaler,
        RemoveScaler,
        ModifyScaler
    }
    public enum DeriveFromWhom {
        Source,
        Cause,
        Trigger,
        CurrentEntityTarget,
        CurrentEffectTarget,
        CurrentAbilityTarget,
        OtherEntityTarget,
        OtherEffect,
        OtherAbility,
        SourceAbility,
        SourceEffect,
        TriggerAbility,
        TriggerEffect,
        CauseAbility,
        CauseEffect,
        WeaponDamage
    }

    public enum StatModDesignation {
        None,
        PrimaryDamage,
        SecondaryDamage,
        ShotCount

    }


    public StatName targetStat;
    public float value;
    public StatModType modifierType;
    public StatVariantTarget variantTarget;

    //public StatModDesignation modDesignation;

    public ModValueSetMethod modValueSetMethod;
    public float weaponDamagePercent = 1f;
    public float deriveStatMultiplier = 1f;
    public DeriveFromWhom deriveTarget;
    public StatName derivedTargetStat;
    public bool invertDerivedValue;

    public string otherTargetAbility;
    public string otherTagetEffect;
   
    public List<StatScaler> scalers = new List<StatScaler>();

    public Dictionary<StatName, StatScaler> scalersDict = new Dictionary<StatName, StatScaler>();

    //public string deriveEffectName;
    //public string deriveAbilityName;

    public StatCollection Stats { get; private set; }


    public StatModifierData() {
       
    }

    public void SetupStats() {
        Stats = new StatCollection(this);
        SimpleStat modValueStat = new SimpleStat(StatName.StatModifierValue, value);
        Stats.AddStat(modValueStat);

        SetupScalers();
     
    }

    public void SetupScalers() {
        for (int i = 0; i < scalers.Count; i++) {
            scalers[i].InitStat();
            
            if (scalersDict.ContainsKey(scalers[i].targetStat) == false)
                scalersDict.Add(scalers[i].targetStat, scalers[i]);
        }
    }

    public void SetupEffectStats() {
        SetupStats();
        SimpleStat weaponCoeeficientStat = new SimpleStat(StatName.AbilityWeaponCoefficicent, weaponDamagePercent);
        Stats.AddStat(weaponCoeeficientStat);
    }

    public void CloneEffectStats(StatModifierData clone) {
        Stats = new StatCollection(this);
        
        SimpleStat modValueStat = new SimpleStat(StatName.StatModifierValue, clone.Stats[StatName.StatModifierValue]);
        SimpleStat weaponCoeeficientStat = new SimpleStat(StatName.AbilityWeaponCoefficicent, clone.Stats[StatName.AbilityWeaponCoefficicent]);
        Stats.AddStat(modValueStat);
        Stats.AddStat(weaponCoeeficientStat);


    }


    public void AddScaler(StatScaler scaler) {
        if(scalersDict.ContainsKey(scaler.targetStat) == true) {
            Debug.LogError("Duplicate stat scaler: " + scaler.targetStat + ". this is not supported");
            return;
        }

        scalersDict.Add(scaler.targetStat, scaler);
        scaler.InitStat();
    }

    public void RemoveScaler(StatScaler scaler) {
        if (scalersDict.ContainsKey(scaler.targetStat) == true) {
            scalersDict.Remove(scaler.targetStat);
        }
    }

    public void AddScalerMod(StatName targetStat, StatModifier mod) {

        //Debug.Log("Adding a scaler mod to: " + targetStat.ToString() + " of " + mod.Value);

        scalersDict[targetStat].AddScalerMod(mod);


        //Debug.Log(scalersDict[targetStat].scalerStat.ModCount + " mods found");
        //for (int i = 0; i < scalers.Count; i++) {
        //    if (scalers[i].targetStat == targetStat) {
        //        scalers[i].AddScalerMod(mod);
        //        break;
        //    }
        //}
    }

    public void RemoveScalerMod(StatName targetStat, StatModifier mod) {

        scalersDict[targetStat].RemoveScalerMod(mod);
        
        //for (int i = 0; i < scalers.Count; i++) {
        //    if (scalers[i].targetStat == targetStat) {
        //        scalers[i].RemoveScalerMod(mod);
        //        break;
        //    }
        //}
    }

    public void RemoveAllscalerModsFromSource(StatName targetStat, object source) {

        scalersDict[targetStat].RemoveAllScalarModsFromSource(source);

        //for (int i = 0; i < scalers.Count; i++) {
        //    if (scalers[i].targetStat == targetStat) {
        //        scalers[i].RemoveAllScalarModsFromSource(source);
        //        break;
        //    }

        //}
    }

    //public float GetScalerValue(StatName targetStat) {
    //    for (int i = 0; i < scalers.Count; i++) {
    //        if (scalers[i].targetStat == targetStat)
    //            return scalers[i].scalerStat.ModifiedValue;
    //    }

    //    return 0f;
    //}

    public Dictionary<StatName, float> GetAllScalerValues() {
        Dictionary<StatName, float> results = new Dictionary<StatName, float>();

        foreach (var entry in scalersDict) {
            results.Add(entry.Key, entry.Value.scalerStat.ModifiedValue);
        }

        return results;
    }

    public float GetWeaponScaler() {
        if(scalersDict.TryGetValue(StatName.AbilityWeaponCoefficicent, out StatScaler target) == true) {
            return target.scalerStat.ModifiedValue;
        }

        return -1f;
    }

    //public StatScaler GetScalerByStat(StatName targetStat) {
    //    for (int i = 0; i < scalers.Count; i++) {
    //        if (scalers[i].targetStat == targetStat)
    //            return scalers[i];
    //    }

    //    return null;
    //}

    public StatModifierData(StatModifierData copy) {
        this.targetStat = copy.targetStat;
        this.value = copy.value;
        this.modifierType = copy.modifierType;
        this.variantTarget = copy.variantTarget;
        this.modValueSetMethod = copy.modValueSetMethod;
        this.deriveTarget = copy.deriveTarget;
        this.derivedTargetStat = copy.derivedTargetStat;
        this.invertDerivedValue = copy.invertDerivedValue;
        this.otherTagetEffect = copy.otherTagetEffect;
        this.otherTargetAbility = copy.otherTargetAbility;
        this.weaponDamagePercent = copy.weaponDamagePercent;
        CloneStatScalers(copy);
    }

    private void CloneStatScalers(StatModifierData copy) {
        for (int i = 0; i < copy.scalers.Count; i++) {
            this.scalers.Add(new StatScaler(copy.scalers[i]));
        }

        foreach (var entry in copy.scalersDict) {
            this.scalersDict.Add(entry.Key, new StatScaler(entry.Value));
        }

    }
}


//[System.Serializable]
//public class StatAdjustmnetInfo {
//    public StatModifierData data;
//    public StatModifier mod;

//    public StatModifierData.StatModDesignation Designation { get { return data.modDesignation; } }

//    public StatAdjustmnetInfo(StatModifierData data, StatModifier mod) {
//        this.data = data;
//        this.mod = mod;
//    }
//}
