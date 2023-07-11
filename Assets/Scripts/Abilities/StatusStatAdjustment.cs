using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;

public class StatusStatAdjustment /*: Status*/ {


    //private List<StatModifier> activeMods = new List<StatModifier>();

    //public StatusStatAdjustment(StatusData data, Entity target, Entity source, EffectData effectData) : base(data, target, source, effectData) {
    //    CreateStatMods();
    //}

    //private void CreateStatMods() {
    //    activeMods.Clear();

    //    for (int i = 0; i < Data.statModifiers.Count; i++) {
    //        StatModifierData data = Data.statModifiers[i];

    //        StatModifier mod = new StatModifier(data.value, data.modifierType, data.targetStat, Source, data.variantTarget);
    //        activeMods.Add(mod);
    //    }
    //}

    //public override void FirstApply() {
    //    base.FirstApply();
    //}

    //public override void Stack() {
    //    base.Stack();

    //    if (intervalTimer == null) {
    //        ApplyStatModifiers();
    //    }


    //}

    //protected override void Tick(EventData timerEventData) {
    //    base.Tick(timerEventData);

    //    ApplyStatModifiers();
    //}

    //protected override void CleanUp(EventData timerEventData) {
    //    base.CleanUp(timerEventData);

    //    RemoveStatModifiers();
    //}

    //private void ApplyStatModifiers() {

    //    for (int i = 0; i < activeMods.Count; i++) {

    //        bool nonRange = activeMods[i].VariantTarget != StatModifierData.StatVariantTarget.RangeCurrent;
    //        if (nonRange) {
    //            Debug.LogWarning("A status belonging to: " + Source.EntityName + " is applying a non-range-curent stat adjustment to: " + Target.EntityName);
    //            Debug.LogWarning("This is not supported and will not remove properly.");
    //        }


    //        float multiplier = Data.multiplyByStackCount == true ? StackCount : 1f;

    //        //float modResult = StatAdjustmentManager.ApplyStatAdjustment(Target, Data.statModifiers[i], Source, multiplier);

    //        float modResult = StatAdjustmentManager.ApplyStatAdjustment(Target, activeMods[i], activeMods[i].VariantTarget, Source, multiplier);

    //        CreateFloatingText(modResult, activeMods[i].TargetStat);


    //    }
    //}

    //private void CreateFloatingText(float modValue, StatName stat) {

    //    if (Target == null)
    //        return;

    //    if (stat == StatName.Health) {
    //        FloatingTextManager.SpawnFloatingText(Target.transform.position, modValue.ToString());
    //    }

    //    if (stat == StatName.MoveSpeed) {
    //        FloatingTextManager.SpawnFloatingText(Target.transform.position, "Slowed");
    //    }
    //}

    //private void RemoveStatModifiers() {
    //    //Target.Stats.RemoveAllModifiersFromSource(this);

    //    for (int i = 0; i < activeMods.Count; i++) {
    //        StatAdjustmentManager.RemoveStatAdjustment(Target, activeMods[i], activeMods[i].VariantTarget, Source);
    //    }
    //}

}
