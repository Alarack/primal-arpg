using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    public StatModifier(float value, StatModType modType, StatName targetStat, object source)
    {
        Value = value;
        ModType = modType;
        Source = source;
        TargetStat = targetStat;
    }

    public void UpdateModValue(float updatedValue)
    {
        Value = updatedValue;
    }




}
