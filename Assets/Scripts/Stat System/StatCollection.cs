using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatCollection {

    public object Owner { get; private set; }

    public float this[StatName stat] { get { return GetStatCurrentValue(stat); } }

    private Dictionary<StatName, BaseStat> statDictionary = new Dictionary<StatName, BaseStat>();


    #region EVENTS

    public void AddStatListener(StatName name, Action<BaseStat, object, float> callback) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return;
        }

        BaseStat stat = GetStatByName(name);
        stat.onValueChanged += callback;
    }

    public void RemoveStatListener(StatName name, Action<BaseStat, object, float> callback) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return;
        }

        BaseStat stat = GetStatByName(name);
        stat.onValueChanged -= callback;
    }

    public void AddStatRangeMinListener(StatName name, Action<BaseStat, object, float> callback) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return;
        }

        StatRange stat = GetStat<StatRange>(name);
        stat.MinValueStat.onValueChanged += callback;
    }

    public void AddStatRangeMaxListener(StatName name, Action<BaseStat, object, float> callback) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return;
        }

        StatRange stat = GetStat<StatRange>(name);
        stat.MaxValueStat.onValueChanged += callback;
    }

    public void RemoveStatRangeMinListener(StatName name, Action<BaseStat, object, float> callback) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return;
        }

        StatRange stat = GetStat<StatRange>(name);
        stat.MinValueStat.onValueChanged -= callback;
    }

    public void RemoveStatRangeMaxListener(StatName name, Action<BaseStat, object, float> callback) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return;
        }

        StatRange stat = GetStat<StatRange>(name);
        stat.MaxValueStat.onValueChanged -= callback;
    }

    #endregion


    #region CONSTRUCTION

    public StatCollection(object owner) {
        Owner = owner;
    }

    public StatCollection(object owner, StatDataGroup defaultStats) {
        Owner = owner;

        if(defaultStats == null) {
            //Debug.LogWarning("No stat definitions provided to: " + owner.ToString());
            return;
        }

        int count = defaultStats.dataList.Count;
        for (int i = 0; i < count; i++) {
            CreateStatFromData(defaultStats.dataList[i]);
        }
    }

    public StatCollection(object owner, List<StatData> data) {
        Owner = owner;

        int count = data.Count;
        for (int i = 0; i < count; i++) {
            CreateStatFromData(data[i]);
        }

    }

    public void AddMissingStats(StatCollection stats, List<StatName> exceptions = null, string parent = "", string child = "") {
        foreach (var item in stats.statDictionary) {

            if(exceptions != null) {
                if (exceptions.Contains(item.Key))
                    continue;
            }

            if(Contains(item.Key) == false) {
                SimpleStat missingStat = new SimpleStat(item.Key, item.Value.ModifiedValue);
                AddStat(missingStat);
                
                //if(item.Key == StatName.EffectLifetime) {
                //    Debug.Log(item.Key + " Not found on: " + child + ". Adding it from: " + parent  +". Value: " + item.Value.ModifiedValue);
                //}
             
            }
            else {

                if (stats[item.Key] == 0)
                    continue;

                //if (item.Key == StatName.EffectLifetime) {
                //    Debug.Log(item.Key + " already exists on: " + child + " ["+ GetStatCurrentValue(item.Key) +"]. Setting value from: " + parent +" to: " + stats[item.Key]);
                //}
                
                SetStatValue(item.Key, stats[item.Key], stats.Owner);
            }
        }
    }

    public void AddMissingStats(List<StatData> stats, List<StatName> exceptions = null) {

        for (int i = 0; i < stats.Count; i++) {
            if(exceptions != null) {
                if (exceptions.Contains(stats[i].statName))
                    continue;
            }

            if (Contains(stats[i].statName) == false) {
                CreateStatFromData(stats[i]);
            }
            else {
                SetStatValue(stats[i].statName, stats[i].value, Owner);
            }
        }
    }

    private void CreateStatFromData(StatData data) {
        if (data.variant == StatData.StatVariant.Simple) {
            SimpleStat newSimpleStat = new SimpleStat(data.statName, data.value);
            AddStat(newSimpleStat);
        }

        if (data.variant == StatData.StatVariant.Range) {
            StatRange newRange = new StatRange(data.statName, data.minValue, data.maxValue, data.value);
            AddStat(newRange);
        }

    }



    #endregion

    #region ADD AND REMOVE

    public void AddStat(BaseStat stat) {
        if (Contains(stat.Name) == true) {
            Debug.LogError("Stat Collection already contains a stat of type: " + stat.Name);
            return;
        }

        statDictionary.Add(stat.Name, stat);
    }

    public void RemoveStat(StatName name) {
        if (Contains(name) == true) {
            statDictionary.Remove(name);
        }
    }

    #endregion

    #region GET STAT VALUES

    public TStat GetStat<TStat>(StatName name) where TStat : BaseStat {
        BaseStat result = null;

        if (statDictionary.TryGetValue(name, out result) == true) {
            return result as TStat;
        }

        return null;
    }

    public BaseStat GetStatByName(StatName name) {
        BaseStat result = null;

        statDictionary.TryGetValue(name, out result);

        return result;
    }

    public float GetStatCurrentValue(StatName name) {
        if (Contains(name) == false) {
            //Debug.LogWarning("Stat: " + name + " was not found");
            return 0f;
        }

        BaseStat targetStat = GetStatByName(name);
        return targetStat.ModifiedValue;
    }

    public float GetStatRangeMaxValue(StatName name) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return 0f;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        return targetStat.MaxValueStat.ModifiedValue;
    }

    public float GetStatRangeMinValue(StatName name) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return 0f;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        return targetStat.MaxValueStat.ModifiedValue;
    }

    public float GetStatRangeRatio(StatName name) {
        if (Contains(name) == false) {
            Debug.LogError("Stat: " + name + " was not found");
            return 0f;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        return targetStat.Ratio;
    }


    #endregion

    #region MODIFY STATS

    public void SetStatValue(StatName name, float value, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        SimpleStat targetStat = GetStat<SimpleStat>(name);
        targetStat.SetStatValue(value, source);
    }

    public void EmptyStatRange(StatName name, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        targetStat.Empty(source);
    }

    public void AddModifier(StatName name, StatModifier mod) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        SimpleStat targetStat = GetStat<SimpleStat>(name);
        targetStat.AddModifier(mod);
    }

    public void AddModifier(StatName name, float value, StatModType modType, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        SimpleStat targetStat = GetStat<SimpleStat>(name);
        targetStat.AddModifier(value, modType, source);
    }

    public void RemoveModifier(StatName name, StatModifier mod) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " not found");
            return;
        }

        SimpleStat targetStat = GetStat<SimpleStat>(name);
        targetStat.RemoveModifier(mod);
    }

    public void AddMinValueModifier(StatName name, StatModifier mod) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        targetStat.AddMinModifier(mod);
    }

    public void AddMinValueModifier(StatName name, float value, StatModType modType, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        targetStat.AddMinModifier(value, modType, source);
    }

    public void RemoveMinValueModifier(StatName name, StatModifier mod) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        targetStat.RemoveMinModifier(mod);
    }

    public void AddMaxValueModifier(StatName name, StatModifier mod) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        targetStat.AddMaxModifier(mod);
    }

    public void AddMaxValueModifier(StatName name, float value, StatModType modType, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        targetStat.AddMaxModifier(value, modType, source);
    }

    public void RemoveMaxValueModifier(StatName name, StatModifier mod) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);
        targetStat.RemoveMaxModifier(mod);
    }


    public void AddStatRangeCurrentModifier(StatName name, StatModifier mod) {

    }

    public void RemoveCurrentRangeAdjustment(StatName name, StatModifier mod) {
        AdjustStatRangeCurrentValue(name, -mod.Value, mod.ModType, mod.Source);
    }

    public void AdjustStatRangeCurrentValue(StatName name, float value, StatModType modType, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);

        switch (modType) {
            case StatModType.Flat:
                targetStat.AdjustValueFlat(value, source);
                break;
            case StatModType.PercentAdd:
            case StatModType.PercentMult:
                targetStat.AdjustValuePercentage(value, source);
                break;
        }

    }

    public void AdjustStatRangeByPercentOfMaxValue(StatName name, float value, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);

        targetStat.AdjustValueByPercentOfMax(value, source);
    }

    public void AdjustStatRangeCurrentValue(StatName name, StatModifier mod) {
        AdjustStatRangeCurrentValue(name, mod.Value, mod.ModType, mod.Source);
    }

    public void RemoveAllModifiersFromSource(StatName name, object source) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        BaseStat targetStat = GetStatByName(name);
        targetStat.RemoveAllModifiersFromSource(source);
    }

    public void RemoveAllModifiersFromSource(object source) {
        foreach (var stat in statDictionary) {
            stat.Value.RemoveAllModifiersFromSource(source);
        }
    }


    public void Refresh(StatName name) {
        if (Contains(name) == false) {
            Debug.LogWarning("Stat: " + name + " was not found");
            return;
        }

        StatRange targetStat = GetStat<StatRange>(name);

        targetStat.Refresh(Owner);


    }





    #endregion

    #region HELPERS

    public bool Contains(StatName name) {
        return statDictionary.ContainsKey(name);
    }


    #endregion

}
