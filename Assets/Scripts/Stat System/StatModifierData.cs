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
        DeriveFromWeaponDamage
    }
    public enum DeriveFromWhom {
        Source,
        Cause,
        Trigger,
        OtherTarget,
        CurrentTarget,
        SourceCharacter
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
    public DeriveFromWhom deriveTarget;
    public StatName derivedTargetStat;
    public bool invertDerivedValue;

    public string otherTargetAbility;
    public string otherTagetEffect;

    public StatCollection Stats { get; private set; }


    public StatModifierData() {
       
    }

    public void SetupStats() {
        Stats = new StatCollection(this);
        SimpleStat modValueStat = new SimpleStat(StatName.StatModifierValue, value);
        Stats.AddStat(modValueStat);
     
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
