using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;

public class AnimHelper : MonoBehaviour
{

    public Animator animator;

    private Entity owner;

    private void Awake() {
        owner = GetComponentInParent<Entity>();
    }

    private void OnEnable() {
        
        if(owner != null && owner.entityType == Entity.EntityType.Player) {
            EventManager.RegisterListener(GameEvent.UserActivatedAbility, OnAbilityActivated);
        }

    }


    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }


    public void SetBool(string name, bool value) {
        animator.SetBool(name, value);
    }

    
    public void SetTrigger(string name) {
        animator.SetTrigger(name);
    }


    private void OnAbilityActivated(EventData data) {
        Ability ability = data.GetAbility("Ability");

        if(string.IsNullOrEmpty( ability.Data.animationString) == true) {
            Debug.Log("No animation for: " + ability.Data.abilityName);
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(ability.Data.animationString)) {
            return;
        }

        if (ability.IsReady == false)
            return;

        Debug.Log("Recieving activation for: " + ability.Data.abilityName);

        SetTrigger(ability.Data.animationString);
    }

}
