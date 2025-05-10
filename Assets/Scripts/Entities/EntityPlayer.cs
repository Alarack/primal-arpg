using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AllIn1VfxToolkit.Demo.Scripts;

public class EntityPlayer : Entity {

    public bool debugGodMode;
    public CircleCollider2D vacumCollider;
    public Inventory Inventory { get; private set; }


    private Task iFrameTask;

    public float CurrentDamageRoll { get { return GetDamgeRoll(); } }

    protected override void Awake() {
        base.Awake();
        Inventory = GetComponent<Inventory>();
    }

    protected override void Start() {
        base.Start(); 
    }

    protected override void OnEnable() {
        base.OnEnable();
    }

    protected override void OnDisable() {
        base.OnDisable();
    }

    protected override void Update() {
        base.Update();
        if (essenceRegenTimer != null) {
            essenceRegenTimer.UpdateClock();
        }

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.K)) {
            Die(this);
        }

        if(Input.GetKeyDown(KeyCode.H)) {
            StatAdjustmentManager.ApplyStatAdjustment(this, -1, StatName.Health, StatModType.Flat, StatModifierData.StatVariantTarget.RangeCurrent, this, null);
        }

        if (Input.GetKeyDown(KeyCode.O)) {
            StatAdjustmentManager.ApplyStatAdjustment(this, 1, StatName.HeathPotions, StatModType.Flat, StatModifierData.StatVariantTarget.RangeCurrent, this, null);
        }
#endif
    }

    public float GetDamgeRoll() {
        return Inventory.GetDamageRoll();
    }

    public float GetAverageDamageRoll() {
        return Inventory.GetAverageDamageRoll();
    }

    public void ActivateBigVacum() {
        vacumCollider.radius = 50f;
    }

    public void DeactivateBigVacum() {
        vacumCollider.radius = 6f;
    }

    private IEnumerator OnDamageInvincible() {
        Invincible = true;

        yield return new WaitForSeconds(0.2f);

        iFrameTask = null;
        Invincible = false;
    }

    #region EVENTS

    public override bool HasAbilityOfTag(AbilityTag tag) {

        List<Ability> targetAbilities = AbilityManager.GetAbilitiesByTag(tag, AbilityCategory.KnownSkill);
        bool unlocked = false;
        foreach (Ability ability in targetAbilities) {
            if(ability.Locked == false) {
                unlocked = true;
                break;
            }
        }


        return unlocked;
    }


    protected override void OnStatChanged(EventData data) {
        base.OnStatChanged(data);

        StatName stat = (StatName)data.GetInt("Stat");
        float value = data.GetFloat("Value");
        Entity target = data.GetEntity("Target");

        if (target != this)
            return;

        if (stat != StatName.Health)
            return;

        if (value >= 0f) {
            return;
        }

        if(iFrameTask == null) {
            iFrameTask = new Task(OnDamageInvincible());
        }

        AllIn1Shaker.i.DoCameraShake(0.05f);
        HitStopManager.Stop();
    }

    #endregion


    protected override void Die(Entity source, Ability sourceAbility = null) {

        if (debugGodMode == true) {
            return;
        }


        base.Die(source, sourceAbility);

        Invincible = false;
        iFrameTask = null;
        //EntityManager.RemoveEntity(this);
        SpawnDeathVFX();

        //Show Gameover Screen PanelManager.OpenPanel<GameOverPanel>();
        //GameOverPanel panel = FindObjectOfType<GameOverPanel>();
        //panel.Open();

        gameObject.SetActive(false);
        EntityManager.GameOver();
        PanelManager.OpenPanel<GameOverPanel>();

        //Destroy(gameObject);
    }


    #region DEBUG

    private void OnDrawGizmos() {

        Ray2D ray = new Ray2D(transform.position, transform.up);

        Vector2 forwardPoint = ray.GetPoint(7f);

        //Physics2D.Raycast(transform.position, transform.up, 7f);


        Gizmos.DrawSphere(forwardPoint, 0.5f);
    }


    #endregion

}
