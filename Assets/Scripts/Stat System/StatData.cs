using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatData 
{
    public enum StatVariant
    {
        Simple,
        Range
    }


    public StatVariant variant;
    public StatName statName;
    public float value;

    public float maxValue;
    public float minValue;

    public StatData() {

    }

}
