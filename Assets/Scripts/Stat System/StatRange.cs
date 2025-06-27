using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatRange : BaseStat {

    public float Ratio { get { return GetRatio(); } }

    public SimpleStat MinValueStat { get; protected set; }
    public SimpleStat MaxValueStat { get; protected set; }
    
    private float currentValue;

    public override float ModifiedValue => currentValue;


    public StatRange(StatName name, float minValue, float maxValue, float currentValue = 0f) : base(name) {
        if (minValue > maxValue) {
            Debug.LogError("Tried to make a StatRange with a higher min than max. This is not supported");
        }


        this.MinValueStat = new SimpleStat(name, minValue);
        this.MaxValueStat = new SimpleStat(name, maxValue);
        this.currentValue = currentValue;
    }

    public void AdjustValueFlat(float adjustment, object source) {
        if (CheckMinMax(adjustment) == true)
            return;

        float valueCorrection = CheckBoundsFlat(currentValue + adjustment, adjustment);
        currentValue += valueCorrection;

        onValueChanged?.Invoke(this, source, adjustment);
    }

    public void AdjustValuePercentage(float adjustment, object source) {

        float valueCorrection = CheckBoundsPercentage(adjustment);

        if (CheckMinMax(valueCorrection) == true)
            return;

        currentValue += valueCorrection;

        onValueChanged?.Invoke(this, source, adjustment);
    }

    public void AdjustValueByPercentOfMax(float adjustment, object source) {
        float resultingAdjustment = (MaxValueStat.ModifiedValue * (1f + adjustment)) - MaxValueStat.ModifiedValue;
        AdjustValueFlat(resultingAdjustment, source);
    }

    public void AdjustValueByPercentOfCurrent(float adjustment, object source) {
        float resultingAdjustment = (currentValue * (1f + adjustment)) - currentValue;
        AdjustValueFlat(resultingAdjustment, source);
    }

    public float Refresh(object source) {
        float difference = MaxValueStat.ModifiedValue - currentValue;
        AdjustValueFlat(difference, source);

        return difference;
    }

    public void Empty(object source) {
        AdjustValueFlat(-currentValue, source);
    }


    #region MODIFY MIN AND MAX

    public void AddMinModifier(StatModifier mod) {
        MinValueStat.AddModifier(mod);
        CheckBounds();
    }

    public void AddMinModifier(float value, StatModType modType, object source) {
        MinValueStat.AddModifier(value, modType, source);
        CheckBounds();
    }
    public void AddMaxModifier(StatModifier mod) {
        MaxValueStat.AddModifier(mod);
        CheckBounds();
    }

    public void AddMaxModifier(float value, StatModType modType, object source) {
        MaxValueStat.AddModifier(value, modType, source);
        CheckBounds();
    }

    public void RemoveMinModifier(StatModifier mod) {
        MinValueStat.RemoveModifier(mod);
        CheckBounds();
    }

    public void RemoveMaxModifier(StatModifier mod) {
        MaxValueStat.RemoveModifier(mod);
        CheckBounds();
    }

    public void RemoveAllMaxModifiersFromSource(object source) {
        MaxValueStat.RemoveAllModifiersFromSource(source);
        CheckBounds();
    }

    public void RemoveAllMinModifiersFromSource(object source) {
        MinValueStat.RemoveAllModifiersFromSource(source);
        CheckBounds();
    }

    public override void RemoveAllModifiersFromSource(object source) {
        RemoveAllMinModifiersFromSource(source);
        RemoveAllMaxModifiersFromSource(source);
    }

    public void HardReset(float currentValue = 0f) {
        MinValueStat.HardReset();
        MaxValueStat.HardReset();
        this.currentValue = currentValue;
        CheckBounds();
    }


    #endregion

    #region BOUNDS CHECKS

    private bool CheckMinMax(float adjustment) {
        if (currentValue == MaxValueStat.ModifiedValue && adjustment > 0)
            return true;

        if (currentValue == MinValueStat.ModifiedValue && adjustment < 0)
            return true;

        return false;
    }

    private void CheckBounds() {
        if (currentValue < MinValueStat.ModifiedValue) {
            currentValue = MinValueStat.ModifiedValue;
            float difference = -currentValue;
            onValueChanged?.Invoke(this, this, difference);
        }

        if (currentValue > MaxValueStat.ModifiedValue) {
            currentValue = MaxValueStat.ModifiedValue;
            float difference = MaxValueStat.ModifiedValue - currentValue;
            onValueChanged?.Invoke(this, this, difference);
        }

    }

    private float CheckBoundsPercentage(float adjustment) {
        float resultingValue = currentValue * (adjustment + 1f);

        if (resultingValue < MinValueStat.ModifiedValue) {
            float differenceToMin = Mathf.Abs(MinValueStat.ModifiedValue) + currentValue;
            return -differenceToMin;
        }


        if (resultingValue > MaxValueStat.ModifiedValue) {
            float difference = MaxValueStat.ModifiedValue - currentValue;
            return difference;
        }

        return (currentValue * (adjustment + 1f)) - currentValue;
    }

    private float CheckBoundsFlat(float resultingValue, float adjustment) {

        if (resultingValue < MinValueStat.ModifiedValue) {
            float differenceToMin = Mathf.Abs(MinValueStat.ModifiedValue) + currentValue;
            return -differenceToMin;
        }

        if (resultingValue > MaxValueStat.ModifiedValue) {
            float difference = MaxValueStat.ModifiedValue - currentValue;
            return difference;
        }


        return adjustment;
    }

    #endregion

    #region HELPERS

    private float GetRatio() {
        if (MaxValueStat.ModifiedValue < 0f) {
            float maxV = MaxValueStat.ModifiedValue;
            float minV = MinValueStat.ModifiedValue;
            float correctMax = maxV + Mathf.Abs(minV);
            float correctCurrent = currentValue + Mathf.Abs(minV);

            return MathF.Round(correctCurrent / correctMax, 2);
        }

        return MathF.Round(currentValue / MaxValueStat.ModifiedValue, 2);
    }

    #endregion
}
