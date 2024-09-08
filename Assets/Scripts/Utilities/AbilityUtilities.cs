using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using SkillEntryLocation = SkillEntry.SkillEntryLocation;
using GameButtonType = InputHelper.GameButtonType;

public static class AbilityUtilities {

    #region SETUP

    public static void SetupAbilities(List<AbilityData> abilityData, List<Ability> abilities, Entity source) {
        for (int i = 0; i < abilityData.Count; i++) {
            abilities.Add(AbilityFactory.CreateAbility(abilityData[i], source));
        }
    }

    public static void SetupAbilities(List<AbilityDefinition> abilityData, List<Ability> abilities, Entity source, bool autoEquip = false, bool registerWithPlayer = false) {
        for (int i = 0; i < abilityData.Count; i++) {

            //Debug.Log("Setting up: " + abilityData[i].AbilityData.abilityName);

            Ability newAbility = AbilityFactory.CreateAbility(abilityData[i].AbilityData, source);
            abilities.Add(newAbility);

            if (source != null && registerWithPlayer == true && newAbility.Data.category != AbilityCategory.Rune) {

                if (source.AbilityManager.AbilitiesByName.ContainsKey(newAbility.Data.abilityName) == false) {
                    source.AbilityManager.AbilitiesByName.Add(newAbility.Data.abilityName, newAbility);

                }
                else {
                    Debug.LogWarning("An ability: " + newAbility.Data.abilityName + " has already been added to " + source.EntityName + "'s Abilities By Name dict"); ;
                }
            }

            if (autoEquip == true) {
                newAbility.Equip();
            }
        }
    }

    //public static void SetupAbilities(List<AbilityData> abilityData, List<Ability> abilities, DieFace source) {
    //    for (int i = 0; i < abilityData.Count; i++) {
    //        abilities.Add(AbilityFactory.CreateAbility(abilityData[i], source));
    //    }
    //}


    #endregion

    #region UI

    public static void CreateEmptySkillEntries(ref List<SkillEntry> list, int count, SkillEntry prefab, Transform holder, SkillEntryLocation location, List<GameButtonType> defaultBinds) {
        list.PopulateList(count, prefab, holder, true);

        for (int i = 0; i < list.Count; i++) {
            list[i].Setup(null, location, false, defaultBinds[i], i);
        }
    }

    public static void CreateEmptyPassiveSkillEntries(ref List<SkillEntry> list, int count, SkillEntry prefab, Transform holder) {
        list.PopulateList(count, prefab, holder, true);
        for (int i = 0; i < list.Count; i++) {
            list[i].Setup(null, SkillEntryLocation.ActivePassive, true);
        }
    }

    public static SkillEntry CreateSkillEntry(Ability ability, SkillEntry prefab, Transform holder, SkillEntryLocation location, GameButtonType keyBind = GameButtonType.None, int index = -1) {
        SkillEntry entry = GameObject.Instantiate(prefab, holder);
        entry.gameObject.SetActive(true);
        entry.Setup(ability, location, false, keyBind, index);

        return entry;
    }

    public static SkillEntry CreatePassiveSkillEntry(Ability ability, SkillEntry prefab, Transform holder, SkillEntryLocation location) {
        SkillEntry entry = GameObject.Instantiate(prefab, holder);
        entry.gameObject.SetActive(true);
        entry.Setup(ability, location, true);

        return entry;
    }

    public static void PopulateSkillEntryList(ref List<SkillEntry> list, SkillEntry prefab, Transform holder, SkillEntryLocation location) {

        list.ClearList();

        List<Ability> abilities = location switch {
            SkillEntryLocation.ActiveSkill => EntityManager.ActivePlayer.AbilityManager.ActiveAbilities,
            SkillEntryLocation.KnownSkill => EntityManager.ActivePlayer.AbilityManager.KnownAbilities,
            SkillEntryLocation.Hotbar => EntityManager.ActivePlayer.AbilityManager.ActiveAbilities,
            SkillEntryLocation.KnownPassive => EntityManager.ActivePlayer.AbilityManager.PassiveAbilities,
            SkillEntryLocation.ClassFeatureSkill => EntityManager.ActivePlayer.AbilityManager.ClassFeatures,
            _ => new List<Ability>(),
        };



        if (location == SkillEntryLocation.KnownPassive || location == SkillEntryLocation.ActivePassive) {
            for (int i = 0; i < abilities.Count; i++) {
                //if (abilities[i].Tags.Contains(AbilityTag.ClassFeature) == true)
                //    return;
                if (abilities[i].Locked == true)
                    continue;

                if (abilities[i].Tags.Contains(AbilityTag.Mastery) == true)
                    continue;


                list.Add(CreatePassiveSkillEntry(abilities[i], prefab, holder, location));
            }
        }
        else {
            for (int i = 0; i < abilities.Count; i++) {

                if (abilities[i].Locked == true)
                    continue;

                list.Add(CreateSkillEntry(abilities[i], prefab, holder, location));
            }
        }




    }

    public static SkillEntry GetSkillEntryByAbility(List<SkillEntry> list, Ability ability) {
        for (int i = 0; i < list.Count; i++) {
            if (list[i].Ability == ability)
                return list[i];
        }

        return null;
    }

    #endregion

    #region GETTERS

    public static Ability GetAbilityByName(string name, List<Ability> abiliites) {
        for (int i = 0; i < abiliites.Count; i++) {
            if (abiliites[i].Data.abilityName == name)
                return abiliites[i];
        }

        return null;
    }

    public static Ability GetAbilityByName(string name, Entity source, AbilityCategory category) {
        return source.GetAbilityByName(name, category);
    }

    public static Ability GetAbilityByName(string name, Entity source) {

        if (source.AbilityManager.AbilitiesByName.ContainsKey(name) == false) {
            Debug.LogError("Ability: " + name + " not found in dict. Source: " + source.EntityName);
            return null;
        }

        return source.AbilityManager.AbilitiesByName[name];
    }

    public static Effect GetEffectByName(string name, Ability ability) {
        return ability.GetEffectByName(name);
    }

    public static Effect GetEffectByName(string abilityName, string effectName, Entity source, AbilityCategory category) {
        Tuple<Ability, Effect> tuple = GetAbilityAndEffectByName(abilityName, effectName, source, category);

        return tuple.Item2;
    }

    public static Tuple<Ability, Effect> GetAbilityAndEffectByName(string abilityName, string effectName, Entity source, AbilityCategory category) {
        Ability targetAbility = GetAbilityByName(abilityName, source);//GetAbilityByName(abilityName, source, category);


        Effect targetEffect = GetEffectByName(effectName, targetAbility);

        Tuple<Ability, Effect> target = new Tuple<Ability, Effect>(targetAbility, targetEffect);

        if (target.Item1 == null) {
            Debug.LogError("Could not find: " + abilityName + " on " + source.EntityName);
        }

        if (target.Item2 == null) {
            Debug.LogError("Could not find: " + effectName + " on " + target.Item1.Data.abilityName);
        }

        return target;
    }

    public static List<Entity> GetTargetsFromOtherAbility(string abilityName, string effectName, Entity source, AbilityCategory category) {
        var abilityEffect = GetAbilityAndEffectByName(abilityName, effectName, source, category);

        if (abilityEffect.Item1 == null) {
            Debug.LogError("An ability: " + abilityEffect + " could not be found on: " + source.EntityName);
            return null;
        }

        if (abilityEffect.Item2 == null) {
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

    #region CONVERTERS

    public static List<StatName> ConvertTagsToStats(Ability ability) {
        List<StatName> results = new List<StatName>();

        for (int i = 0; i < ability.Tags.Count; i++) {
            StatName stat = ability.Tags[i] switch {
                //AbilityTag.Fire => throw new NotImplementedException(),
                //AbilityTag.Poison => throw new NotImplementedException(),
                //AbilityTag.Melee => throw new NotImplementedException(),
                //AbilityTag.Force => throw new NotImplementedException(),
                //AbilityTag.Physical => throw new NotImplementedException(),
                //AbilityTag.Water => throw new NotImplementedException(),
                //AbilityTag.Space => throw new NotImplementedException(),
                //AbilityTag.Air => throw new NotImplementedException(),
                AbilityTag.Arcane => StatName.VulnerableArcane,
                AbilityTag.Void => StatName.VulnerableVoid,
                AbilityTag.Time => StatName.VulnerableTime,
                AbilityTag.Space => StatName.VulnerableSpace,
                _ => StatName.Armor,
            };

            if (stat != StatName.Armor)
                results.Add(stat);

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
