using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class AbilityUtilities 
{

    #region SETUP

    public static void SetupAbilities(List<AbilityData> abilityData, List<Ability> abilities, Entity source) {
        for (int i = 0; i < abilityData.Count; i++) {
            abilities.Add(AbilityFactory.CreateAbility(abilityData[i], source));
        }
    }

    //public static void SetupAbilities(List<AbilityData> abilityData, List<Ability> abilities, DieFace source) {
    //    for (int i = 0; i < abilityData.Count; i++) {
    //        abilities.Add(AbilityFactory.CreateAbility(abilityData[i], source));
    //    }
    //}


    #endregion

    #region GETTERS

    public static Ability GetAbilityByName(string name, List<Ability> abiliites) {
        for (int i = 0; i < abiliites.Count; i++) {
            if (abiliites[i].Data.abilityName == name)
                return abiliites[i];
        }

        return null;
    }

    public static Ability GetAbilityByName(string name, Entity source) {
        return source.GetAbilityByName(name);
    }

    public static Effect GetEffectByName(string name, Ability ability) {
        return ability.GetEffectByName(name);
    }

    public static Tuple<Ability, Effect> GetAbilityAndEffectByName(string abilityName, string EffectName, Entity source) {
        Ability targetAbility = GetAbilityByName(abilityName, source);
        Effect targetEffect = GetEffectByName(EffectName, targetAbility);


        return new Tuple<Ability, Effect>(targetAbility, targetEffect);
    }

    public static List<Entity> GetTargetsFromOtherAbility(string abilityName, string effectName, Entity source) {
        var abilityEffect = GetAbilityAndEffectByName(abilityName, effectName, source);

        if(abilityEffect.Item1 == null) {
            Debug.LogError("An ability: " + abilityEffect + " could not be found on: " + source.EntityName);
            return null;
        }

        if(abilityEffect.Item2 == null) {
            Debug.LogError("An Effect: " + effectName + " could not be found on: " + abilityName + " on the entity: " + source.EntityName);
            return null;
        }

        return abilityEffect.Item2.ValidTargets;

    }

    public static List<Entity> GetEntitiesWithMostStat(StatName stat, List<Entity> validTargets) {
        Dictionary<Entity, float> statDict = GetAllEntitiesWithStat(stat, validTargets);
        float highestValue = statDict.Values.Max();

        return ProcessHighestStats(statDict, highestValue);
    }

    public static List<Entity> GetEntitiesWithLeastStat(StatName stat, List<Entity> validTargets) {
        Dictionary<Entity, float> statDict = GetAllEntitiesWithStat(stat, validTargets);
        float lowestValue = statDict.Values.Min();

        return ProcessLowestStats(statDict, lowestValue);
    }

    public static Dictionary<Entity, float> GetAllEntitiesWithStat(StatName stat, List<Entity> searchTargets) {
        Dictionary<Entity, float> results = new Dictionary<Entity, float>();

        for (int i = 0; i < searchTargets.Count; i++) {
            if (searchTargets[i].Stats.Contains(stat) == true)
                results.Add(searchTargets[i], searchTargets[i].Stats[stat]);
        }

        return results;
    }

    private static List<Entity> ProcessLowestStats(Dictionary<Entity, float> dict, float targetStatValue) {
        List<Entity> results = new List<Entity>();

        foreach (var entry in dict) {
            if (entry.Value <= targetStatValue)
                results.Add(entry.Key);
        }

        return results;
    }

    private static List<Entity> ProcessHighestStats(Dictionary<Entity, float> dict, float targetStatValue) {
        List<Entity> results = new List<Entity>();

        foreach (var entry in dict) {
            if (entry.Value >= targetStatValue)
                results.Add(entry.Key);
        }

        return results;
    }



    #endregion

    #region CLEAN UP

    public static void TearDownAbiliites(List<Ability> abilities) {
        for (int i = 0; i < abilities.Count; i++) {
            abilities[i].TearDown();
        }

        abilities.Clear();
    }

    #endregion



    //public static bool CheckOwner(Entity source, Entity target) {

    //    if(source.Owner == EntityData.Owner.Friendly) {
    //        return target.Owner == source.Owner;
    //    }



    //}


}
