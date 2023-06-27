using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;

public class StatusStatAdjustment : Status
{


    //private List<StatModifier> activeMods = new List<StatModifier>();

    public StatusStatAdjustment(StatusData data, Entity target, Entity source) :base(data, target, source) {

    }

    public override void FirstApply() {
        base.FirstApply();
    }

    public override void Stack() {
        base.Stack();

        if (intervalTimer == null) {
            ApplyStatModifiers();
        }


    }

    protected override void Tick(EventData timerEventData) {
        base.Tick(timerEventData);

        ApplyStatModifiers();
    }

    protected override void CleanUp(EventData timerEventData) {
        base.CleanUp(timerEventData);

        RemoveStatModifiers();
    }

    //private void CreateModifiers() {
    //    for (int i = 0; i < Data.statModifiers.Count; i++) {
    //        StatModifierData modData = Data.statModifiers[i];

    //        StatModifier mod = new StatModifier(modData.value, modData.modifierType, Source);
    //        activeMods.Add(mod);
    //    }
    //}

    private void ApplyStatModifiers() {

        for (int i = 0; i < Data.statModifiers.Count; i++) {


            float multiplier = Data.multiplyByStackCount == true ? StackCount : 1f;

            float modResult = StatAdjustmentManager.ApplyStatAdjustment(Target, Data.statModifiers[i], Source, multiplier);

            CreateFloatingText(modResult, Data.statModifiers[i].targetStat);


        }
    }

    private void CreateFloatingText(float modValue, StatName stat) {

        if (Target == null)
            return;

        if(stat == StatName.Health) {
            FloatingTextManager.SpawnFloatingText(Target.transform.position, modValue.ToString());
        }

        if (stat == StatName.MoveSpeed) {
            FloatingTextManager.SpawnFloatingText(Target.transform.position, "Slowed");
        }
    }

    private void RemoveStatModifiers() {
        Target.Stats.RemoveAllModifiersFromSource(this);
    }

}
