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
        BladeDash
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

        //this.MaxStacks = (int)ParentEffect.Stats.GetStatRangeMaxValue(StatName.StackCount);
        //StackCount = (int)ParentEffect.Stats[StatName.StackCount];

        this.Target = target;
        this.Source = source;

        CreateTimers();
        //CreateEffect(activeEffect);
        ActiveEffect = activeEffect;

        TimerManager.AddTimerAction(ManagedUpdate);

        FirstApply();
    }

    private void CreateEffect(EffectData effectData) {

        if(effectData.type == EffectType.AddStatus) {
            Debug.LogError("Status trying to create an infinite loop of statuses. Noping out.");
            return;
        }

        ActiveEffect = AbilityFactory.CreateEffect(effectData, Source);
        //ParentEffect.activeStatusEffects.Add(ActiveEffect);
    }

    private void CreateTimers() {
        if (Data.duration > 0f)
            durationTimer = new Timer(ParentEffect.GetModifiedEffectDuration(), CleanUp, false);

        if (Data.interval > 0f)
            intervalTimer = new Timer(ParentEffect.GetModifiedIntervalDuration(), Tick, true);
    }

    protected virtual void Tick(EventData timerEventData) {
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

        activeVFX = GameObject.Instantiate(Data.VFXPrefab, Target.transform);
        activeVFX.transform.localPosition = Vector3.zero;
    }

    public virtual void Stack() {
        RefreshDuration();


        switch (stackMethod) {
            case StackMethod.None:
                return;
            case StackMethod.LimitedStacks:
                if (IsStackCapped == true) {
                    Debug.LogWarning("Max stack reached");
                    return;
                }
                break;
        }

        //ParentEffect.Stats.AdjustStatRangeCurrentValue(StatName.StackCount, 1, StatModType.Flat, Source);
        StackCount++;
        ActiveEffect.Stack(this);
    }

    public virtual void Remove() {
        CleanUp(null);
    }

    protected virtual void CleanUp(EventData timerEventData) {
        //StatusManager.RemoveStatus(Target, this);
        TimerManager.RemoveTimerAction(ManagedUpdate);
        ParentEffect.CleanUp(Target, ActiveEffect);
        ActiveEffect.Remove(Target);
        ActiveEffect = null;


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
        if (durationTimer != null)
            durationTimer.UpdateClock();

        if (intervalTimer != null)
            intervalTimer.UpdateClock();
    }

}
