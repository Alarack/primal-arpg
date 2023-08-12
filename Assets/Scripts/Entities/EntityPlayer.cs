using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;

public class EntityPlayer : Entity
{

    public bool debugGodMode;
    public Inventory Inventory { get; private set; }
    //public bool CanAttack { get; set; } = true;

    private Timer essenceRegenTimer;

    public float CurrentDamageRoll { get { return GetDamgeRoll(); } }



    protected override void Awake() {
        base.Awake();
        Inventory = GetComponent<Inventory>();
    }

    protected override void Start() {
        base.Start();

        essenceRegenTimer = new Timer(Stats[StatName.EssenceRegenerationRate], RegenEssence, true);
    }

    protected override void OnEnable() {
        base.OnEnable();

        Stats.AddStatListener(StatName.EssenceRegenerationRate, OnEssenceRegenChanged);
    }

    protected override void OnDisable() {
        base.OnDisable();
        Stats.RemoveStatListener(StatName.EssenceRegenerationRate, OnEssenceRegenChanged);
    }


    protected override void Update() {
        base.Update();
        if(essenceRegenTimer != null) {
            essenceRegenTimer.UpdateClock();
        }

    }


    public bool TrySpendEssence(float value) {
        float difference = Stats[StatName.Essence] - value;

        if (difference < 0)
            return false;

        Stats.AdjustStatRangeCurrentValue(StatName.Essence, -value, StatModType.Flat, this);
        return true;
    }

    private void OnEssenceRegenChanged(BaseStat stat, object source, float value) {
        essenceRegenTimer.SetDuration(stat.ModifiedValue);
    }
 
    private void RegenEssence(EventData data) {
        //Debug.Log("Regening: " + Stats[StatName.EssenceRegenerationValue] + "% of max essence. CurrentValue: " + Stats[StatName.Essence]);
        Stats.AdjustStatRangeByPercentOfMaxValue(StatName.Essence, Stats[StatName.EssenceRegenerationValue], this);
    }

    public float GetDamgeRoll() {
        return Inventory.GetDamageRoll();
    }

    public float GetAverageDamageRoll() {
        return Inventory.GetAverageDamageRoll();
    }

    #region EVENTS

    #endregion


    protected override void Die(Entity source, Ability sourceAbility = null) {

        if(debugGodMode == true) {
            return;
        }


        base.Die(source, sourceAbility);

        //EntityManager.RemoveEntity(this);
        SpawnDeathVFX();

        //Show Gameover Screen PanelManager.OpenPanel<GameOverPanel>();
        //GameOverPanel panel = FindObjectOfType<GameOverPanel>();
        //panel.Open();

        gameObject.SetActive(false);

        //Destroy(gameObject);
    }

}
