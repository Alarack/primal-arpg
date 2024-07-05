using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;

public class AnimHelper : MonoBehaviour {

    public Animator animator;

    private Entity owner;

    private Ability currentAbility;
    private AbilityTrigger.TriggerInstance currentTriggerInstance;

    private void Awake() {
        owner = GetComponentInParent<Entity>();
    }

    private void OnEnable() {

        if (owner != null) {
            EventManager.RegisterListener(GameEvent.AbilityInitiated, OnAbilityInitiated);
        }


        //if (owner != null && owner is EntityPlayer) {
        //    EventManager.RegisterListener(GameEvent.UserActivatedAbility, OnAbilityActivated);
        //}

        //if (owner != null && owner is NPC) {
        //    EventManager.RegisterListener(GameEvent.AbilityInitiated, OnAIAbilityActivated);
        //}

    }


    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }


    public void SetBool(string name, bool value) {
        animator.SetBool(name, value);

        //Debug.Log("Anim: " + name + " value: " + value + " Owner: " + owner.EntityName);
    }


    public void SetTrigger(string name) {
        animator.SetTrigger(name);
    }

    public void ReceiveAnimEvent(string name) {
        EventData data = new EventData();
        data.AddAbility("Ability", currentAbility);
        data.AddTriggerInstance("Instance", currentTriggerInstance);
        
        EventManager.SendEvent(GameEvent.AbilityAnimReceived, data);
    }


    //private void OnAbilityActivated(EventData data) {
    //    Ability ability = data.GetAbility("Ability");

    //    if (string.IsNullOrEmpty(ability.Data.animationString) == true) {
    //        Debug.Log("No animation for: " + ability.Data.abilityName);
    //        return;
    //    }

    //    SetAttackAnim(ability, true);

    //    //if (animator.GetCurrentAnimatorStateInfo(0).IsName(ability.Data.animationString)) {
    //    //    return;
    //    //}

    //    //if (ability.IsReady == false)
    //    //    return;

    //    //Debug.Log("Recieving activation for: " + ability.Data.abilityName);

    //    //SetTrigger(ability.Data.animationString);
    //}

    private void OnAbilityInitiated(EventData data) {

        //Debug.Log("Ability Initiated: " + data.GetAbility("Ability").Data.abilityName);
        
        Entity entity = data.GetEntity("Source");

        if (entity == null || owner != entity) {
            return;
        }

        Ability ability = data.GetAbility("Ability");
        AbilityTrigger.TriggerInstance triggerInstance = data.GetTriggerInstance("Instance");

        currentAbility = ability;
        currentTriggerInstance = triggerInstance;

        bool readyCheck = entity is EntityPlayer;

        SetAttackAnim(ability, readyCheck);

    }

    private void SetAttackAnim(Ability ability, bool readyCheck = false) {
        if (string.IsNullOrEmpty(ability.Data.animationString) == true) {
            //Debug.Log("No animation for: " + ability.Data.abilityName);
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(ability.Data.animationString)) {
            return;
        }

        if (readyCheck == true && ability.IsReady == false)
            return;

        Debug.Log("Recieving activation for: " + ability.Data.abilityName);

        SetTrigger(ability.Data.animationString);
    }

}
