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
