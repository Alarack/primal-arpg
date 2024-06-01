using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityPlayer : Entity {

    public bool debugGodMode;
    public CircleCollider2D vacumCollider;
    public Inventory Inventory { get; private set; }


    public float CurrentDamageRoll { get { return GetDamgeRoll(); } }



    protected override void Awake() {
        base.Awake();
        Inventory = GetComponent<Inventory>();
    }

    protected override void Start() {
        base.Start();

        Inventory.AddEXP(25f);
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

    #region EVENTS

    public override bool HasAbilityOfTag(AbilityTag tag) {

        List<Ability> targetAbilities = AbilityManager.GetAbilitiesByTag(tag, AbilityCategory.KnownSkill);



        return false;
    }



    #endregion


    protected override void Die(Entity source, Ability sourceAbility = null) {

        if (debugGodMode == true) {
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


    #region DEBUG

    private void OnDrawGizmos() {

        Ray2D ray = new Ray2D(transform.position, transform.up);

        Vector2 forwardPoint = ray.GetPoint(7f);

        //Physics2D.Raycast(transform.position, transform.up, 7f);


        Gizmos.DrawSphere(forwardPoint, 0.5f);
    }


    #endregion

}
