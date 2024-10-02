using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StatVariantTarget = StatModifierData.StatVariantTarget;


public enum StatModType
{
    Flat,
    PercentAdd,
    PercentMult
}


public class StatModifier
{

    public float Value { get; private set; }
    public StatModType ModType { get; private set; }
    public object Source { get; private set; }
    public StatName TargetStat { get; private set; }

    public StatVariantTarget VariantTarget { get; private set; }

    

    public StatModifier(float value, StatModType modType, StatName targetStat, object source, StatVariantTarget variantTarget, List<StatModifierData.StatusModifier> statusModifiers = null)
    {
        Value = value;
        ModType = modType;
        Source = source;
        TargetStat = targetStat;
        VariantTarget = variantTarget;
    }

    public StatModifier(StatModifierData data, object source) {
        Value = data.value;
        ModType = data.modifierType;
        Source = source;
        TargetStat = data.targetStat;
        VariantTarget = data.variantTarget;
    }

    public void UpdateModValue(float updatedValue)
    {
        Value = updatedValue;
    }




}
