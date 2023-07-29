using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;
using LL.FSM;
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

    protected bool CheckMultiTypeConstraints(TriggerInstance activationInstance) {

        foreach (var entry in constraintDict) {
            Tuple<Entity, Ability, Effect> foci = GetConstraintFocus(entry.Key, activationInstance);

            if(foci == null) {
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
                            //Debug.Log("an ability: " + ParentAbility.Data.abilityName + " failed an entity constraint");
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


        if(CheckMultiTypeConstraints(activationInstance) == false) 
            return false;


        return true;
    }

    /// <summary>
    /// Attempt to Activate an ability after receving an event
    /// </summary>
    protected void TryActivateTrigger(TriggerInstance activationInstance) {
        if(Data.delay1Frame == true) {
            new Task(FrameDelay(activationInstance));
            return;
        }
        
        if (IsTriggerValid(activationInstance) == false) {
            return;
        }

        ActivationCallback?.Invoke(activationInstance);
    }

    protected IEnumerator FrameDelay(TriggerInstance activationInstance) {
        yield return new WaitForEndOfFrame();

        if (IsTriggerValid(activationInstance) == false) {
            yield break;
        }

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
      
        if(ParentAbility == null) {
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
        triggerInstance.CausingAbility= causingAbility;
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
        Entity causeOfChange = data.GetEntity("Cause");
        float changeValue = data.GetFloat("Value");
        Ability ability = data.GetAbility("Ability");

        TriggeringEntity = affectedTarget;
        CauseOfTrigger = causeOfChange;
        CauseOfAbilityTrigger = ability;

        //Debug.Log(affectedTarget.gameObject.name + " had a stat change: " + targetStat + " :: " + changeValue);


        StatChangeTriggerInstance triggerInstance = new StatChangeTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, targetStat, changeValue, CauseOfAbilityTrigger);
        triggerInstance.CausingAbility = CauseOfAbilityTrigger;
        triggerInstance.SourceAbility = ParentAbility;
        triggerInstance.TriggeringAbility = ParentAbility;
        
        TryActivateTrigger(triggerInstance);
    }

    public class StatChangeTriggerInstance : TriggerInstance {
        public StatName targetStat;
        public float changeValue;
        public Ability causingAbility;

        public StatChangeTriggerInstance(Entity trigger, Entity cause, TriggerType type, StatName targetStat, float changeValue, Ability causingAbility) : base(trigger, cause, type) {
            this.targetStat = targetStat;
            this.changeValue = changeValue;
            this.causingAbility = causingAbility;
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

    private Timer myTimer;

    public TimedTrigger(TriggerData data, Entity source, Ability parentAbility = null) : base(data, source, parentAbility) {
        SetupTriggerTimer();
    }


    private void SetupTriggerTimer() {
        //TimerManager.AddTimer(this, Data.triggerTimerDuration, true);

        EventData data = new EventData();
        data.AddTrigger("Trigger", this);
        data.AddEntity("Owner", ParentAbility.Source);

        myTimer = new Timer(Data.triggerTimerDuration, OnTriggerTimerCompleted, true, data);
        TimerManager.AddTimerAction(UpdateClock);
    }


    private void UpdateClock() {
        if (ParentAbility.IsEquipped == false)
            return;

        if (ParentAbility.IsActive == false)
            return;

        if(myTimer != null)
            myTimer.UpdateClock();
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


        //Debug.LogWarning("Trigger Timer Complete");

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

        Ability targetAbility = SourceEntity.GetAbilityByName(Data.riderAbilityName, AbilityCategory.Any);



        if (targetEffect == null) {
            Debug.LogError("The target Effect: " + Data.riderEffectName + " was not found for " + ParentAbility.Data.abilityName + " On " + SourceEntity.EntityName);
            return;
        }

        if (targetAbility == null) {
            Debug.LogError("The target Ability: " + Data.riderAbilityName + " was not found for " + ParentAbility.Data.abilityName + " On " + SourceEntity.EntityName);
            return;
        }

        if(targetEffect == targetAbility.GetEffectByName(Data.riderEffectName) == false) {

            //Debug.LogWarning("The target effect: " + targetEffect.Data.effectName + " does not match the rider effect: " + Data.riderEffectName);

            
            return;
        }

        if(targetEffect.EntityTargets.Count > 0) {
            ridereffectTargets = targetEffect.ValidTargets;
            TriggeringEntity = targetEffect.LastTarget;
        }

        CauseOfTrigger = targetEffect.Source;

        //Debug.Log("A rider ability: " + ParentAbility.Data.abilityName + " is trying to trigger");


        RiderTriggerInstance triggerInstance = new RiderTriggerInstance(TriggeringEntity, CauseOfTrigger, Type, ridereffectTargets);

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




