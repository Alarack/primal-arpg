using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using UnityEditor.Playables;
using Unity.VisualScripting;

public class AnimHelper : MonoBehaviour {

    public Animator animator;

    private Entity owner;

    private void Awake() {
        owner = GetComponentInParent<Entity>();
    }

    private void OnEnable() {

        if (owner != null && owner is EntityPlayer) {
            EventManager.RegisterListener(GameEvent.UserActivatedAbility, OnAbilityActivated);
        }

        if (owner != null && owner is NPC) {
            EventManager.RegisterListener(GameEvent.AbilityInitiated, OnAIAbilityActivated);
        }

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


    private void OnAbilityActivated(EventData data) {
        Ability ability = data.GetAbility("Ability");

        if (string.IsNullOrEmpty(ability.Data.animationString) == true) {
            Debug.Log("No animation for: " + ability.Data.abilityName);
            return;
        }

        SetAttackAnim(ability, true);

        //if (animator.GetCurrentAnimatorStateInfo(0).IsName(ability.Data.animationString)) {
        //    return;
        //}

        //if (ability.IsReady == false)
        //    return;

        //Debug.Log("Recieving activation for: " + ability.Data.abilityName);

        //SetTrigger(ability.Data.animationString);
    }

    private void OnAIAbilityActivated(EventData data) {

        //Debug.Log("Ability Initiated: " + data.GetAbility("Ability").Data.abilityName);
        
        NPC npc = data.GetEntity("Source") as NPC;

        if (npc == null || owner != npc) {
            return;
        }

        Ability ability = data.GetAbility("Ability");

        SetAttackAnim(ability);

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

        //Debug.Log("Recieving activation for: " + ability.Data.abilityName);

        SetTrigger(ability.Data.animationString);
    }

}
