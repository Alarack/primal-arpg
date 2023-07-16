using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
//using System;

public class Projectile : Entity {

    [Header("Visuals")]
    public Gradient textColorGradient;
    public bool ricochet;


    [Header("Impact Variables")]
    public float impactNoise;
    public float chainRadius = 15f;
    public Vector2 reboundForceMod = new Vector2(500f, 1500f);

    [Header("On Death Spawns")]
    public GameObject onDeathEffectPrefab;

    [Header("Chain Mask")]
    public LayerMask chainMask;

    [Header("Split Options")]
    public bool cloneSelfOnSplit;
    public float childSplitCount;
    public Projectile splitPrefab;

    private Collider2D myCollider;
    private Entity source;

    private List<Effect> onHitEffects = new List<Effect>();


    private Weapon parentWeapon;
    private Effect parentEffect;

    //private EffectZone activeZone;

    private Task killTimer;
    private Task impactTask;

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

    private void SetupChildCollision(Collider2D other) {
        Physics2D.IgnoreCollision(myCollider, other);
    }



    private void OnTriggerEnter2D(Collider2D other) {
        //if (LayerTools.IsLayerInMask(parentWeapon.collisionMask, other.gameObject.layer) == false)
        //    return;


        string layer = LayerMask.LayerToName(other.gameObject.layer);


        DeployZoneEffect();

        impactTask = new Task(ProcessImpact(other));

        return;

        ////Debug.Log(layer + " was hit");

        //if(layer != "Environment"){

        //    HandleProjectileSplit(other);

        //    //if (HandleProjectileSplit(other) == true)
        //    //    return;

        //    if (HandleProjectileChain(other) == true) {
        //        return;
        //    }

        //    if (HandleProjectilePierce() == true) {
        //        return;
        //    }

        //}


        //myCollider.enabled = false;
        //Movement.CanMove = false;
        //Movement.MyBody.freezeRotation = false;
        //Movement.MyBody.velocity = Vector2.zero;

        //SpawnDeathVFX();

        //if (ricochet == true)
        //    Ricochet(other);
        //else
        //    CleanUp(false);
        
        
        //Entity otherEntity = other.gameObject.GetComponent<Entity>();
        //if(otherEntity != null)
        //{
        //    DealDamage(otherEntity);
        //    ApplyOnHitEffects(otherEntity);
        //}

    }

    private IEnumerator ProcessImpact(Collider2D other) {

        string layer = LayerMask.LayerToName(other.gameObject.layer);

        yield return new WaitForSeconds(0.05f);

        if (layer != "Environment") {

            HandleProjectileSplit(other);

            //if (HandleProjectileSplit(other) == true)
            //    return;

            if (HandleProjectileChain(other) == true) {
                yield break; ;
            }

            if (HandleProjectilePierce() == true) {
                yield break; ;
            }

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
    }

    private void DeployZoneEffect() {
        EffectZone activeZone = Instantiate(parentEffect.Data.effectZoneInfo.effectZonePrefab, transform.position, Quaternion.identity);
        activeZone.Setup(parentEffect, parentEffect.Data.effectZoneInfo, null, this);

    }


    private bool HandleProjectilePierce() {

        if (Stats.Contains(StatName.ProjectilePierceCount) == false || Stats[StatName.ProjectilePierceCount] < 1f) {
            StartCleanUp();
            return false;
        }


        Stats.AddModifier(StatName.ProjectilePierceCount, -1, StatModType.Flat, this);

        EventData data = new EventData();

        data.AddEntity("Projectile", this);
        data.AddEntity("Owner", source);
        data.AddEffect("Parent Effect", parentEffect);
        data.AddAbility("Ability", parentEffect.ParentAbility);


        Debug.Log("Piercing has occured");

        EventManager.SendEvent(GameEvent.ProjectilePierced, data);

        return true;
    }

    private bool HandleProjectileSplit(Collider2D recentHit) {

        if (Stats.Contains(StatName.ProjectileSplitCount) == false || Stats[StatName.ProjectileSplitCount] < 1f) {
            StartCleanUp();
            return false;
        }


        Stats.AddModifier(StatName.ProjectileSplitCount, -1, StatModType.Flat, this);

        for (int i = 0; i < Stats[StatName.ProjectileSplitQuantity]; i++) {

            if (cloneSelfOnSplit == true) {
                Projectile child = Instantiate(parentEffect.Data.payloadPrefab, transform.position, transform.rotation) as Projectile;
                child.Setup(source, parentEffect);
                child.SetupChildCollision(recentHit);
                child.Stats.SetStatValue(StatName.ProjectileSplitCount, childSplitCount, this);

                TargetUtilities.RotateToRandomNearbyTarget(recentHit, child, chainRadius, chainMask, true);

            }
        }
        return true;
    }

    private bool HandleProjectileChain(Collider2D recentHit) {
        if (Stats.Contains(StatName.ProjectileChainCount) == false || Stats[StatName.ProjectileChainCount] < 1f) {
            StartCleanUp();
            return false;
        }


        Stats.AddModifier(StatName.ProjectileChainCount, -1, StatModType.Flat, this);
        TargetUtilities.RotateToRandomNearbyTarget(recentHit, this, chainRadius, chainMask, true);

        return true;
    }

    

    private void DealDamage(Entity target) {
        //Debug.Log("Doing Damage " + Stats[StatName.BaseDamage]);

        float value = StatAdjustmentManager.DealDamageOrHeal(target, Stats[StatName.BaseDamage], source, null);
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



    private void StartCleanUp() {

        myCollider.enabled = false;
        Movement.CanMove = false;
        Movement.MyBody.freezeRotation = false;
        Movement.MyBody.velocity = Vector2.zero;

        SpawnDeathVFX();

        //if (ricochet == true)
        //    Ricochet(other);
        //else
            CleanUp(false);
    }

    private void CleanUp(bool deployZone) {

        if (killTimer.Running == true)
            killTimer.Stop();

        if(impactTask.Running == true)
            impactTask.Stop();

        if (deployZone == true) {
            DeployZoneEffect();
            SpawnDeathVFX();
        }


        Destroy(gameObject);

        //new Task(CleanUpNextFrame(deployZone));
    }

    private IEnumerator CleanUpNextFrame(bool deployZone) {
        yield return new WaitForSeconds(0.05f);


        if (killTimer.Running == true)
            killTimer.Stop();

        if (deployZone == true) {
            DeployZoneEffect();
            SpawnDeathVFX();
        }


        Destroy(gameObject);

    }

}
