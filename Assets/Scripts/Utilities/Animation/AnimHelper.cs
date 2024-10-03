using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using UnityEditor.Playables;

public class AnimHelper : MonoBehaviour {

    public Animator animator;

    private Entity owner;

    //private Ability currentAbility;
    //private AbilityTrigger.TriggerInstance currentTriggerInstance;

    private Queue<AbilityActivationInstance> abilityQueue = new Queue<AbilityActivationInstance>();

    private void Awake() {
        owner = GetComponentInParent<Entity>();
    }

    private void OnEnable() {

        if (owner != null) {
            EventManager.RegisterListener(GameEvent.AbilityInitiated, OnAbilityInitiated);
            EventManager.RegisterListener(GameEvent.AbilityEnded, OnAbilityEnded);
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
        
        AbilityActivationInstance nextInstance = abilityQueue.Dequeue();
        
        
        data.AddAbility("Ability", nextInstance.ability);
        data.AddTriggerInstance("Instance", nextInstance.triggerInstance);

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

        if (ability.IsChanneled == true && ability.IsActive == true) {
            StartChannelAnim(ability);
            return;
        }


        if (string.IsNullOrEmpty(ability.Data.animationString) == true) {
            return;
        }

        AbilityTrigger.TriggerInstance triggerInstance = data.GetTriggerInstance("Instance");



        //currentAbility = ability;
        //currentTriggerInstance = triggerInstance;

        abilityQueue.Enqueue(new AbilityActivationInstance(ability, triggerInstance));


        bool readyCheck = entity is EntityPlayer;

        SetAttackAnim(ability, readyCheck);

    }

    private void StartChannelAnim(Ability ability) {



        if(IsAnimRunning("Channel") == false) {
            //Debug.Log("Starting Channel");
            SetBool("Channel", true);
        }

    }

    private void OnAbilityEnded(EventData data) {
        Entity entity = data.GetEntity("Source");

        if (entity == null || owner != entity) {
            return;
        }

        Ability ability = data.GetAbility("Ability");

        if (ability.IsChanneled == true && IsAnimRunning("Channel") == true) {
            //Debug.Log("Ending Channel");
            SetBool("Channel", false);
        }


    }

    private void SetAttackAnim(Ability ability, bool readyCheck = false) {
        if (string.IsNullOrEmpty(ability.Data.animationString) == true) {
            //Debug.Log("No animation for: " + ability.Data.abilityName);
            return;
        }

        if (IsAnimRunning(ability.Data.animationString)) {
            return;
        }

        if (readyCheck == true && ability.IsReady == false)
            return;

        //Debug.Log("Recieving activation for: " + ability.Data.abilityName);

        SetTrigger(ability.Data.animationString);
    }

    public bool IsAnimRunning(string animName) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animName);
    }




    public class AbilityActivationInstance {
        public Ability ability;
        public AbilityTrigger.TriggerInstance triggerInstance;

        public AbilityActivationInstance() {

        }

        public AbilityActivationInstance(Ability ability, AbilityTrigger.TriggerInstance triggerInstance) {
            this.ability = ability;
            this.triggerInstance = triggerInstance;
        }
    }

}
