using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputHelper;

public class SkillBasePanel : BasePanel
{

    [Header("Template")]
    public Transform holder;
    public SkillEntry skillEntryTemplate;

    protected List<SkillEntry> activeSkillEntries = new List<SkillEntry>();

    [Header("Default KeyBinds")]
    public List<GameButtonType> defaultKeybinds = new List<GameButtonType>();


    protected override void Awake() {
        base.Awake();
        skillEntryTemplate.gameObject.SetActive(false);
    }

    protected override void Start() {
        base.Start();

        CreateEmptySlots();
    }

    protected override void OnEnable() {
        base.OnEnable();

        if(EntityManager.ActivePlayer == null) {
            Debug.LogError("No player exists when opening skill panel");
            return;
        }

        EntityManager.ActivePlayer.AbilityManager.onAbilityEquipped += OnAbilityEquipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilityUnequipped += OnAbilityUnequipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilitySwapped += OnAbilitySwapped;
    }


    protected override void OnDisable() {
        base.OnDisable();

        EntityManager.ActivePlayer.AbilityManager.onAbilityEquipped -= OnAbilityEquipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilityUnequipped -= OnAbilityUnequipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilitySwapped -= OnAbilitySwapped;
    }

    protected virtual void CreateEmptySlots() {
        AbilityUtilities.CreateEmptySkillEntries(ref activeSkillEntries, 6, skillEntryTemplate, holder, SkillEntry.SkillEntryLocation.Hotbar, defaultKeybinds);
    }

    public Ability GetActiveAbilityBySlot(int index) {
        
        if(index >= activeSkillEntries.Count) {
            Debug.LogError("Index out of range. Tried to get the " + index + "item, but there are only " + activeSkillEntries.Count);
            return null;
        }
        
        return activeSkillEntries[index].Ability;
    }

    public int GetAbilitySlotIndex(Ability ability) {

        //Debug.Log("Checking: " + ability.Data.abilityName);
        
        for (int i = 0; i < activeSkillEntries.Count; i++) {

            //if (activeSkillEntries[i].Ability != null)
            //    Debug.Log("Found: " + activeSkillEntries[i].Ability.Data.abilityName);
            
            if (activeSkillEntries[i].Ability == ability) {
                //Debug.Log("Match!");
                
                return i;
            }
        }

        Debug.LogError("Could not find: " + ability.Data.abilityName + " in acitve slots");

        return -1;
    }

    #region EVENTS

    protected virtual void OnAbilityEquipped(Ability ability, int index) {
        activeSkillEntries[index].AssignNewAbility(ability);
    }

    protected virtual void OnAbilityUnequipped(Ability ability, int index) {
        activeSkillEntries[index].AssignNewAbility(null);
    }

    protected virtual void OnAbilitySwapped(Ability first, int firstIndex, Ability second, int secondIndex) {
        //activeSkillEntries[firstIndex].AssignNewAbility(null);
        //activeSkillEntries[secondIndex].AssignNewAbility(null);
        activeSkillEntries[firstIndex].AssignNewAbility(second);
        activeSkillEntries[secondIndex].AssignNewAbility(first);
    }
    #endregion
}
