using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Layer Masks")]
    public LayerMask chainMask;
    public LayerMask projectileHitMask;

    [Header("Split Options")]
    public bool cloneSelfOnSplit;
    public float childSplitCount;
    public Projectile splitPrefab;

    [Header("Misc")]
    public float smoothScaleSpeed;
    public bool varyInitalSpeed = true;

    private Collider2D myCollider;
    public Entity Source { get; private set; }

    private List<Effect> onHitEffects = new List<Effect>();


    //private Weapon parentWeapon;
    private Effect parentEffect;

    //private EffectZone activeZone;

    private Task killTimer;
    private Task impactTask;
    private Task smoothScale;
    private float projectileSize;

    private int parentLayer;
    protected override void Awake() {
        base.Awake();

        if (Stats.Contains(StatName.ProjectileSize) == false) {
            Stats.AddStat(new SimpleStat(StatName.ProjectileSize, 1f));
        }


        myCollider = GetComponent<Collider2D>();

        if(varyInitalSpeed == true) {
            float speedVariance = UnityEngine.Random.Range(-0.2f, 0.2f);
            Stats.AddModifier(StatName.MoveSpeed, speedVariance, StatModType.PercentAdd, this);
        }
       
        //Debug.Log("Projectile: " + EntityName + " has an initial speed of: " + Stats[StatName.MoveSpeed].ToString());

        killTimer = new Task(KillAfterLifetime());

    }

    protected override void OnEnable() {
        base.OnEnable();

        Stats.AddStatListener(StatName.ProjectileSize, OnSizeChanged);
    }

    protected override void OnDisable() {
        base.OnDisable();

        Stats.RemoveStatListener(StatName.ProjectileSize, OnSizeChanged);
    }

    private void OnSizeChanged(BaseStat stat, object source, float value) {
        smoothScale = new Task(SmoothScale());
    }

    public void Setup(Entity source, Weapon parentWeapon, List<Effect> onHitEffects) {
        this.Source = source;
        this.onHitEffects = onHitEffects;
        //this.parentWeapon = parentWeapon;
        SetupCollisionIgnore(source.GetComponent<Collider2D>());
    }

    public void Setup(Entity source, Effect parentEffect, LayerMask hitMask, MaskTargeting maskTargeting = MaskTargeting.Opposite) {
        this.Source = source;
        this.parentEffect = parentEffect;
        this.projectileHitMask = hitMask;
        this.parentLayer = parentEffect.Source.gameObject.layer;
        this.ownerType = source.ownerType;

        //SetupHitMask();
        projectileHitMask = LayerTools.SetupHitMask(projectileHitMask, source.gameObject.layer, maskTargeting);

        ProjectileMovement move = Movement as ProjectileMovement;
        if(move != null) {
            move.SetSeekMask(source.gameObject.layer, maskTargeting);
        }
 
        //projectileHitMask = LayerTools.AddToMask(projectileHitMask, LayerMask.NameToLayer("Environment"));
        StartCoroutine(DelayEnvironmentMask());

        SetupCollisionIgnore(source.GetComponent<Collider2D>());
        SetupSize();

        SendProjectileCreatedEvent();
    }

    private IEnumerator DelayEnvironmentMask() {
        yield return new WaitForSeconds(0.15f);
        projectileHitMask = LayerTools.AddToMask(projectileHitMask, LayerMask.NameToLayer("Environment"));

    }

    private void SendProjectileCreatedEvent() {
        EventData data = new EventData();
        data.AddEffect("Parent Effect", parentEffect);
        data.AddAbility("Parent Ability", parentEffect.ParentAbility);
        data.AddEntity("Projectile", this);

        EventManager.SendEvent(GameEvent.ProjectileCreated, data);
    }

    private void SetupSize() {
        UpdateProjectleSize();

        transform.localScale = new Vector3(projectileSize, projectileSize, projectileSize);
    }

    private void UpdateProjectleSize() {
        projectileSize = Stats[StatName.ProjectileSize];

        //if(parentEffect.ParentAbility != null && parentEffect.ParentAbility.Stats.Contains(StatName.ProjectileSize)) {
        //    float inheritedAbilitySize = parentEffect.ParentAbility.Stats[StatName.ProjectileSize];
        //    projectileSize += inheritedAbilitySize;
        //    //Debug.Log("Updating projectile size by: " + inheritedAbilitySize);
        //}

        if (projectileSize <= 0)
            projectileSize = 1f;

        float globalSizeMod = 1f + Source.Stats[StatName.GlobalProjectileSizeModifier];

        projectileSize *= globalSizeMod;
    }

    private IEnumerator SmoothScale() {

        UpdateProjectleSize();

        //Debug.Log("Projectile Size " + Stats[StatName.ProjectileSize]);

        if (this == null)
            yield break;

        WaitForEndOfFrame waiter = new WaitForEndOfFrame();

        while (this != null && transform.localScale.x != projectileSize) {
            if (transform == null)
                yield break;

            float targetScale = Mathf.MoveTowards(transform.localScale.x, projectileSize, Time.deltaTime * smoothScaleSpeed);
            //Debug.Log("Target scale: " + targetScale);
            transform.localScale = new Vector3(targetScale, targetScale, targetScale);
            yield return waiter;
        }

    }

    public void IgnoreCollision(Entity target) {
        SetupCollisionIgnore(target.GetComponent<Collider2D>());
    }

    private void SetupCollisionIgnore(Collider2D ownerCollider) {
        if (ownerCollider == null)
            return;
        
        Physics2D.IgnoreCollision(ownerCollider, myCollider);
    }

    private void SetupChildCollision(Collider2D other) {
        Physics2D.IgnoreCollision(myCollider, other);
    }



    private void OnTriggerEnter2D(Collider2D other) {
        if (LayerTools.IsLayerInMask(projectileHitMask, other.gameObject.layer) == false) {
            //Debug.LogWarning(LayerMask.LayerToName(other.gameObject.layer) + " is not in the hit mask");
            return;
        }

        DeployZoneEffect(other);

        ApplyImpact(other);

    }

    private void ApplyImpact(Collider2D other) {

        string layer = LayerMask.LayerToName(other.gameObject.layer);

        if (layer != "Environment") {

            HandleProjectileSplit(other);
            HandleProjectileChain(other);
            HandleProjectilePierce(other);

            if (Stats.Contains(StatName.ProjectilePierceCount) && Stats[StatName.ProjectilePierceCount] > 0) {
                //Stats.AddModifier(StatName.ProjectilePierceCount, -1, StatModType.Flat, this);
                return;
            }

            if (Stats.Contains(StatName.ProjectileChainCount) && Stats[StatName.ProjectileChainCount] > 0) {
                //Stats.AddModifier(StatName.ProjectileChainCount, -1, StatModType.Flat, this);
                return;
            }
        }

        StartCleanUp();
    }

    private void DeployZoneEffect(Collider2D other) {
        //Debug.Log(gameObject.name + " is tryin to deplay an effect zone");
        if (other != null && parentEffect.EffectZonePrefab == null) {
            Entity otherEntity = other.GetComponent<Entity>();
            if (otherEntity != null) {
                bool applied = parentEffect.Apply(otherEntity);
                if(applied == true) {
                    parentEffect.SendEffectAppliedEvent();
                }
            }

            return;
        }

        if (parentEffect == null)
            return;

        if (parentEffect.EffectZonePrefab == null)
            return;

        Debug.LogWarning("Creating effect zone: " + parentEffect.EffectZonePrefab.gameObject.name);

        EffectZone activeZone = Instantiate(parentEffect.EffectZonePrefab, transform.position, Quaternion.identity);
        activeZone.Stats.AddMissingStats(parentEffect.Stats);
        activeZone.Setup(parentEffect, parentEffect.Data.effectZoneInfo, null, this, parentLayer, parentEffect.Data.maskTargeting);

    }


    #region CHAIN, PIERCE, AND SPLIT

    private bool HandleProjectilePierce(Collider2D recentHit) {
        if (Stats.Contains(StatName.ProjectilePierceCount) == false || Stats[StatName.ProjectilePierceCount] < 1f) {
            return false;
        }

        Entity otherEntity = recentHit.GetComponent<Entity>();
        new Task(SendPierceEvent(otherEntity));

        return true;
    }

    private IEnumerator SendPierceEvent(Entity cause) {
        yield return new WaitForSeconds(0.05f);

        Stats.AddModifier(StatName.ProjectilePierceCount, -1, StatModType.Flat, this);
        EventData data = new EventData();

        data.AddEntity("Projectile", this);
        data.AddEntity("Owner", Source);
        data.AddEntity("Cause", cause);
        data.AddEffect("Parent Effect", parentEffect);
        data.AddAbility("Ability", parentEffect.ParentAbility);

        //Debug.Log("Piercing has occured");

        EventManager.SendEvent(GameEvent.ProjectilePierced, data);
    }

    private bool HandleProjectileSplit(Collider2D recentHit) {

        if (Stats.Contains(StatName.ProjectileSplitCount) == false || Stats[StatName.ProjectileSplitCount] < 1f) {
            //StartCleanUp();
            return false;
        }

        Stats.AddModifier(StatName.ProjectileSplitCount, -1, StatModType.Flat, this);

        for (int i = 0; i < Stats[StatName.ProjectileSplitQuantity]; i++) {

            if (cloneSelfOnSplit == true) {
                Projectile child = Instantiate(parentEffect.PayloadPrefab, transform.position, transform.rotation) as Projectile;
                child.Setup(Source, parentEffect, projectileHitMask, parentEffect.Data.maskTargeting);
                child.SetupChildCollision(recentHit);
                child.Stats.SetStatValue(StatName.ProjectileSplitCount, childSplitCount, this);

                TargetUtilities.RotateToRandomNearbyTarget(recentHit, child, chainRadius, chainMask, true);

            }
        }
        return true;
    }

    private bool HandleProjectileChain(Collider2D recentHit) {
        if (Stats.Contains(StatName.ProjectileChainCount) == false || Stats[StatName.ProjectileChainCount] < 1f) {
            return false;
        }

        TargetUtilities.RotateToRandomNearbyTarget(recentHit, this, chainRadius, chainMask, true);
        Entity otherEntity = recentHit.GetComponent<Entity>();
        new Task(SendChainEvent(otherEntity));

        return true;
    }

    private IEnumerator SendChainEvent(Entity cause) {
        yield return new WaitForSeconds(0.05f);

        Stats.AddModifier(StatName.ProjectileChainCount, -1, StatModType.Flat, this);

        EventData data = new EventData();

        data.AddEntity("Projectile", this);
        data.AddEntity("Owner", Source);
        data.AddEntity("Cause", cause);
        data.AddEffect("Parent Effect", parentEffect);
        data.AddAbility("Ability", parentEffect.ParentAbility);

        //Debug.Log("Chaining has occured");

        EventManager.SendEvent(GameEvent.ProjectileChained, data);
    }

    #endregion

    private IEnumerator KillAfterLifetime() {
        WaitForSeconds waiter = new WaitForSeconds(Stats[StatName.ProjectileLifetime]);
        yield return waiter;

        CleanUp(true);
    }

    public void StartCleanUp() {

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

        if (killTimer != null && killTimer.Running == true)
            killTimer.Stop();

        if (impactTask != null && impactTask.Running == true)
            impactTask.Stop();

        if (smoothScale != null && smoothScale.Running == true)
            smoothScale.Stop();

        if (deployZone == true) {
            DeployZoneEffect(null);
            SpawnDeathVFX();
        }


        Destroy(gameObject, 0.05f);

        //new Task(CleanUpNextFrame(deployZone));
    }

    private IEnumerator CleanUpNextFrame(bool deployZone) {
        yield return new WaitForSeconds(0.05f);


        if (killTimer.Running == true)
            killTimer.Stop();

        if (deployZone == true) {
            DeployZoneEffect(null);
            SpawnDeathVFX();
        }


        Destroy(gameObject);

    }

    #region DEPRECATED

    //private void DealDamage(Entity target) {
    //    //Debug.Log("Doing Damage " + Stats[StatName.BaseDamage]);

    //    float value = StatAdjustmentManager.DealDamageOrHeal(target, Stats[StatName.BaseDamage], Source, null);
    //    FloatingText floatingText = FloatingTextManager.SpawnFloatingText(target.transform.position, value.ToString());
    //    floatingText.SetColor(textColorGradient);
    //}

    //private void ApplyOnHitEffects(Entity target) {
    //    if (onHitEffects == null || onHitEffects.Count == 0) {
    //        return;
    //    }

    //    for (int i = 0; i < onHitEffects.Count; i++) {
    //        onHitEffects[i].Apply(target);
    //    }
    //}

    //private void Ricochet(Collider2D other) {
    //    Vector2 direction = other.transform.position - transform.position;
    //    Vector2 offsetVector = TargetUtilities.CreateRandomDirection(-impactNoise, impactNoise);
    //    direction += offsetVector;
    //    Vector2 reboundForce = (-direction.normalized) * UnityEngine.Random.Range(reboundForceMod.x, reboundForceMod.y);


    //    float rotationForce = UnityEngine.Random.Range(720f, 1080f);
    //    rotationForce *= UnityEngine.Random.Range(0, 2) * 2 - 1;


    //    Movement.MyBody.angularVelocity = rotationForce;
    //    Movement.MyBody.AddForce(reboundForce, ForceMode2D.Force);

    //}

    #endregion

}
