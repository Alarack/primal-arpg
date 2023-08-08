using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class BaseStat 
{

    public StatName Name { get; protected set; }
    public Action<BaseStat, object, float> onValueChanged;

    protected Dictionary<object, List<StatModifier>> modDictionary;

    public int ModCount { get { return modDictionary.Count; } }

    public abstract float ModifiedValue { get; }


    public BaseStat (StatName name)
    {
        this.Name = name;
    }

    public BaseStat (BaseStat clone) {
        
        if(clone == null) {
            Debug.LogError("Null clone passed to copy constructor");
            return;
        }
        
        this.Name = clone.Name;
        this.modDictionary = new Dictionary<object, List<StatModifier>>(clone.modDictionary);
    }

    public void CloneMods(SimpleStat clone) {
        
        if(clone.modDictionary != null) {
            foreach (var item in clone.modDictionary) {
                this.modDictionary.Add(item.Key, item.Value);
            }
        }
        
        
    }


    public virtual void RemoveAllModifiersFromSource(object source)
    {

    }


   

}
