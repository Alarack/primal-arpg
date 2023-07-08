using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriggerInstance = AbilityTrigger.TriggerInstance;


public abstract class AbilityRecovery {
    public abstract RecoveryType Type { get; }
    public Entity Source { get; protected set; }
    public Ability ParentAbility { get; protected set; }
    public RecoveryData Data { get; protected set; }

    protected List<AbilityTrigger> recoveryTriggers = new List<AbilityTrigger>();
    protected TriggerActivationCounter activationCounter;

    #region CONSTRUCTION
    public AbilityRecovery() {

    }

    public AbilityRecovery(RecoveryData data, Entity Source, Ability parentAbility) {
        this.Data = data;
        this.Source = Source;
        this.ParentAbility = parentAbility;

        SetupActivationTriggers();
        SetupTriggerCounters();
        RegisterEvents();
    }
    #endregion

    #region SETUP AND TEARDOWN

    protected void SetupActivationTriggers() {
        foreach (TriggerData triggerData in Data.recoveryTriggers) {

            AbilityTrigger trigger = AbilityFactory.CreateAbilityTrigger(triggerData, Source);
            trigger.ActivationCallback = ReceiveStartActivationInstance;
            recoveryTriggers.Add(trigger);
        }
    }

    protected void SetupTriggerCounters() {
        if (Data.counterData.limitedNumberOfTriggers == true || Data.counterData.requireMultipleTriggers == true)
            activationCounter = new TriggerActivationCounter(Data.counterData, Source);
        else
            activationCounter = null;
    }


    protected virtual void RegisterEvents() {

    }


    public virtual void TearDown() {
        EventManager.RemoveMyListeners(this);

        for (int i = 0; i < recoveryTriggers.Count; i++) {
            recoveryTriggers[i].TearDown();
        }

        if (activationCounter != null)
            activationCounter.TearDown();
    }

    #endregion

    #region ACTIVATION

    public void ReceiveStartActivationInstance(TriggerInstance activationInstance) {

        if (activationCounter != null && activationCounter.Evaluate() == false) {
            return;
        }

        ParentAbility.RecoveryCharge(1);

    }

    #endregion

}


public class AbilityRecoveryCooldown : AbilityRecovery {
    public override RecoveryType Type => RecoveryType.Timed;

    private Timer cooldownTimer;

    public float Ratio { get { return cooldownTimer.Ratio; } }
    //public float Cooldown { get { return Stats[StatName.Cooldown]; } }

    //public StatCollection Stats { get; private set; } 

    public AbilityRecoveryCooldown(RecoveryData data, Entity Source, Ability parentAbility) : base(data, Source, parentAbility) {
        //Stats = new StatCollection(this);
        //Stats.AddStat(new SimpleStat(StatName.Cooldown, data.cooldown));
        //Stats.AddStatListener(StatName.Cooldown, OnCooldownChanged);


        parentAbility.Stats.AddStat(new SimpleStat(StatName.Cooldown, data.cooldown));
        parentAbility.Stats.AddStatListener(StatName.Cooldown, OnCooldownChanged);


        cooldownTimer = new Timer(parentAbility.Stats[StatName.Cooldown], OnCooldownComplete, true);
        TimerManager.AddTimerAction(Recover);
    }

    protected override void RegisterEvents() {
        base.RegisterEvents();

        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnSourceCDRChanged);
    }


    private void OnSourceCDRChanged(EventData data) {
        StatName stat = (StatName)data.GetInt("Stat");
        Entity target = data.GetEntity("Target");
        Entity source = data.GetEntity("Source");

        if (stat != StatName.CooldownReduction || target != Source)
            return;

        float cooldownReduction = Source.Stats[StatName.CooldownReduction];

        ParentAbility.Stats.RemoveAllModifiersFromSource(this);


        float cdrPercent =  cooldownReduction;

        if (cdrPercent <= 0)
            return;

        StatModifier cdrMod = new StatModifier(-cdrPercent, StatModType.PercentAdd, StatName.Cooldown, this, StatModifierData.StatVariantTarget.Simple);


        ParentAbility.Stats.AddModifier(StatName.Cooldown, cdrMod);
        //Debug.Log("Cooldown Reduction changed. CDR: " + -cdrPercent + ". My Cooldown: " + cooldownStats[StatName.Cooldown]);
        //Debug.Log("Modifier Value: " + -cdrPercent);


    }

    private void OnCooldownComplete(EventData data) {
        ReceiveStartActivationInstance(null);
    }

    public void Recover() {
        if(ParentAbility.IsEquipped == false) 
            return;   

        if (ParentAbility.Charges < ParentAbility.MaxCharges) {
            cooldownTimer.UpdateClock();
            //Debug.Log("Updating a cooldown");
        }
    }

    public override void TearDown() {
        base.TearDown();
        
        TimerManager.RemoveTimerAction(Recover);
        ParentAbility.Stats.RemoveStatListener(StatName.Cooldown, OnCooldownChanged);
    }

    //public void AddCooldownModifier(StatModifier mod) {
    //    Stats.AddModifier(StatName.Cooldown, mod);
    //}

    //public void RemoveCooldownModifier(StatModifier mod) {
    //    Stats.RemoveModifier(StatName.Cooldown, mod);
    //}

    private void OnCooldownChanged(BaseStat stat, object source, float value) {

        cooldownTimer.SetDuration(ParentAbility.Stats[StatName.Cooldown]);

        //Debug.Log("A cooldown changed: " + ParentAbility.Data.abilityName + ". Cooldown Timer Duration: " + cooldownTimer.Duration);
        //Debug.Log("Stat Value: " + cooldownStats[StatName.Cooldown]);

    }

}
