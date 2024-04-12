using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using TMPro;
using System;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;

public class DPSMeter : MonoBehaviour
{

    public bool playerOnly = false;

    private TextMeshProUGUI dpsText;

    private Timer dpsTimer;
    private Timer clearTimer;
    private Timer activeTimer;

    private bool isActive;
    private float currentDamage;
    private float previousDamage;
    private float averageDamage;
    private float timeElapsed;

    private Dictionary<Ability, List<DamageEntry>> damageEntries = new Dictionary<Ability, List<DamageEntry>>();

    private void Awake() {
        dpsText = GetComponentInChildren<TextMeshProUGUI>();
        dpsTimer = new Timer(0.1f, OnTimerComplete, true);
        clearTimer = new Timer(1f, OnClockReset, true);
        activeTimer = new Timer(3f, OnActiveTimeOut, true);
    }

    private void OnEnable() {
        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatChanged);
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }

    private void Update() {
        //if (dpsTimer != null) {
        //    dpsTimer.UpdateClock();
        //}

        //if (clearTimer != null) {
        //    clearTimer.UpdateClock();
        //}

        if (activeTimer != null && isActive == true) {
            timeElapsed += Time.deltaTime;
            activeTimer.UpdateClock();

            if (timeElapsed == 0f)
                return;

            float averageDamage = Mathf.Abs(currentDamage) / timeElapsed;
            dpsText.text = "DPS: " + MathF.Round(averageDamage, 2).ToString();
        }
    }



    private void OnStatChanged(EventData data) {
        StatName stat = (StatName)data.GetInt("Stat");
        Entity target = data.GetEntity("Target");
        Entity cause = data.GetEntity("Source");
        Ability ability = data.GetAbility("Ability");
        float value = data.GetFloat("Value");

        if (stat != StatName.Health)
            return;

        if (value > 0f)
            return;

        if(cause.ownerType != OwnerConstraintType.Friendly) 
            return;

        if(playerOnly == true && cause != EntityManager.ActivePlayer) 
            return;

        ProcessDamageDealt(value, ability, DateTime.Now);

    }


    private void ProcessDamageDealt(float value, Ability ability, DateTime time) {

        currentDamage += value;
        activeTimer.ResetTimer();
        isActive = true;

        //previousDamage = currentDamage;
        //currentDamage = value;

        //averageDamage = (previousDamage + currentDamage) / 2f;

        //DamageEntry newEntry = new DamageEntry(ability, value, time);
        //LogDamageEntry(newEntry);
        //float totalDamage = GetTotalDamage();
        //dpsText.text = "DPS: " + MathF.Round(MathF.Abs(totalDamage), 2).ToString();


        //currentDamage += value;

        //float lastPrevious = previousDamage;
        //previousDamage = currentDamage;

        //averageDamage = (lastPrevious + currentDamage) / 2f;

        //dpsText.text = "DPS: " + MathF.Round(MathF.Abs(averageDamage), 2).ToString();
    }

    private float GetTotalDamage() {
        float total = 0f;
        foreach (var item in damageEntries) {
            for (int i = 0; i < item.Value.Count; i++) {
                total += item.Value[i].damageValue;
            }
        }

        return total;
    }

    private void PruneOldEntries() {

        DateTime lastSecond = DateTime.Now.AddSeconds(-1);

        for (int i = damageEntries.Count -1; i >= 0; i--) {
            KeyValuePair<Ability, List<DamageEntry>> target = damageEntries.ElementAt(i);

            for (int j = target.Value.Count -1; j >=0 ; j--) {
                DamageEntry damageEntry = target.Value[j];

                if(damageEntry.time > lastSecond) {
                    target.Value.Remove(damageEntry);
                }
            }
        }
    }

    private void LogDamageEntry(DamageEntry entry) {
        List<DamageEntry> entries;

        if(damageEntries.TryGetValue(entry.source, out entries) == true) {
            damageEntries[entry.source].Add(entry);
        }
        else {
            damageEntries.Add(entry.source, new List<DamageEntry> { entry });
        }
    }

    private void OnTimerComplete(EventData data) {

        float totalDamage = GetTotalDamage();


        dpsText.text = "DPS: " + MathF.Round(MathF.Abs(totalDamage), 2).ToString();
        //dpsText.text = "DPS: " + MathF.Round( MathF.Abs(averageDamage), 2).ToString();


    }

    private void OnClockReset(EventData data) {

        PruneOldEntries();

        //currentDamage = 0f;

    }

    private void OnActiveTimeOut(EventData data) {
        timeElapsed = 0f;
        currentDamage = 0f;
        isActive = false;
        dpsText.text = "DPS: 0";
    }

    private struct DamageEntry {
        public Ability source;
        public float damageValue;
        public DateTime time;

        public DamageEntry(Ability ability, float damageValue, DateTime time) {
            this.source = ability;
            this.damageValue = damageValue;
            this.time = time;
        }
    }
}
