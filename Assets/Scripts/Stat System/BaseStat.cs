using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class BaseStat 
{

    public StatName Name { get; protected set; }
    public Action<BaseStat, object, float> onValueChanged;

    protected Dictionary<object, List<StatModifier>> modDictionary;
    public abstract float ModifiedValue { get; }


    public BaseStat (StatName name)
    {
        this.Name = name;
    }


    public virtual void RemoveAllModifiersFromSource(object source)
    {

    }


   

}
