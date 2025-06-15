using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;
using DG.Tweening;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public abstract class AbilityTrigger {
    public abstract TriggerType Type { get; }
    public abstract GameEvent TargetEvent { get; }
    public abstract Action<EventData> EventReceiver { get; }

    public Action<TriggerInstance> ActivationCallback { get; set; }

    public Ability ParentAbility { get; protected set; }
    public Entity SourceEntity { get; protected set; }
    public Entity TriggeringEntity { get; protected set; }
    public Entity CauseOfTrigger { get; protected set; }
    public Ability TriggeringAbility { get; protected set; }
    public Ability CauseOfAbilityTrigger { get; protected set; }
    public TriggerData Data { get; protected set; }

    public string AIState { get; set; }

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

    public virtual void TearDown() {
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

    protected bool CheckMultiTypeConstraints(TriggerInstance activationInstance) {

        foreach (var entry in constraintDict) {
            Tuple<Entity, Ability, Effect> foci = GetConstraintFocus(entry.Key, activationInstance);

            if (foci == null) {
                Debug.LogError("Could not get constraint focus group from a trigger on : " + SourceEntity.EntityName + " : " + ParentAbility.Data.abilityName);
                return false;
            }


            switch (entry.Key) {
                case ConstraintFocus.Source:
                case ConstraintFocus.Trigger:
                case ConstraintFocus.Cause:

                    if (foci.Item1 != null) {
                        bool entityCheck = CheckFocusConstraints(entry.Key, foci.Item1, activationInstance);

                        if (entityCheck == false) {
                            //if (ParentAbility != null && ParentAbility.Data.abilityName == "Reaping Summon")
                            //    Debug.Log("an ability: " + ParentAbility.Data.abilityName + " failed an entity constraint");
                            //else
                            //    Debug.LogWarning("A Trigger: " + Type.ToString() + " failed a constraint");

                            return false;
                        }
                    }

                    break;
                case ConstraintFocus.AbilitySource:
                case ConstraintFocus.AbilityTrigger:
                case ConstraintFocus.AbiityCause:

                    if (foci.Item2 != null) {
                        bool abilitycheck = CheckAbilityFocusConstraints(entry.Key, foci.Item2, activationInstance);

                        if (abilitycheck == false) {
                            //Debug.Log("an ability: " + ParentAbility.Data.abilityName + " failed an ability constraint");
                            return false;
                        }
                    }

                    break;
                default:
                    if (foci.Item3 != null) {
                        bool effectCheck = CheckEffectFocusConstraints(entry.Key, foci.Item3, activationInstance);

                        if (effectCheck == false)
                            return false;
                    }
                    break;
            }

        }

        return true;

    }


    //protected bool CheckAllAbilityConstraints(AbilityTriggerInstance activationInstance) {
    //    foreach (var entry in constraintDict) {
    //        Ability focusAbility = GetAbilityConstraintFocus(entry.Key);

    //        bool checkResult = CheckAbilityFocusConstraints(entry.Key, focusAbility, activationInstance);

    //        if (checkResult == false)
    //            return false;
    //    }

    //    return true;
    //}

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


    protected bool CheckAbilityFocusConstraints(ConstraintFocus focus, Ability target, TriggerInstance activationInstance) {
        foreach (AbilityConstraint constraint in constraintDict[focus]) {
            bool result = constraint.Evaluate(target, activationInstance);

            if (result == false)
                return false;
        }

        return true;
    }

    protected bool CheckEffectFocusConstraints(ConstraintFocus focus, Effect target, TriggerInstance activationInstance) {
        foreach (AbilityConstraint constraint in constraintDict[focus]) {
            bool result = constraint.Evaluate(target, activationInstance);

            if (result == false)
                return false;
        }

        return true;
    }



    public Tuple<Entity, Ability, Effect> GetConstraintFocus(ConstraintFocus focus, TriggerInstance instance) {
        Tuple<Entity, Ability, Effect> result = focus switch {
            ConstraintFocus.Source => new Tuple<Entity, Ability, Effect>(SourceEntity, instance.SourceAbility, instance.SourceEffect),
            ConstraintFocus.Trigger => new Tuple<Entity, Ability, Effect>(TriggeringEntity, instance.TriggeringAbility, instance.TriggeringEffect),
            ConstraintFocus.Cause => new Tuple<Entity, Ability, Effect>(CauseOfTrigger, instance.CausingAbility, instance.CausingEffect),
            ConstraintFocus.AbilitySource => new Tuple<Entity, Ability, Effect>(SourceEntity, instance.SourceAbility, instance.SourceEffect),
            ConstraintFocus.AbilityTrigger => new Tuple<Entity, Ability, Effect>(TriggeringEntity, instance.TriggeringAbility, instance.TriggeringEffect),
            ConstraintFocus.AbiityCause => new Tuple<Entity, Ability, Effect>(CauseOfTrigger, instance.CausingAbility, instance.CausingEffect),
            _ => null,
        };

        return result;
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

    protected Ability GetAbilityConstraintFocus(ConstraintFocus focus) {
        Ability targetFocus = focus switch {
            ConstraintFocus.Source => ParentAbility,
            ConstraintFocus.Trigger => TriggeringAbility,
            ConstraintFocus.Cause => CauseOfAbilityTrigger,
            _ => null,
        };

        return targetFocus;
    }



    /// <summary>
    /// Check all constraints and anything else that may invalidate an ability activating
    /// </summary>
    protected bool IsTriggerValid(TriggerInstance activationInstance) {

        //if(activationInstance is AbilityTriggerInstance) {
        //    if(CheckAllAbilityConstraints(activationInstance as AbilityTriggerInstance) == false) {
        //        return false;
        //    }
        //    else {
        //        return true;
        //    }
        //}

        //if (CheckAllConstrains(activationInstance) == false)
        //    return false;


        if (CheckMultiTypeConstraints(activationInstance) == false)
            return false;


        return true;
    }

    /// <summary>
    /// Attempt to Activate an ability after receving an event
    /// </summary>
    protected void TryActivateTrigger(TriggerInstance activationInstance) {
        if (Data.delay1Frame == true) {
            new Task(FrameDelay(activationInstance));
            return;
        }

        if (IsTriggerValid(activationInstance) == false) {
            return;
        }

        if (RollProc(activationInstance) == false) {
            return;
        }

        if (Data.triggerDelay > 0f) {
            Debug.Log("Delaying trigger for: " + ParentAbility.Data.abilityName);
            new Task(CustomDelay(activationInstance));
            return;
        }

        ActivationCallback?.Invoke(activationInstance);
    }

    protected bool RollProc(TriggerInstance activationInstance) {

        if (ParentAbility != null) {
            
            if (ParentAbility.Stats.Contains(StatName.ProcChance) == false && Data.overrideProcStat == false)
                return true;

            if (Data.overrideProcStat == true && Data.procChance >= 1f)
                return true;


            if(ParentAbility.Data.normalizedProcRate == true) {
                if(activationInstance.TriggeringAbility != null) {
                    float cooldown = activationInstance.TriggeringAbility.Stats[StatName.Cooldown];

                    //Debug.LogWarning("Normalizing a proc from: " + activationInstance.TriggeringAbility.Data.abilityName);

                    if(cooldown <= 0f) {
                        return UnityEngine.Random.Range(0f, 1f) < 0.05f;
                    }

                    if(cooldown >= 2f) {
                        return true;
                    }

                    return UnityEngine.Random.Range(0f, 1f) < cooldown /2f;
                }
            }

            float proc = Data.overrideProcStat == true ? Data.procChance : ParentAbility.Stats[StatName.ProcChance];

            if(ParentAbility.Data.scaleProcByLevel == true) {
                proc *= ParentAbility.AbilityLevel;
            }


            //float proc = ParentAbility.Stats[StatName.ProcChance];
            float roll = UnityEngine.Random.Range(0f, 1f);

            return roll < proc;

        }

        if (Data.procChance > 0f && Data.procChance < 1f) {
            float roll = UnityEngine.Random.Range(0f, Data.procChance);

            return roll < Data.procChance;
        }

        //Debug.LogWarning("A trigger of type: " + Data.type + " has no parent ability,  and no proc chance in its data. Source Entity: " + SourceEntity.EntityName);

        return true;
    }

    protected IEnumerator FrameDelay(TriggerInstance activationInstance) {
        yield return new WaitForEndOfFrame();

        if (IsTriggerValid(activationInstance) == false) {
            yield break;
        }

        ActivationCallback?.Invoke(activationInstance);
    }

    protected IEnumerator CustomDelay(TriggerInstance activationInstance) {
        WaitForSeconds waiter = new WaitForSeconds(Data.triggerDelay);
        yield return waiter;

        Debug.Log("Resolving delayed trigger for: " + ParentAbility.Data.abilityName);

        ActivationCallback?.Invoke(activationInstance);
    }

    public class TriggerInstance {
        public Entity TriggeringEntity { get; private set; }
        public Entity CauseOfTrigger { get; private set; }

        public Ability TriggeringAbility { get; set; }
        public Ability CausingAbility { get; set; }
        public Ability SourceAbility { get; set; }

        public Effect TriggeringEffect { get; set; }
        public Effect CausingEffect { get; set; }
        public Effect SourceEffect { get; set; }

        public Vector2 SavedLocation { get; set; }

        public TriggerType Type { get; private set; }

        public TriggerInstance(Entity trigger, Entity cause, TriggerType type) {
            this.TriggeringEntity = trigger;
            this.CauseOfTrigger = cause;
            this.Type = type;
        }


    }

    //public class AbilityTriggerInstance : TriggerInstance {
    //    public Ability triggeringAbility;
    //    public Ability causeOfTriggerAbility;
    //    public Ability sourceAbility;

    //    public AbilityTriggerInstance(Entity trigger, Entity cause, TriggerType type, Ability triggeringAbility, Ability sourceAbility, Ability causeOfTriggerAbility) : base(trigger, cause, type) {
    //        this.triggeringAbility = triggeringAbility;
    //        this.sourceAbility = sourceAbility;
    //        this.causeOfTriggerAbility = causeOfTriggerAbility;
    //    }

    //}


}

public class UserActivatedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UserActivated;
    public override GameEvent TargetEvent => GameEvent.UserActivatedAbility;
    public override Action<EventData> EventReceiver => OnUserActivation;

    public UserActivatedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnUserActivation(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("a user activated trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        //if(triggeringAbility != ParentAbility) {
        //    return;
        //}

        //if(triggeringAbility.Data.abilityName == "Test Sword Swipe") {
        //    Debug.Log("Swipe activation recieved");
        //}


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);

    }
}

public class UserCancelledTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UserCancelled;
    public override GameEvent TargetEvent => GameEvent.UserAbilityCanceled;
    public override Action<EventData> EventReceiver => OnUserCancelled;

    public UserCancelledTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnUserCancelled(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("a user activated trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        if(triggeringAbility.IsChanneled == false) {
            //Debug.LogWarning("A non channeled ability has been canceled. This hack is stopping the cancelation");
            return;
        }

        //if(triggeringAbility != ParentAbility) {
        //    return;
        //}

        //if(triggeringAbility.Data.abilityName == "Test Sword Swipe") {
        //    Debug.Log("Swipe activation recieved");
        //}


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);

    }
}

public class AIActivatedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.AIActivated;
    public override GameEvent TargetEvent => GameEvent.AIActivated;
    public override Action<EventData> EventReceiver => OnAIActivation;

    public AIActivatedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAIActivation(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("an AI activated trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");
        NPC owner = data.GetEntity("NPC") as NPC;


        if (triggeringAbility != ParentAbility) {
            return;
        }

        if (owner == null) {
            Debug.LogError("Null Owner on an AI triggered ability: " + triggeringAbility.Data.abilityName);
            return;
        }




        //if(triggeringAbility.Data.abilityName == "Sword Guy Attack!") {
        //    Debug.Log("Swipe activation recieved");
        //}


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);

    }
}

public class AbilityResolvedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.AbilityResolved;
    public override GameEvent TargetEvent => GameEvent.AbilityResolved;
    public override Action<EventData> EventReceiver => OnAbilityResolved;

    public AbilityResolvedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAbilityResolved(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("An ability resolved trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        //if(triggeringAbility != ParentAbility) {
        //    return;
        //}

        //if(triggeringAbility.Data.abilityName == "Test Sword Swipe") {
        //    Debug.Log("Swipe activation recieved");
        //}

        //Debug.Log("Recieveing an ability resolve trigger: " + triggeringAbility.Data.abilityName);


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);

    }
}

public class AbilityEndedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.AbilityEnded;
    public override GameEvent TargetEvent => GameEvent.AbilityEnded;
    public override Action<EventData> EventReceiver => OnAbilityEnded;

    public AbilityEndedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAbilityEnded(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("An ability resolved trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");
        Entity effectZone = data.GetEntity("EffectZone");

        //if(triggeringAbility != ParentAbility) {
        //    return;
        //}

        //if(triggeringAbility.Data.abilityName == "Test Sword Swipe") {
        //    Debug.Log("Swipe activation recieved");
        //}

        //Debug.Log("Recieveing an ability resolve trigger: " + triggeringAbility.Data.abilityName);


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;
        triggerInstance.SavedLocation = effectZone != null ? effectZone.transform.position : Vector2.zero;
        TryActivateTrigger(triggerInstance);

    }
}

public class AbilityInitiatedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.AbilityInitiated;
    public override GameEvent TargetEvent => GameEvent.AbilityInitiated;
    public override Action<EventData> EventReceiver => OnAbilityInitiated;

    public AbilityInitiatedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAbilityInitiated(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("An ability resolved trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        //if(triggeringAbility != ParentAbility) {
        //    return;
        //}

        //if(triggeringAbility.Data.abilityName == "Test Sword Swipe") {
        //    Debug.Log("Swipe activation recieved");
        //}

        //Debug.Log("Recieveing an ability init trigger: " + triggeringAbility.Data.abilityName);


        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);

    }
}

public class TeleportInitiatedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.TeleportInitiated;
    public override GameEvent TargetEvent => GameEvent.TeleportInitiated;
    public override Action<EventData> EventReceiver => OnTeleportInitiated;

    public TeleportInitiatedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnTeleportInitiated(EventData data) {

        Entity trigger = data.GetEntity("Target");
        Ability causingAbility = data.GetAbility("Ability");
        Vector3 position = data.GetVector3("Position");
        //Effect causingEffect = data.GetEffect("Effect");

        //Debug.Log("Teleport detected: " + trigger.transform.position);

        TriggeringEntity = trigger;
        CauseOfTrigger = trigger;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        //triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.CausingAbility = causingAbility;
        triggerInstance.SourceAbility = ParentAbility;
        triggerInstance.SavedLocation = position;
        TryActivateTrigger(triggerInstance);

    }
}

public class TeleportConcludedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.TeleportConcluded;
    public override GameEvent TargetEvent => GameEvent.TeleportConcluded;
    public override Action<EventData> EventReceiver => OnTeleportConcluded;

    public TeleportConcludedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnTeleportConcluded(EventData data) {

        Entity trigger = data.GetEntity("Target");
        Ability causingAbility = data.GetAbility("Ability");
        //Effect causingEffect = data.GetEffect("Effect");


        TriggeringEntity = trigger;
        CauseOfTrigger = trigger;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        //triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.CausingAbility = causingAbility;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);

    }
}

public class StatusAppliedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.StatusApplied;
    public override GameEvent TargetEvent => GameEvent.StatusApplied;
    public override Action<EventData> EventReceiver => OnStatusApplied;

    public StatusAppliedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnStatusApplied(EventData data) {

        Entity target = data.GetEntity("Target");
        Entity cause = data.GetEntity("Cause");
        Ability causingAbility = data.GetAbility("Causing Ability");
        Effect causingEffect = data.GetEffect("Causing Effect");
        Status status = data.GetStatus("Status");

        TriggeringEntity = target;
        CauseOfTrigger = cause;


        //Debug.Log("Status Trigger: " + status.Data.statusName + " applied to: " + target.EntityName + " from " + cause.EntityName);

        StatusAppliedTriggerInstance triggerInstance = new StatusAppliedTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, status);
        triggerInstance.CausingAbility = causingAbility;
        triggerInstance.CausingEffect = causingEffect;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);
    }


    public class StatusAppliedTriggerInstance : TriggerInstance {
        public Status statusApplied;

        public StatusAppliedTriggerInstance(Entity trigger, Entity cause, TriggerType type, Status statusApplied) : base(trigger, cause, type) {
            this.statusApplied = statusApplied;
        }
    }
}

public class StatusStackedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.StatusStacked;
    public override GameEvent TargetEvent => GameEvent.StatusStacked;
    public override Action<EventData> EventReceiver => OnStatusStacked;

    public StatusStackedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnStatusStacked(EventData data) {

        Entity target = data.GetEntity("Target");
        Entity cause = data.GetEntity("Cause");
        Ability causingAbility = data.GetAbility("Causing Ability");
        Effect causingEffect = data.GetEffect("Causing Effect");
        Status status = data.GetStatus("Status");

        TriggeringEntity = target;
        CauseOfTrigger = cause;


        //Debug.Log("Status Trigger: " + status.Data.statusName + " stacked on: " + target.EntityName + " from " + cause.EntityName);

        StatusStackedTriggerInstance triggerInstance = new StatusStackedTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, status);
        triggerInstance.CausingAbility = causingAbility;
        triggerInstance.CausingEffect = causingEffect;
        triggerInstance.SourceAbility = ParentAbility;
        TryActivateTrigger(triggerInstance);
    }


    public class StatusStackedTriggerInstance : TriggerInstance {
        public Status statusApplied;

        public StatusStackedTriggerInstance(Entity trigger, Entity cause, TriggerType type, Status statusApplied) : base(trigger, cause, type) {
            this.statusApplied = statusApplied;
        }
    }
}

public class DashStartedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.DashStarted;
    public override GameEvent TargetEvent => GameEvent.DashStarted;
    public override Action<EventData> EventReceiver => OnDashStarted;

    public DashStartedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnDashStarted(EventData data) {

        Entity dasher = data.GetEntity("Entity");

        TriggeringEntity = dasher;
        CauseOfTrigger = dasher;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);
    }
}

public class DashEndedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.DashEnded;
    public override GameEvent TargetEvent => GameEvent.DashEnded;
    public override Action<EventData> EventReceiver => OnDashEnded;

    public DashEndedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnDashEnded(EventData data) {

        Entity dasher = data.GetEntity("Entity");

        TriggeringEntity = dasher;
        CauseOfTrigger = dasher;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);
    }
}

public class OverloadTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.OverloadTriggered;
    public override GameEvent TargetEvent => GameEvent.OverloadTriggered;
    public override Action<EventData> EventReceiver => OnAbilityOverload;

    public OverloadTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAbilityOverload(EventData data) {

        Ability triggeringAbility = data.GetAbility("Ability");
        Effect triggeringEffect = data.GetEffect("Effect");
        Entity target = data.GetEntity("Target");
        Entity cause = data.GetEntity("Source");

        TriggeringEntity = target;
        CauseOfTrigger = cause;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = triggeringAbility;
        triggerInstance.TriggeringEffect = triggeringEffect;
        triggerInstance.SourceAbility = ParentAbility;


        //Debug.Log(triggeringAbility.Data.abilityName + " is overloading on " + target.EntityName);
        //Debug.Log("Triggering Ability: " + triggeringAbility.Data.abilityName);
        //Debug.Log("Triggeing Effect: " + triggeringEffect.Data.effectName);

        TryActivateTrigger(triggerInstance);
    }
}

public class ProjectilePiercedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.ProjectilePierced;
    public override GameEvent TargetEvent => GameEvent.ProjectilePierced;
    public override Action<EventData> EventReceiver => OnProjectilePierced;

    public ProjectilePiercedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnProjectilePierced(EventData data) {

        Entity projectile = data.GetEntity("Projectile");
        Entity owner = data.GetEntity("Owner");
        Entity cause = data.GetEntity("Cause");
        Effect parentEffect = data.GetEffect("Parent Effect");
        Ability parentAbility = data.GetAbility("Ability");

        //Debug.Log(parentAbility.Data.abilityName + " is the piercing ability");


        TriggeringEntity = projectile;
        CauseOfTrigger = cause;


        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.SourceEffect = parentEffect;
        triggerInstance.TriggeringAbility = parentAbility;
        TryActivateTrigger(triggerInstance);
    }
}

public class ProjectileChainedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.ProjectileChained;
    public override GameEvent TargetEvent => GameEvent.ProjectileChained;
    public override Action<EventData> EventReceiver => OnProjectileChained;

    public ProjectileChainedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnProjectileChained(EventData data) {

        Entity projectile = data.GetEntity("Projectile");
        Entity owner = data.GetEntity("Owner");
        Entity cause = data.GetEntity("Cause");
        Effect parentEffect = data.GetEffect("Parent Effect");
        Ability parentAbility = data.GetAbility("Ability");

        //Debug.Log(parentAbility.Data.abilityName + " is the piercing ability");


        TriggeringEntity = projectile;
        CauseOfTrigger = cause;


        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.SourceEffect = parentEffect;
        triggerInstance.TriggeringAbility = parentAbility;
        TryActivateTrigger(triggerInstance);
    }
}

public class RuneEquippedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.RuneEquipped;
    public override GameEvent TargetEvent => GameEvent.RuneEquipped;
    public override Action<EventData> EventReceiver => OnRuneEquipped;

    public RuneEquippedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnRuneEquipped(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("an Ability Equipped trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");
        Item rune = data.GetItem("Item");
        //Ability cause = data.GetAbility("Cause");

        //if (triggeringAbility != ParentAbility) {
        //    return;
        //}


        TriggeringAbility = triggeringAbility;
        CauseOfAbilityTrigger = rune.Abilities[0];
        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        //Debug.Log("Rune Equipped Trigger recieved: Trigger " + triggeringAbility.Data.abilityName + ". Casuse: " + CauseOfAbilityTrigger.Data.abilityName);


        //AbilityTriggerInstance triggerInstance = new AbilityTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, triggeringAbility, ParentAbility, CauseOfAbilityTrigger);

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = TriggeringAbility;
        triggerInstance.CausingAbility = CauseOfAbilityTrigger;
        triggerInstance.SourceAbility = ParentAbility;

        TryActivateTrigger(triggerInstance);

    }
}

public class RuneUnequippedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.RuneUnequipped;
    public override GameEvent TargetEvent => GameEvent.RuneUnequipped;
    public override Action<EventData> EventReceiver => OnRuneUnequipped;

    public RuneUnequippedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnRuneUnequipped(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("an Ability Equipped trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");
        Item rune = data.GetItem("Item");
        //Ability cause = data.GetAbility("Cause");

        //if (triggeringAbility != ParentAbility) {
        //    return;
        //}

        TriggeringAbility = triggeringAbility;
        CauseOfAbilityTrigger = rune.Abilities[0];
        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        //AbilityTriggerInstance triggerInstance = new AbilityTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, triggeringAbility, ParentAbility, CauseOfAbilityTrigger);


        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = TriggeringAbility;
        triggerInstance.CausingAbility = CauseOfAbilityTrigger;
        triggerInstance.SourceAbility = ParentAbility;

        TryActivateTrigger(triggerInstance);

    }
}

public class AbilityLearnedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.AbilityLearned;
    public override GameEvent TargetEvent => GameEvent.AbilityLearned;
    public override Action<EventData> EventReceiver => OnAbilityLearned;

    public AbilityLearnedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnAbilityLearned(EventData data) {

        if (ParentAbility == null) {
            Debug.LogError("an Ability Equipped trigger cannot resolve because it has no parent ability. Source: " + SourceEntity.EntityName);
            return;
        }

        Ability triggeringAbility = data.GetAbility("Ability");

        TriggeringAbility = triggeringAbility;
        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;



        //AbilityTriggerInstance triggerInstance = new AbilityTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, triggeringAbility, ParentAbility, triggeringAbility);

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = TriggeringAbility;
        triggerInstance.CausingAbility = TriggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;

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

        //Debug.Log(triggeringAbility.Data.abilityName + " is the ability being equipped. " + ParentAbility.Data.abilityName + " is the parent");


        if (triggeringAbility != ParentAbility) {

            //Debug.LogWarning("Missmatched abilities");
            return;
        }

        TriggeringAbility = triggeringAbility;
        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        //AbilityTriggerInstance triggerInstance = new AbilityTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, triggeringAbility, ParentAbility, triggeringAbility);

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = TriggeringAbility;
        triggerInstance.CausingAbility = TriggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;

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

        //Debug.Log(triggeringAbility.Data.abilityName + " is the ability being UNequipped. " + ParentAbility.Data.abilityName + " is the parent");


        if (triggeringAbility != ParentAbility) {
            //Debug.LogWarning("Missmatched abilities");
            return;
        }

        TriggeringAbility = triggeringAbility;
        TriggeringEntity = SourceEntity;
        CauseOfTrigger = SourceEntity;

        //AbilityTriggerInstance triggerInstance = new AbilityTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, triggeringAbility, ParentAbility, triggeringAbility);

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.TriggeringAbility = TriggeringAbility;
        triggerInstance.CausingAbility = TriggeringAbility;
        triggerInstance.SourceAbility = ParentAbility;

        TryActivateTrigger(triggerInstance);

    }
}

public class ProjectileCreatedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.ProjectileCreated;

    public override GameEvent TargetEvent => GameEvent.ProjectileCreated;

    public override Action<EventData> EventReceiver => OnProjectileCreated;

    public ProjectileCreatedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnProjectileCreated(EventData data) {

        Effect causingEffect = data.GetEffect("Parent Effect");
        Ability causingAbility = data.GetAbility("Parent Ability");
        Entity triggeringEntity = data.GetEntity("Projectile");

        TriggeringEntity = triggeringEntity;


        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.CausingEffect = causingEffect;
        triggerInstance.CausingAbility = causingAbility;
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
        Ability ability = data.GetAbility("Ability Cause");

        TriggeringEntity = victim;
        CauseOfTrigger = killer;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.CausingAbility = ability;
        TryActivateTrigger(triggerInstance);

    }
}

public class UnitDiedWithStatusTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.UnitDiedWithStatus;
    public override GameEvent TargetEvent => GameEvent.UnitDiedWithStatus;
    public override Action<EventData> EventReceiver => OnUnitDiedWithStatus;

    public UnitDiedWithStatusTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    public void OnUnitDiedWithStatus(EventData data) {
        Entity victim = data.GetEntity("Victim");
        Entity killer = data.GetEntity("Killer");
        Ability ability = data.GetAbility("Ability");
        Effect triggeringEffect = data.GetEffect("Effect");

        TriggeringEntity = victim;
        CauseOfTrigger = killer;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        triggerInstance.CausingAbility = ability;
        triggerInstance.TriggeringEffect = triggeringEffect;
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
        Entity causeOfChange = data.GetEntity("Source");
        Entity delivery = data.GetEntity("Delivery");
        float changeValue = data.GetFloat("Value");
        Ability ability = data.GetAbility("Ability");
        bool isRemoval = data.GetBool("Removal");

        TriggeringEntity = affectedTarget;
        CauseOfTrigger = causeOfChange;
        CauseOfAbilityTrigger = ability;


        //if(ParentAbility != null && ParentAbility.Source.ownerType == OwnerConstraintType.Friendly) {
        //    string cause = causeOfChange != null ? causeOfChange.EntityName : "null entity";
        //    Debug.Log(affectedTarget.gameObject.name + " had a stat change: " + targetStat + " :: " + changeValue + " caused by: " + cause);

        //    string abilityCause = ability != null ? ability.Data.abilityName : "Null ability";
        //    Debug.Log("Ability Cause: " + abilityCause);
        //}



        StatChangeTriggerInstance triggerInstance = new StatChangeTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, targetStat, changeValue, CauseOfAbilityTrigger, delivery, isRemoval);
        triggerInstance.CausingAbility = CauseOfAbilityTrigger;
        triggerInstance.SourceAbility = ParentAbility;
        triggerInstance.TriggeringAbility = ParentAbility;

        TryActivateTrigger(triggerInstance);
    }

    public class StatChangeTriggerInstance : TriggerInstance {
        public StatName targetStat;
        public float changeValue;
        public Ability causingAbility;
        public Entity delivery;
        public bool removal;

        public StatChangeTriggerInstance(Entity trigger, Entity cause, TriggerType type, StatName targetStat, float changeValue, Ability causingAbility, Entity delivery, bool removal) : base(trigger, cause, type) {
            this.targetStat = targetStat;
            this.changeValue = changeValue;
            this.causingAbility = causingAbility;
            this.delivery = delivery;
            this.removal = removal;
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

public class ProjectileDetectedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.ProjectileDetected;

    public override GameEvent TargetEvent => GameEvent.ProjectileDetected;

    public override Action<EventData> EventReceiver => OnProjectileDetected;

    public ProjectileDetectedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnProjectileDetected(EventData data) {
        
        Entity projectile = data.GetEntity("Projectile");
        Entity detector = data.GetEntity("Detector");



        TriggeringEntity = projectile;
        if (detector != null)
            CauseOfTrigger = detector;
        else
            CauseOfTrigger = SourceEntity;

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        TryActivateTrigger(triggerInstance);
    }
}

public class EntitySpawnedTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.EntitySpawned;

    public override GameEvent TargetEvent => GameEvent.EntitySpawned;

    public override Action<EventData> EventReceiver => OnEntitySpawned;

    public EntitySpawnedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
    }

    private void OnEntitySpawned(EventData data) {
        Entity target = data.GetEntity("Entity");
        Ability cause = data.GetAbility("Cause");

        TriggeringEntity = target;
        if (cause != null) {
            CauseOfAbilityTrigger = cause;
            CauseOfTrigger = cause.Source;
        }
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

        if (owner == null) {
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

    public float TriggerInterval { get { return myTimer.Duration; } }
    
    private Timer myTimer;

    private NPC aiOwner;

    public TimedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        SetupTriggerTimer();

        aiOwner = SourceEntity as NPC;

        //if(aiOwner != null) {
        //    EventManager.RegisterListener(GameEvent.StateEntered, OnStateEntered);
        //}
    }

    public override void TearDown() {
        base.TearDown();

        TimerManager.RemoveTimerAction(UpdateClock);
        //EventManager.RemoveMyListeners(this);
    }


    private void OnStateEntered(EventData data) {
        string stateName = data.GetString("State");
        if (stateName == AIState)
            ResetClock();
    }

    private void SetupTriggerTimer() {
        //TimerManager.AddTimer(this, Data.triggerTimerDuration, true);

        if(Data.autoActivateTimer == true) {
            ParentAbility.SetActive(true);
        }

        EventData data = new EventData();
        data.AddTrigger("Trigger", this);
        data.AddEntity("Owner", SourceEntity);

        myTimer = new Timer(Data.triggerTimerDuration, OnTriggerTimerCompleted, true, data);
        TimerManager.AddTimerAction(UpdateClock);

        if(Data.resetTimerOnParentAbilityEnd == true) {
            EventManager.RegisterListener(GameEvent.AbilityEnded, OnParentAbilityEnded);
        }
    }

    public void ResetClock() {

        //Debug.Log("Resetting clock for state: " + AIState);

        myTimer.ResetTimer();
    }

    private void OnParentAbilityEnded(EventData data) {
        Ability parent = data.GetAbility("Ability");

        if (parent != ParentAbility)
            return;

        ResetClock();
        ParentAbility.SetActive(true);
    }


    private void UpdateClock() {
        if (ParentAbility != null && ParentAbility.IsEquipped == false)
            return;

        if (ParentAbility != null && ParentAbility.IsActive == false)
            return;

        if (CheckAIState() == true)
            return;

        //Debug.Log("Updating a trigger timer for: " + SourceEntity.EntityName);

        if (myTimer != null)
            myTimer.UpdateClock();
    }

    private bool CheckAIState() {
        if (aiOwner == null) {
            //Debug.Log("No ai owner");
            return false;
        }

        if (string.IsNullOrEmpty(AIState) == true) {
            return false;
        }

        if (aiOwner.Brain.CurrentStateName != AIState) {
            //Debug.Log("Wrong state: Current:" + aiOwner.Brain.CurrentStateName + ". Target: " + AIState);
            //if (ParentAbility != null) {
            //    Debug.Log("Parent Ability: " + ParentAbility.Data.abilityName);
            //}
            return true;
        }



        return false;
    }

    private void OnTriggerTimerCompleted(EventData data) {

        AbilityTrigger trigger = data.GetTrigger("Trigger");

        if (trigger != this)
            return;


        Entity owner = data.GetEntity("Owner");

        if (owner == null) {
            Debug.LogError("A null owner was passed to a Timer Trigger");
            return;
        }

        TriggeringEntity = owner;
        CauseOfTrigger = owner;

        //if (ParentAbility != null /*&& ParentAbility.Data.abilityName == "Swing Orb"*/) {
        //    Debug.LogWarning("Trigger Timer Complete");

        //}

        TriggerInstance triggerInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);

        TryActivateTrigger(triggerInstance);
    }




}

public class RiderTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.Rider;

    public override GameEvent TargetEvent => GameEvent.EffectApplied;

    public override Action<EventData> EventReceiver => OnEffectApplied;

    private List<Entity> ridereffectTargets = new List<Entity>();

    public RiderTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    private void OnEffectApplied(EventData data) {

        Effect targetEffect = data.GetEffect("Effect");
        Effect parentEffect = data.GetEffect("Parent Effect");

        Ability targetAbility = SourceEntity.GetAbilityByName(Data.riderAbilityName);



        if (targetEffect == null) {
            Debug.LogError("The target Effect: " + Data.riderEffectName + " was not found for " + ParentAbility.Data.abilityName + " On " + SourceEntity.EntityName);
            return;
        }

        if (targetAbility == null) {
            Debug.LogError("The target Ability: " + Data.riderAbilityName + " was not found for " + ParentAbility.Data.abilityName + " On " + SourceEntity.EntityName);
            return;
        }

        Effect matchingEffect = targetAbility.GetEffectByName(Data.riderEffectName);

        bool foundMatch = targetEffect == matchingEffect;

        if (parentEffect == null && foundMatch == false) {
            //Debug.LogWarning("No Parent Effect found and the target effect: " + targetEffect.Data.effectName + " does not match the rider effect: " + Data.riderEffectName);
            return;
        }

        if (parentEffect != null) {

            if (parentEffect.Data.effectName != Data.riderEffectName) {
                Debug.LogWarning("Parent Effect Name: " + parentEffect.Data.effectName);
                Debug.Log("Does not match");
                Debug.LogWarning("Rider Effect Name: " + Data.riderEffectName);

                //Debug.LogWarning("Parent effect found and the target effect: " + targetEffect.Data.effectName + " does not match the rider effect: " + Data.riderEffectName);
                return;
            }
        }

        if (targetEffect.EntityTargets.Count > 0) {
            ridereffectTargets = targetEffect.ValidTargets;
            TriggeringEntity = targetEffect.LastTarget;
        }

        CauseOfTrigger = targetEffect.Source;

        //Debug.Log("A rider ability: " + ParentAbility.Data.abilityName + " is trying to trigger");


        RiderTriggerInstance triggerInstance = new RiderTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, ridereffectTargets);

        triggerInstance.TriggeringEffect = targetEffect;
        triggerInstance.TriggeringAbility = targetEffect.ParentAbility;

        TryActivateTrigger(triggerInstance);
    }

    public class RiderTriggerInstance : TriggerInstance {

        public List<Entity> RiderEffectTargets { get; private set; }

        public RiderTriggerInstance(
            Entity trigger,
            Entity cause,
            TriggerType type,
            List<Entity> riderEffectTargets) : base(trigger, cause, type) {
            this.RiderEffectTargets = riderEffectTargets;
        }
    }
}


public class ChainTrigger : AbilityTrigger {

    public override TriggerType Type => TriggerType.Chain;

    public override GameEvent TargetEvent => GameEvent.EffectApplied;

    public override Action<EventData> EventReceiver => OnEffectApplied;

    private List<Entity> ridereffectTargets = new List<Entity>();

    private int chainCount;

    public ChainTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {

    }

    private void OnEffectApplied(EventData data) {

        if(chainCount >= ParentAbility.Stats[StatName.ProjectileChainCount]) {
            chainCount = 0;
            return;
        }

        Effect targetEffect = data.GetEffect("Effect");
        Effect matchingEffect = ParentAbility.GetEffectByName(targetEffect.Data.effectName);

        if (matchingEffect == null) {
            //Debug.LogError("No effect named: " + targetEffect.Data.effectName + " was not found for " + ParentAbility.Data.abilityName + " On " + SourceEntity.EntityName);
            return;
        }

        if(matchingEffect != targetEffect) {
            Debug.LogError("Effects don't match: " + targetEffect.Data.effectName + " Ability: " + ParentAbility.Data.abilityName + " On " + SourceEntity.EntityName);
            return;
        }


        TriggeringEntity = targetEffect.LastTarget;
        CauseOfTrigger = targetEffect.Source;

        TriggerInstance activationInstance = new TriggerInstance(TriggeringEntity, CauseOfTrigger, Type);
        activationInstance.TriggeringEffect = targetEffect;


        new Task(ActivateOnDelay(activationInstance));
    }

    private IEnumerator ActivateOnDelay(TriggerInstance activationInstance) {
        WaitForSeconds waiter = new WaitForSeconds( Data.chainTriggerDelay);
        yield return waiter;
        chainCount++;
        TryActivateTrigger(activationInstance);
    }

    
}




