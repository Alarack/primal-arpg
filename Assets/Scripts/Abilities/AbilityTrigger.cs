using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;
using LL.FSM;

public abstract class AbilityTrigger {
    public abstract TriggerType Type { get; }
    public abstract GameEvent TargetEvent { get; }
    public abstract Action<EventData> EventReceiver { get; }

    public Action<TriggerInstance> ActivationCallback { get; set; }

    public Ability ParentAbility { get; protected set; }
    public Entity SourceEntity { get; protected set; }
    public Entity TriggeringEntity { get; protected set; }
    public Entity CauseOfTrigger { get; protected set; }
    public TriggerData Data { get; protected set; }


    protected Dictionary<ConstraintFocus, List<AbilityConstraint>> constraintDict = new Dictionary<ConstraintFocus, List<AbilityConstraint>>();



    #region CONSTRUCTION AND SETUP

    public AbilityTrigger(TriggerData data, Entity source, Ability parentAbility = null) {
        this.ParentAbility = parentAbility;
        this.Data = data;
        this.SourceEntity = source;
        SetupConstraints();
        RegisterEvents();
    }

    protected void RegisterEvents() {
        EventManager.RegisterListener(TargetEvent, EventReceiver);
    }

    public void TearDown() {
        EventManager.RemoveMyListeners(this);
        constraintDict.Clear();
        ActivationCallback = null;
    }

    /// <summary>
    /// Loop Through our Constraint Data Focus class, and populate our constraint dictionary with Ability Constraints
    /// </summary>
    protected void SetupConstraints() {
        foreach (ConstraintDataFocus constraintFocus in Data.allConstraints) {
            
            //For each focus (Source, Trigger, Cause) in our data, add an assosiated entry into our constraint Dictionary.
            constraintDict.Add(constraintFocus.focus, CreateConstraints(constraintFocus.constraintData));
        }
    }

    /// <summary>
    /// Convert Constraint Data Class into Ability Constraint Functional Class
    /// </summary>
    protected List<AbilityConstraint> CreateConstraints(List<ConstraintData> data) {
        List<AbilityConstraint> results = new List<AbilityConstraint>();

        for (int i = 0; i < data.Count; i++) {

            AbilityConstraint newConstraint = AbilityFactory.CreateAbilityConstraint(data[i], SourceEntity, ParentAbility);
            results.Add(newConstraint);
        }

        return results;
    }

    #endregion


    /// <summary>
    /// Check ALL Focuses (Source, Trigger, Cause) and their assosiated individual constraint lists
    /// </summary>
    protected bool CheckAllConstrains(TriggerInstance activationInstance) {

        foreach (var entry in constraintDict) {
            Entity focusEntity = GetConstraintFocus(entry.Key);

            bool checkResult = CheckFocusConstraints(entry.Key, focusEntity, activationInstance);

            if (checkResult == false)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Check if the given Focus (Source, Trigger, Cause) meets ALL its constraint requirements
    /// </summary>
    protected bool CheckFocusConstraints(ConstraintFocus focus, Entity target, TriggerInstance activationInstance) {

        foreach (AbilityConstraint constraint in constraintDict[focus]) {
            bool result = constraint.Evaluate(target, activationInstance);

            //if (this is StateEnteredTrigger && constraint is IsInStateConstraint) {
            //    Debug.LogWarning(result + " is the state check result for: " + GetType().ToString());
            //}

            if (result == false) {
                //if (this is StateEnteredTrigger) {
                //    Debug.LogWarning("A trigger of type: " + GetType().ToString() + " failed a constraint: " + constraint.Type);

                //}
                return false;
            }

        }

        return true;

    }

    /// <summary>
    /// Who are we asking questions about?
    /// Get an entity (Source, Trigger, Cause) assosiated with a constraint focus 
    /// </summary>
    protected Entity GetConstraintFocus(ConstraintFocus focus) {
        switch (focus) {
            case ConstraintFocus.Source:
                return SourceEntity;

            case ConstraintFocus.Trigger:
                return TriggeringEntity;

            case ConstraintFocus.Cause:
                return CauseOfTrigger;
        }

        return null;
    }

    /// <summary>
    /// Check all constraints and anything else that may invalidate an ability activating
    /// </summary>
    protected bool IsTriggerValid(TriggerInstance activationInstance) {
        if (CheckAllConstrains(activationInstance) == false)
            return false;

        return true;
    }

    /// <summary>
    /// Attempt to Activate an ability after receving an event
    /// </summary>
    protected void TryActivateTrigger(TriggerInstance activationInstance) {
        if (IsTriggerValid(activationInstance) == false) {
            return;
        }

        ActivationCallback?.Invoke(activationInstance);
    }

    public class TriggerInstance {
        public Entity TriggeringEntity { get; private set; }
        public Entity CauseOfTrigger { get; private set; }
        public TriggerType Type { get; private set; }

        public TriggerInstance(Entity trigger, Entity cause, TriggerType type) {
            this.TriggeringEntity = trigger;
            this.CauseOfTrigger = cause;
            this.Type = type;
        }
    }

}

public class UserActivatedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UserActivated;
    public override GameEvent TargetEvent => GameEvent.UserActivatedAbility;
    public override Action<EventData> EventReceiver => OnUserActivation;

    public UserActivatedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnUserActivation(EventData data) {
      
        if(ParentAbility == null) {
            Debug.LogError("a user activated trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        if(triggeringAbility != ParentAbility) {
            return;
        }


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);

    }
}

public class AbilityEquippedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.AbilityEquipped;
    public override GameEvent TargetEvent => GameEvent.AbilityEquipped;
    public override Action<EventData> EventReceiver => OnAbilityEquipped;

    public AbilityEquippedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAbilityEquipped(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("an Ability Equipped trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        if (triggeringAbility != ParentAbility) {
            return;
        }


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);

    }
}

public class AbilityUnequippedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.AbilityUnequipped;
    public override GameEvent TargetEvent => GameEvent.AbilityUnequipped;
    public override Action<EventData> EventReceiver => OnAbilityUnequipped;

    public AbilityUnequippedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAbilityUnequipped(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("an Ability Unequipped trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        if (triggeringAbility != ParentAbility) {
            return;
        }


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);

    }
}

public class UnitDiedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UnitDied;
    public override GameEvent TargetEvent => GameEvent.UnitDied;
    public override Action<EventData> EventReceiver => OnUnitDied;

    public UnitDiedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnUnitDied(EventData data) {
        Entity victim = data.GetEntity("Victim");
        Entity killer = data.GetEntity("Killer");

        TriggeringEntity = victim;
        CauseOfTrigger = killer;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);

    }
}

public class StatChangedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UnitStatChanged;
    public override GameEvent TargetEvent => GameEvent.UnitStatAdjusted;
    public override Action<EventData> EventReceiver => OnStatChanged;

    public StatChangedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnStatChanged(EventData data) {
        StatName targetStat = (StatName)data.GetInt("Stat");
        Entity affectedTarget = data.GetEntity("Target");
        Entity causeOfChange = data.GetEntity("Cause");
        float changeValue = data.GetFloat("Value");

        TriggeringEntity = affectedTarget;
        CauseOfTrigger = causeOfChange;

        Debug.Log(affectedTarget.gameObject.name + " had a stat change: " + targetStat + " :: " + changeValue);


        StatChangeTriggerInstance triggerInstance = new StatChangeTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, targetStat, changeValue);
        TryActivateTrigger(triggerInstance);
    }

    public class StatChangeTriggerInstance : TriggerInstance {
        public StatName targetStat;
        public float changeValue;

        public StatChangeTriggerInstance(Entity trigger, Entity cause, TriggerType type, StatName targetStat, float changeValue) : base(trigger, cause, type) {
            this.targetStat = targetStat;
            this.changeValue = changeValue;
        }

    }

}


public class UnitForgottenTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UnitForgotten;

    public override GameEvent TargetEvent => GameEvent.UnitForgotten;

    public override Action<EventData> EventReceiver => OnUnitForgotten;

    public UnitForgottenTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnUnitForgotten(EventData data) {
        Entity target = data.GetEntity("Target");
        Entity cause = data.GetEntity("Cause");

        TriggeringEntity = target;

        if (cause != null)
            CauseOfTrigger = cause;
        else
            CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);
    }


}

public class UnitDetectedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UnitDetected;

    public override GameEvent TargetEvent => GameEvent.UnitDetected;

    public override Action<EventData> EventReceiver => OnUnitDetected;

    public UnitDetectedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnUnitDetected(EventData data) {
        Entity target = data.GetEntity("Target");
        Entity cause = data.GetEntity("Cause");

        TriggeringEntity = target;
        if (cause != null)
            CauseOfTrigger = cause;
        else
            CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);
    }
}

public class StateEnteredTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.StateEntered;

    public override GameEvent TargetEvent => GameEvent.StateEntered;

    public override Action<EventData> EventReceiver => OnStateEntered;

    public StateEnteredTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnStateEntered(EventData data) {
        Entity target = data.GetEntity("Target");
        string stateEntered = data.GetString("State");

        TriggeringEntity = target;

        //Debug.LogWarning(SourceEntity.gameObject.name + " is trying to trigger an on state entered trigger");
        StateEnteredTriggerInstance triggerInstance = new StateEnteredTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, stateEntered);
        TryActivateTrigger(triggerInstance);
    }


    public class StateEnteredTriggerInstance : TriggerInstance {

        public string stateEntered;
        
        public StateEnteredTriggerInstance(Entity trigger, Entity cause, TriggerType type, string stateEntered) : base(trigger, cause, type) {
            this.stateEntered = stateEntered;
        }
    }


}


public class WeaponCooldownFinishedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.WeaponCooldownFinished;

    public override GameEvent TargetEvent => GameEvent.WeaponCooldownFinished;

    public override Action<EventData> EventReceiver => OnWeaponCooldownFinished;

    public WeaponCooldownFinishedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnWeaponCooldownFinished(EventData data) {

        Entity owner = data.GetEntity("Owner");
        Weapon weapon = data.GetWeapon("Weapon");

        if (owner == null) {
            Debug.LogError("A null owner was passed to a Weapon Cooldown Started Trigger");
        }

        TriggeringEntity = owner;
        CauseOfTrigger = owner;

        WeaponCooldownFinishedTriggerInstance triggerInstance = new WeaponCooldownFinishedTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, weapon);

        TryActivateTrigger(triggerInstance);
    }



    public class WeaponCooldownFinishedTriggerInstance : TriggerInstance {

        public Weapon weapon;

        public WeaponCooldownFinishedTriggerInstance(Entity trigger, Entity cause, TriggerType type, Weapon weapon) : base(trigger, cause, type) {
            this.weapon = weapon;
        }

    }


}

public class WeaponCooldownStartedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.WeaponCooldownStarted;

    public override GameEvent TargetEvent => GameEvent.WeaponCooldownStarted;

    public override Action<EventData> EventReceiver => OnWeaponCooldownStarted;

    public WeaponCooldownStartedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnWeaponCooldownStarted(EventData data) {

        Entity owner = data.GetEntity("Owner");
        Weapon weapon = data.GetWeapon("Weapon");

        if(owner == null) {
            Debug.LogError("A null owner was passed to a Weapon Cooldown Started Trigger");
        }

        TriggeringEntity = owner;
        CauseOfTrigger = owner;

        WeaponCooldownStartedTriggerInstance triggerInstance = new WeaponCooldownStartedTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, weapon);

        TryActivateTrigger(triggerInstance);
    }

    public class WeaponCooldownStartedTriggerInstance : TriggerInstance {

        public Weapon weapon;

        public WeaponCooldownStartedTriggerInstance(Entity trigger, Entity cause, TriggerType type, Weapon weapon) : base(trigger, cause, type) {
            this.weapon = weapon;
        }

    }


}

public class TimedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.Timed;

    public override GameEvent TargetEvent => GameEvent.TriggerTimerCompleted;

    public override Action<EventData> EventReceiver => OnTriggerTimerCompleted;

    public TimedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        //SetupTriggerTimer();
    
    }


    private void SetupTriggerTimer() {
        TimerManager.AddTimer(this, Data.triggerTimerDuration, true);
    }


    private void OnTriggerTimerCompleted(EventData data) {

        AbilityTrigger trigger = data.GetTrigger("Trigger");

        if (trigger != this)
            return;


        Entity owner = data.GetEntity("Owner");

        if (owner == null) {
            Debug.LogError("A null owner was passed to a Timer Trigger");
        }

        TriggeringEntity = owner;
        CauseOfTrigger = owner;


        Debug.LogWarning("Trigger Timer Complete");

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);

        TryActivateTrigger(triggerInstance);
    }

    


}




