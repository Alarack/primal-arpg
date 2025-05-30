using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using GameButtonType = InputHelper.GameButtonType;

public class HotbarPanel : SkillBasePanel {

    private PlayerInputActions playerInputActions;


    //private List<Action<GameButtonType>> autoFireSlots = new List<Action<GameButtonType>>();
    private Dictionary<InputAction, GameButtonType> autoFireDict = new Dictionary<InputAction, GameButtonType>();

    protected override void Awake() {
        base.Awake();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    protected override void OnEnable() {
        base.OnEnable();

        playerInputActions.Player.Fire.performed += OnFirePerformed;
        playerInputActions.Player.Fire.canceled += OnFireCanceled;

        playerInputActions.Player.SecondaryFire.performed += OnSecondaryFirePerformed;
        playerInputActions.Player.SecondaryFire.canceled += OnsecondaryFireCanceled;

        playerInputActions.Player.Skill1.performed += OnSkill1Performed;
        playerInputActions.Player.Skill1.canceled += OnSkill1Canceled;  

        playerInputActions.Player.Skill2.performed += OnSkill2Performed;
        playerInputActions.Player.Skill2.canceled += OnSkill2Canceled;

        playerInputActions.Player.Skill3.performed += OnSkill3Performed;
        playerInputActions.Player.Skill3.canceled += OnSkill3Canceled;

        playerInputActions.Player.Skill4.performed += OnSkill4Performed;
        playerInputActions.Player.Skill4.canceled += OnSkill4Canceled;

    }

    protected override void OnDisable() {
        base.OnDisable();

        playerInputActions.Player.Fire.performed -= OnFirePerformed;
        playerInputActions.Player.Fire.canceled -= OnFireCanceled;

        playerInputActions.Player.SecondaryFire.performed -= OnSecondaryFirePerformed;
        playerInputActions.Player.SecondaryFire.canceled -= OnsecondaryFireCanceled;
        
        playerInputActions.Player.Skill1.performed -= OnSkill1Performed;
        playerInputActions.Player.Skill1.canceled -= OnSkill1Canceled;

        playerInputActions.Player.Skill2.performed -= OnSkill2Performed;
        playerInputActions.Player.Skill2.canceled -= OnSkill2Canceled;

        playerInputActions.Player.Skill3.performed -= OnSkill3Performed;
        playerInputActions.Player.Skill3.canceled -= OnSkill3Canceled;

        playerInputActions.Player.Skill4.performed -= OnSkill4Performed;
        playerInputActions.Player.Skill4.canceled -= OnSkill4Canceled;
    }

    protected override  void Update() {
        base.Update();
        if(PanelManager.IsBlockingPanelOpen() == true) {
            //Debug.Log("Blocked");
            return;
        }

        if (autoFireDict.Count < 1)
            return;

        foreach (var entry in autoFireDict) {
            float pressed = entry.Key.ReadValue<float>();

            if (pressed == 1f) {
                OnSkillBindPressed(entry.Value);
            }
        }
    }

    protected override void CreateEmptySlots() {
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
    
    public void AssignAbility(int index, Ability ability) {

    }

    #region EVENTS

    private void OnFirePerformed(InputAction.CallbackContext context) {
        OnSkillBindPressed(GameButtonType.PrimaryAttack, DoesAbilityAutoFire(GameButtonType.PrimaryAttack));
    }

    private void OnFireCanceled(InputAction.CallbackContext context) {
        OnSkillBindCanceled(GameButtonType.PrimaryAttack);
    }

    private void OnSecondaryFirePerformed(InputAction.CallbackContext context) {
        OnSkillBindPressed(GameButtonType.SecondaryAttack, DoesAbilityAutoFire(GameButtonType.SecondaryAttack));
    }

    private void OnsecondaryFireCanceled(InputAction.CallbackContext context) {
        OnSkillBindCanceled(GameButtonType.SecondaryAttack);
    }

    private void OnSkill1Performed(InputAction.CallbackContext context) {
        OnSkillBindPressed(GameButtonType.Skill1, DoesAbilityAutoFire(GameButtonType.Skill1));
    }

    private void OnSkill1Canceled(InputAction.CallbackContext context) {
        OnSkillBindCanceled(GameButtonType.Skill1);
    }

    private void OnSkill2Performed(InputAction.CallbackContext context) {
        OnSkillBindPressed(GameButtonType.Skill2, DoesAbilityAutoFire(GameButtonType.Skill2));
    }

    private void OnSkill2Canceled(InputAction.CallbackContext context) {
        OnSkillBindCanceled(GameButtonType.Skill2);
    }

    private void OnSkill3Performed(InputAction.CallbackContext context) {
        OnSkillBindPressed(GameButtonType.Skill3, DoesAbilityAutoFire(GameButtonType.Skill3));
    }

    private void OnSkill3Canceled(InputAction.CallbackContext context) {
        OnSkillBindCanceled(GameButtonType.Skill3);
    }

    private void OnSkill4Performed(InputAction.CallbackContext context) {
        OnSkillBindPressed(GameButtonType.Skill4, DoesAbilityAutoFire(GameButtonType.Skill4));
    }

    private void OnSkill4Canceled(InputAction.CallbackContext context) {
        OnSkillBindCanceled(GameButtonType.Skill4);
    }


    private void OnSkillBindPressed(GameButtonType button, bool skipManualFire = false) {

        if(skipManualFire == true) 
            return;

        if (PanelManager.IsBlockingPanelOpen() == true) {
            //Debug.Log("Blocked");
            return;
        }

        Vector2 mouseViewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        if (mouseViewportPos.y < 0.2f)
            return;

        Ability ability = GetAbilityBykeyBind(button);

        if (ability != null) {
            EventData eventData = new EventData();
            eventData.AddAbility("Ability", ability);

            EventManager.SendEvent(GameEvent.UserActivatedAbility, eventData);
        }
    }

    private void OnSkillBindCanceled(GameButtonType button) {
        if (PanelManager.IsBlockingPanelOpen() == true) {
            //Debug.Log("Blocked");
            return;
        }

        Ability ability = GetAbilityBykeyBind(button);

        if (ability != null) {
            EventData eventData = new EventData();
            eventData.AddAbility("Ability", ability);

            EventManager.SendEvent(GameEvent.UserAbilityCanceled, eventData);
        }
    }

    private bool DoesAbilityAutoFire(GameButtonType button) {
        Ability target = GetAbilityBykeyBind(button);

        if(target == null) 
            return false;

        return target.Data.autoFire;
    }

    private void PopulateAutoFireDict(Ability ability, int index) {

        SkillEntry targetEntry = activeSkillEntries[index];
        InputAction targetAction = GetInputActionByIndex(index);

        if (targetAction == null)
            return;

        if (ability.AutoFire == true) {

            if (autoFireDict.ContainsKey(targetAction) == true) {
                autoFireDict[targetAction] = targetEntry.keybind;
            }
            else {
                autoFireDict.Add(targetAction, targetEntry.keybind);
            }

        }
        else {
            if (autoFireDict.ContainsKey(targetAction) == true) {
                autoFireDict.Remove(targetAction);
            }
        }
    }

    private InputAction GetInputActionByIndex(int index) {
        SkillEntry targetEntry = activeSkillEntries[index];

        InputAction targetAction = targetEntry.keybind switch {
            GameButtonType.PrimaryAttack => playerInputActions.Player.Fire,
            GameButtonType.SecondaryAttack => playerInputActions.Player.SecondaryFire,
            GameButtonType.Skill1 => playerInputActions.Player.Skill1,
            GameButtonType.Skill2 => playerInputActions.Player.Skill2,
            GameButtonType.Skill3 => playerInputActions.Player.Skill3,
            GameButtonType.Skill4 => playerInputActions.Player.Skill4,
            _ => null,
        };

        return targetAction;
    }

    protected override void OnAbilityEquipped(Ability ability, int index) {
        base.OnAbilityEquipped(ability, index);

        PopulateAutoFireDict(ability, index);
    }

    protected override void OnAbilityUnequipped(Ability ability, int index) {
        base.OnAbilityUnequipped(ability, index);

        InputAction targetAction = GetInputActionByIndex(index);

        if (targetAction != null) {
            autoFireDict.Remove(targetAction);
        }

    }

    protected override void OnAbilitySwapped(Ability first, int firstIndex, Ability second, int secondIndex) {
        base.OnAbilitySwapped(first, firstIndex, second, secondIndex);
        PopulateAutoFireDict(first, secondIndex);
        PopulateAutoFireDict(second, firstIndex);

    }

    #endregion

}
