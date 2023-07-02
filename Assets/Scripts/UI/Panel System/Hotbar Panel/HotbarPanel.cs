using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GameButtonType = InputHelper.GameButtonType;

public class HotbarPanel : BasePanel
{
    [Header("Template")]
    public Transform holder;
    public SkillEntry skillEntryTemplate;

    private List<SkillEntry> activeSkillEntries = new List<SkillEntry>();

    [Header("Default KeyBinds")]
    public List<GameButtonType> defaultKeybinds = new List<GameButtonType>();

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

    private void CreateEmptySlots() {
        AbilityUtilities.CreateEmptySkillEntries(ref activeSkillEntries, 6, skillEntryTemplate, holder, SkillEntry.SkillEntryLocation.Hotbar, defaultKeybinds);
    }

    private Ability GetAbilityBykeyBind(GameButtonType button) {
        for (int i = 0; i < activeSkillEntries.Count; i++) {
            if (activeSkillEntries[i].keybind == button) {
                return activeSkillEntries[i].Ability;
            }
        }

        return null;

    }

    private void OnGUI() {
        //if (binding == true) {

        //    Event e = Event.current;

        //    if(e.isKey == true) {
        //        Debug.Log("Pressed: " + e.keyCode);
        //    }

        //    if(e.isMouse == true) {
        //        Debug.Log("Clicked: " + e.button);
        //    }


        //}

        if (InputHelper.initBinds == false) {
            InputHelper.InitDefaultBinds();
        }

        Event e = Event.current;

        GameButtonType currentButton = InputHelper.GetCustomInput(e);

        if (currentButton != InputHelper.GameButtonType.None)
            OnSkillBindPressed(currentButton);


    }



    #region EVENTS

    private void OnSkillBindPressed(GameButtonType button) {
        Ability ability = GetAbilityBykeyBind(button);

        if(ability != null) {
            EventData eventData = new EventData();
            eventData.AddAbility("Ability", ability);

            EventManager.SendEvent(GameEvent.UserActivatedAbility, eventData);
        }
    }


    private void OnAbilityEquipped(Ability ability, int index) {
        activeSkillEntries[index].AssignNewAbility(ability);
    }

    private void OnAbilityUnequipped(Ability ability, int index) {
        activeSkillEntries[index].AssignNewAbility(null);
    }

    private void OnAbilitySwapped(Ability first, int firstIndex, Ability second, int secondIndex) {
        //activeSkillEntries[firstIndex].AssignNewAbility(null);
        //activeSkillEntries[secondIndex].AssignNewAbility(null);
        activeSkillEntries[firstIndex].AssignNewAbility(second);
        activeSkillEntries[secondIndex].AssignNewAbility(first);
    }
    #endregion

}
