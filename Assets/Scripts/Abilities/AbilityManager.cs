using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Playables;

public class AbilityManager : MonoBehaviour
{

    public List<AbilityDefinition> preloadedAbilities = new List<AbilityDefinition>();


    public List<Ability> this[AbilityCategory category] { get { return GetAbilitiesByCategory(category); } }

    public List<Ability> ActiveAbilities { get { return this[AbilityCategory.ActiveSkill]; } }
    public List<Ability> KnownAbilities { get { return this[AbilityCategory.KnownSkill]; } }
    public List<Ability> RuneAbilities { get { return this[AbilityCategory.Rune]; } }

    public Dictionary<AbilityCategory, List<Ability>> Abilities { get; private set; } = new Dictionary<AbilityCategory, List<Ability>>();

    public Action<Ability, int> onAbilityEquipped;
    public Action<Ability, int> onAbilityUnequipped;
    public Action<Ability> onAbilityLearned;
    public Action<Ability> onAbilityUnlearned;
    public Action<Ability, int, Ability, int> onAbilitySwapped;


    public Entity Owner { get; private set; }

    private void Awake() {
        Owner = GetComponent<Entity>();
        SetupAbilityDict();
        SetupPreloadedAbilities();
    }

    private void SetupAbilityDict() {

        AbilityCategory[] categories = Enum.GetValues(typeof(AbilityCategory)) as AbilityCategory[];
        foreach (AbilityCategory category in categories) {
            Abilities.Add(category, new List<Ability>());
        }
    }

    public List<Ability> GetAbilitiesByCategory(AbilityCategory category) {
        if (Abilities.TryGetValue(category, out List<Ability> results) == true)
            return results;

        return null;
    }

    //private void Setup(Entity owner) {
    //    Owner = owner;
    //    SetupPreloadedAbilities();
    //}

    private void SetupPreloadedAbilities() {
        AbilityUtilities.SetupAbilities(preloadedAbilities, KnownAbilities, Owner);
    }


    #region LEARNING AND EQUIPPING

    public void LearnAbility(Ability ability) {

        if(KnownAbilities.AddUnique(ability) == true) {
            onAbilityLearned?.Invoke(ability);
        }
    }

    public void UnlearnAbility(Ability ability) {
        if (KnownAbilities.RemoveIfContains(ability) == true) {
            onAbilityUnlearned?.Invoke(ability);
        }
    }

    public void EquipAbility(Ability ability, int index) {
        if (KnownAbilities.Contains(ability)) {
            if (ActiveAbilities.AddUnique(ability) == true) {
                ability.Equip();
                onAbilityEquipped?.Invoke(ability, index);
            }
            else
                Debug.LogError(ability.Data.abilityName + " is already equipped.");
        }
        else
            Debug.LogError("Tried to equip a skill you didn't know");
    }

    public void UnequipAbility(Ability ability, int index) {
        if (ActiveAbilities.RemoveIfContains(ability) == true) {
            ability.Uneqeuip();
            onAbilityUnequipped?.Invoke(ability, index);
        }
        else
            Debug.LogError(ability.Data.abilityName + " is not equipped, so we can't unequip it");
    }

    public void MoveAbilitySlot(Ability ability, int fromIndex, int toIndex) {
        UnequipAbility(ability, fromIndex);
        EquipAbility(ability, toIndex);
        
        //onAbilityUnequipped?.Invoke(ability, fromIndex);
        //onAbilityEquipped?.Invoke(ability, toIndex);
    }

    public void SwapEquippedAbilities(Ability firstAbility, int firstAbilityIndex, Ability secondAbility, int secondAbilityIndex) {
        onAbilitySwapped?.Invoke(firstAbility, firstAbilityIndex, secondAbility, secondAbilityIndex);
    }

    #endregion

    #region GETTING AND ACTIVATION

    public Ability GetAbilityByName(string name, AbilityCategory category) {
        
        if(category == AbilityCategory.Any) {
            foreach (var entry in Abilities) {
                List<Ability> abilities = entry.Value;
                for (int i = 0; i < abilities.Count; i++) {
                    if (abilities[i].Data.abilityName == name) {
                        return abilities[i];
                    }
                }
            }
        }


        for (int i = 0; i < this[category].Count; i++) {
            if (this[category][i].Data.abilityName == name)
                return this[category][i];
        }

        return null;
    }

    public List<Ability> GetRuneAbilities(string abilityName) {
        List<Ability> results = new List<Ability>();    
        
        for (int i = 0; i < RuneAbilities.Count; i++) {
            if (RuneAbilities[i].Data.runeAbilityTarget == abilityName)
                results.Add(RuneAbilities[i]);
        }

        return results;
    }

    public void ActivateFirstAbility() {
        if (KnownAbilities.Count == 0) {
            Debug.LogError(Owner.EntityName + " has no abiliites and was told to force active an ability");
            return;
        }

        KnownAbilities[0].ForceActivate();
    }

    public void ActivateAbilityByName(string name, AbilityCategory category) {
        Ability targetAbility = GetAbilityByName(name, category);
        if (targetAbility == null) {
            Debug.LogError("An abiity: " + name + " could not be found on: " + Owner.EntityName + " and it was told to activate");
            return;
        }

        targetAbility.ForceActivate();
    }

    #endregion

}
