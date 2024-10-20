using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public class Status {
    #region ENUMS

    public enum StackMethod {
        None,
        LimitedStacks,
        Infinite
    }

    public enum StatusName {
        None,
        Burning,
        Slowed,
        Immobilized,
        BladeDash,
        Grow,
        IronReign,
        SpellHaste,
        Enraged,
        Armored,
        Vulnerable,
        StealTime,
        AlteredTimelineBuff,
        EssenceSiphonCDR,
        RisingTide,
        EssenceGreed,
        MoveHaste
    }

    #endregion

    public StatusName statusName;
    public StackMethod stackMethod;


    public bool IsStackCapped { get { return MaxStacks > 0 && StackCount == MaxStacks; } }
    public bool IsDot { get { return IsDOT(); } }
    public int StackCount { get; protected set; }
    public int MaxStacks { get; protected set; }
    public Entity Target { get; protected set; }
    public Entity Source { get; protected set; }

    protected Timer durationTimer;
    protected Timer intervalTimer;
    protected GameObject activeVFX;
    private bool sendRemoveEvent;

    public StatusData Data { get; protected set; }
    public Effect ActiveEffect { get; protected set; }
    public AddStatusEffect ParentEffect { get; protected set; }

    public Status(StatusData data, Entity target, Entity source, Effect activeEffect, AddStatusEffect ParentEffect) {
        this.Data = data;
        this.ParentEffect = ParentEffect;

        this.statusName = data.statusName;
        this.stackMethod = data.stackMethod;

        this.MaxStacks = (int)ParentEffect.Stats.GetStatRangeMaxValue(StatName.StackCount);
        this.StackCount = (int)ParentEffect.Stats[StatName.StackCount];

        this.Target = target;
        this.Source = source;

        RegisterEvents();

        CreateTimers();
        //CreateEffect(activeEffect);
        ActiveEffect = activeEffect;

        TimerManager.AddTimerAction(ManagedUpdate);

        FirstApply();

        EventData statusEventData = new EventData();
        statusEventData.AddEntity("Target", target);
        statusEventData.AddEntity("Cause", source);
        statusEventData.AddStatus("Status", this);
        statusEventData.AddAbility("Causing Ability", ParentEffect.ParentAbility);
        statusEventData.AddEffect("Causing Effect", ParentEffect);

        EventManager.SendEvent(GameEvent.StatusApplied, statusEventData);

        //Debug.Log("Status Class: " + data.statusName + " applied to: " + target.EntityName + " from " + source.EntityName);
    }

    private void RegisterEvents() {
        EventManager.RegisterListener(GameEvent.UnitDied, OnUnitDied);
        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatusDurationChanged);
    }


    private void OnStatusDurationChanged(EventData data) {
        StatName stat = (StatName)data.GetInt("Stat");
        Entity target = data.GetEntity("Target");

        if(target != Source) {
            return;
        }

        if (stat == StatName.GlobalStatusDurationModifier) {
            CreateTimers();
        }

        
    }

    private void OnUnitDied(EventData data) {
        Entity target = data.GetEntity("Victim");
        Entity killer = data.GetEntity("Killer");

        if (target == Target) {
            //Debug.LogWarning(Target.EntityName + " was killed while affected by a status: " + statusName);
            ParentEffect.OnAffectedTargetDies(target, killer, this);

            Remove();
        }
    }

    private void CreateTimers() {

        float totalDuration = ParentEffect.GetModifiedStatusDuration();
        float totalInterval = ParentEffect.GetModifiedIntervalDuration();

        //Debug.Log("Total duration of a status " + Data.statusName + " on " + ParentEffect.Data.effectName + " is " + totalDuration);



        if (totalDuration > 0f) {
            sendRemoveEvent = true;
            durationTimer = new Timer(totalDuration, CleanUp, false);
        }

        if (totalInterval > 0f)
            intervalTimer = new Timer(totalInterval, Tick, true);
    }

    public void ForceTick() {
        Tick(null);
    }

    protected virtual void Tick(EventData timerEventData) {
        if (Target == null) {
            //Debug.LogWarning("A target with a status is null. Removing status");
            Remove();
            return;
        }

        if (ActiveEffect == null) {
            Debug.LogError("An active effect on the status belonging to: " + ParentEffect.Data.effectName + " is null");
            return;
        }

        ActiveEffect.Apply(Target);
        CheckDoubleTick();
    }

    private void CheckDoubleTick() {
        float doubleTickStat = ParentEffect.ParentAbility.Stats[StatName.DoubleTickChance] + ParentEffect.Source.Stats[StatName.DoubleTickChance];

        if (doubleTickStat <= 0f)
            return;

        float roll = UnityEngine.Random.Range(0f, 1f);

        if(roll <= doubleTickStat) {
            ActiveEffect.Apply(Target);
        }
    }

    public virtual void FirstApply() {

        Target.AddStatus(this);

        CreateVFX();
        Tick(null);
    }

    protected void CreateVFX() {
        if (Data.VFXPrefab == null)
            return;
        //Debug.Log("Spawing VFX for Status: " +  Data.VFXPrefab.name);
        activeVFX = VFXUtility.SpawnVFX(Data.VFXPrefab, Target.GetStatusVFXPosition(), 0f, Data.vfxScaleModifier);

        //activeVFX = GameObject.Instantiate(Data.VFXPrefab, Target.transform);
        //activeVFX.transform.localPosition = Vector3.zero;
    }

    public virtual void Stack() {
        RefreshDuration();

        switch (stackMethod) {
            case StackMethod.None:
                return;
            case StackMethod.LimitedStacks:
                if (IsStackCapped == true) {
                    //Debug.LogWarning("Max stack reached");
                    return;
                }
                break;
        }

        StackCount++;
        ActiveEffect.Stack(this);

        //Debug.Log("Stacking: " + ActiveEffect.Data.effectName + " Count: " + StackCount);
        EventData statusEventData = new EventData();
        statusEventData.AddEntity("Target", Target);
        statusEventData.AddEntity("Cause", Source);
        statusEventData.AddStatus("Status", this);
        statusEventData.AddAbility("Causing Ability", ParentEffect.ParentAbility);
        statusEventData.AddEffect("Causing Effect", ParentEffect);

        EventManager.SendEvent(GameEvent.StatusStacked, statusEventData);
    }

    //public void ReApply() {
    //    if (Target == null) {
    //        //Debug.LogWarning("A target with a status is null. Removing status");
    //        Remove();
    //        return;
    //    }

    //    ParentEffect.Remove(Target);
    //    ForceTick();
    //}

    public virtual void Remove(bool sendRemoveEvent = false) {
        this.sendRemoveEvent = sendRemoveEvent;

        
        CleanUp(null);
    }

    private void SendStatusRemovedEvent() {
        EventData statusEventData = new EventData();
        statusEventData.AddEntity("Target", Target);
        statusEventData.AddEntity("Cause", Source);
        statusEventData.AddStatus("Status", this);
        statusEventData.AddAbility("Causing Ability", ParentEffect.ParentAbility);
        statusEventData.AddEffect("Causing Effect", ParentEffect);

        EventManager.SendEvent(GameEvent.StatusRemoved, statusEventData);
    }

    protected virtual void CleanUp(EventData timerEventData) {

        //Debug.LogWarning("Cleaning up: " + Data.statusName + " event: " + sendRemoveEvent);

        if(sendRemoveEvent == true) {
            SendStatusRemovedEvent();
        }
        
        
        TimerManager.RemoveTimerAction(ManagedUpdate);
        ParentEffect.CleanUp(Target, ActiveEffect);

        if (ActiveEffect != null) {
            ActiveEffect.Remove(Target);
            ActiveEffect = null;
        }


        EventManager.RemoveMyListeners(this);

        if (Target != null)
            Target.RemoveStatus(this);

        if (activeVFX == null)
            return;

        ParticleSystem particles = activeVFX.GetComponentInChildren<ParticleSystem>();

        if (particles != null) {
            particles.Stop();
            GameObject.Destroy(activeVFX, particles.main.duration);
        }
        else {
            GameObject.Destroy(activeVFX, 2f);
        }

    }


    public virtual void RefreshDuration() {
        if (durationTimer != null)
            durationTimer.ResetTimer();
    }

    public virtual void ModifyIntervalTime(float mod) {
        if (intervalTimer != null)
            intervalTimer.ModifyDuration(mod);
    }

    public virtual void ModifyDuration(float mod) {
        if (durationTimer != null)
            durationTimer.ModifyDuration(mod);
    }

    public virtual void ManagedUpdate() {

        if (ActiveEffect == null) {
            Debug.LogError("An active effect of a status: " + ParentEffect.Data.effectName + " is null during managerd update");
            return;
        }

        if (durationTimer != null)
            durationTimer.UpdateClock();

        if (intervalTimer != null)
            intervalTimer.UpdateClock();
    }


    private bool IsDOT() {
        return ParentEffect.ParentAbility.Tags.Contains(AbilityTag.DamageOverTime);
    }

}
