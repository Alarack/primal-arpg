using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Projectile : Entity {

    [Header("Visuals")]
    public Gradient textColorGradient;
    public bool ricochet;


    [Header("Impact Variables")]
    public float impactNoise;
    public Vector2 reboundForceMod = new Vector2(500f, 1500f);

    [Header("On Death Spawns")]
    public GameObject onDeathEffectPrefab;

    private Collider2D myCollider;
    private Entity source;

    private List<Effect> onHitEffects = new List<Effect>();


    private Weapon parentWeapon;
    private Effect parentEffect;

    //private EffectZone activeZone;

    private Task killTimer;

    protected override void Awake() {
        base.Awake();

        myCollider = GetComponent<Collider2D>();

        float speedVariance = UnityEngine.Random.Range(0.5f, 1.5f);

        Stats.AddModifier(StatName.MoveSpeed, speedVariance, StatModType.PercentAdd, this);

        killTimer = new Task(KillAfterLifetime());

    }

    public void Setup(Entity source, Weapon parentWeapon, List<Effect> onHitEffects) {
        this.source = source;
        this.onHitEffects = onHitEffects;
        this.parentWeapon = parentWeapon;
        SetupCollisionIgnore(source.GetComponent<Collider2D>());
    }

    public void Setup(Entity source, Effect parentEffect) {
        this.source = source;
        this.parentEffect = parentEffect;
        SetupCollisionIgnore(source.GetComponent<Collider2D>());
    }


    public void IgnoreCollision(Entity target) {
        SetupCollisionIgnore(target.GetComponent<Collider2D>());
    }

    private void SetupCollisionIgnore(Collider2D ownerCollider) {
        Physics2D.IgnoreCollision(ownerCollider, myCollider);
    }



    private void OnTriggerEnter2D(Collider2D other) {
        //if(parentWeapon == null) {
        //    Debug.LogError("Parent Weapon is null when checking layers on Projectile Collision");
        //}

        //if (LayerTools.IsLayerInMask(parentWeapon.collisionMask, other.gameObject.layer) == false)
        //    return;

        //EffectZone activeZone = Instantiate(parentEffect.Data.effectZoneInfo.effectZonePrefab, transform.position, Quaternion.identity);
        //activeZone.Setup(parentEffect, parentEffect.Data.effectZoneInfo);

        DeployZoneEffect();

        if (HandleProjectilePierce() == true) {
            return;
        }


        myCollider.enabled = false;
        Movement.CanMove = false;
        Movement.MyBody.freezeRotation = false;
        Movement.MyBody.velocity = Vector2.zero;



        SpawnDeathVFX();

        if (ricochet == true)
            Ricochet(other);
        else
            CleanUp(false);
        //Entity otherEntity = other.gameObject.GetComponent<Entity>();
        //if(otherEntity != null)
        //{
        //    DealDamage(otherEntity);
        //    ApplyOnHitEffects(otherEntity);
        //}

    }

    private void DeployZoneEffect() {
        EffectZone activeZone = Instantiate(parentEffect.Data.effectZoneInfo.effectZonePrefab, transform.position, Quaternion.identity);
        activeZone.Setup(parentEffect, parentEffect.Data.effectZoneInfo);

    }


    private bool HandleProjectilePierce() {

        if (Stats[StatName.ProjectilePierceCount] == 0)
            return false;


        Stats.AddModifier(StatName.ProjectilePierceCount, -1, StatModType.Flat, this);
        return true;
    }



    private void DealDamage(Entity target) {
        //Debug.Log("Doing Damage " + Stats[StatName.BaseDamage]);

        float value = StatAdjustmentManager.DealDamageOrHeal(target, Stats[StatName.BaseDamage], source);
        FloatingText floatingText = FloatingTextManager.SpawnFloatingText(target.transform.position, value.ToString());
        floatingText.SetColor(textColorGradient);
    }

    private void ApplyOnHitEffects(Entity target) {
        if (onHitEffects == null || onHitEffects.Count == 0) {
            return;
        }

        for (int i = 0; i < onHitEffects.Count; i++) {
            onHitEffects[i].Apply(target);
        }
    }

    private void Ricochet(Collider2D other) {
        Vector2 direction = other.transform.position - transform.position;
        Vector2 offsetVector = TargetUtilities.CreateRandomDirection(-impactNoise, impactNoise);
        direction += offsetVector;
        Vector2 reboundForce = (-direction.normalized) * UnityEngine.Random.Range(reboundForceMod.x, reboundForceMod.y);


        float rotationForce = UnityEngine.Random.Range(720f, 1080f);
        rotationForce *= UnityEngine.Random.Range(0, 2) * 2 - 1;


        Movement.MyBody.angularVelocity = rotationForce;
        Movement.MyBody.AddForce(reboundForce, ForceMode2D.Force);

    }


    private IEnumerator KillAfterLifetime() {
        WaitForSeconds waiter = new WaitForSeconds(Stats[StatName.ProjectileLifetime]);
        yield return waiter;

        CleanUp(true);
    }



    private void CleanUp(bool deployZone) {

        if (killTimer.Running == true)
            killTimer.Stop();

        if(deployZone == true) {
            DeployZoneEffect();
            SpawnDeathVFX();
        }


        Destroy(gameObject);
    }

}
