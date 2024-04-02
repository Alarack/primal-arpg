using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        Vulnerable
    }

    #endregion

    public StatusName statusName;
    public StackMethod stackMethod;


    public bool IsStackCapped { get { return MaxStacks > 0 && StackCount == MaxStacks; } }
    public int StackCount { get; protected set; }
    public int MaxStacks { get; protected set; }
    public Entity Target { get; protected set; }
    public Entity Source { get; protected set; }

    protected Timer durationTimer;
    protected Timer intervalTimer;
    protected GameObject activeVFX;

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
    }

    private void RegisterEvents() {
        EventManager.RegisterListener(GameEvent.UnitDied, OnUnitDied);
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

        if (totalDuration > 0f)
            durationTimer = new Timer(totalDuration, CleanUp, false);

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
    }

    public virtual void FirstApply() {

        Target.AddStatus(this);

        CreateVFX();
        Tick(null);
    }

    protected void CreateVFX() {
        if (Data.VFXPrefab == null)
            return;

        activeVFX = VFXUtility.SpawnVFX(Data.VFXPrefab, Target.transform, 0f, Data.vfxScaleModifier);

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

    public virtual void Remove() {
        CleanUp(null);
    }

    protected virtual void CleanUp(EventData timerEventData) {
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

}
