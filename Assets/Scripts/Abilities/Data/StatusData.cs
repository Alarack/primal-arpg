using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StackMethod = Status.StackMethod;
using StatusName = Status.StatusName;

[System.Serializable]
public class StatusData 
{
    public StatusName statusName;
    public StackMethod stackMethod;
    public int maxStacks;
    public int initialStackCount = 1;
    
    public float duration;
    public float interval;

    public GameObject VFXPrefab;

    public EffectDefinition statusEffectDef;

    //Stat Adjustment
    //public List<StatModifierData> statModifiers = new List<StatModifierData>();
    //public bool multiplyByStackCount;
    
}
