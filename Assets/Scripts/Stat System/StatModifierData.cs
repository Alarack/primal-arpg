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
        RangeMax
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


    public StatName targetStat;
    public float value;
    public StatModType modifierType;
    public StatVariantTarget variantTarget;

    public ModValueSetMethod modValueSetMethod;
    public float weaponDamagePercent = 1f;
    public DeriveFromWhom deriveTarget;
    public StatName derivedTargetStat;
    public bool invertDerivedValue;

    public string otherTargetAbility;
    public string otherTagetEffect;


    public StatModifierData() {

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

    }
}
