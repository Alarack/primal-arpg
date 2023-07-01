using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public class AbilityManager : MonoBehaviour
{

    public List<AbilityDefinition> preloadedAbilities = new List<AbilityDefinition>();

    public List<Ability> ActiveAbilities { get; private set; } = new List<Ability>();
    public List<Ability> KnownAbilities { get; private set; } = new List<Ability>();


    public Action<Ability, int> onAbilityEquipped;
    public Action<Ability, int> onAbilityUnequipped;
    public Action<Ability> onAbilityLearned;
    public Action<Ability> onAbilityUnlearned;
    public Action<Ability, int, Ability, int> onAbilitySwapped;


    public Entity Owner { get; private set; }

    private void Awake() {
        Owner = GetComponent<Entity>();
        SetupPreloadedAbilities();
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



}
