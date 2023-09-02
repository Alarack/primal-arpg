using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectZone : Entity {


    public ParticleSystem vfxParticles;
    public float particleSpeed = 1f;
    protected GameObject applyVFX;
    public GameObject windupFinishedVFX;
    public ParticleSystem effectTelegraph;
    public GameObject growStartVFX;

    private LayerMask mask;

    protected Effect parentEffect;
    protected List<Entity> targets = new List<Entity>();
    protected EffectZoneInfo zoneInfo;

    protected Timer persistantZoneTimer;
    protected Projectile carrier;

    private float effectSize = 1f;

    private Task cleanTask;

    private Collider2D myCollider;

    private Timer windupTimer;

    private ParticleSystem activeEffectTelegraph;
    private GameObject activeGrowVFX;

    public TweenHelper tweenHelper;

    protected override void Awake() {
        base.Awake();

        myCollider = GetComponent<Collider2D>();

        
    }


    public virtual void Setup(Effect parentEffect, EffectZoneInfo info, Transform parentToThis = null, Projectile carrier = null, int parentLayer = -1, MaskTargeting targeting = MaskTargeting.Opposite) {
        this.parentEffect = parentEffect;
        this.zoneInfo = info;
        this.carrier = carrier;
        this.ownerType = parentEffect.Source.ownerType;

        if(parentLayer != -1)
            mask = LayerTools.SetupHitMask(mask, parentLayer, targeting);

        //if (parentEffect.Source == null) {
        //    Debug.LogWarning("Spawning an effect zone while the source is dead: " + parentEffect.Data.effectName);
        //}

        if(parentEffect != null && parentEffect.Source != null)
            ownerType = parentEffect.Source.ownerType;


        

        SetInfo();
        SetSize();

        SetupWindup();

        if (parentToThis != null) {
            transform.SetParent(parentToThis, false);
            transform.localPosition = Vector3.zero;
        }

        if(info.applyOnInterval == true) {
            persistantZoneTimer = new Timer(Stats[StatName.EffectInterval], OnEffectInterval, true);

            float effectDurationModifier = parentEffect.Source.Stats[StatName.GlobalEffectDurationModifier];

            if(effectDurationModifier != 0) {
                Stats.AddModifier(StatName.EffectLifetime, effectDurationModifier, StatModType.PercentAdd, parentEffect.Source);
            }
        }

        if(info.parentEffectToOrigin == true) {
            transform.SetParent(parentEffect.Source.transform, true);
            transform.localPosition = Vector3.zero;
        }

        ConfigureCollision();
        cleanTask = new Task(CleanupAfterLifetime());
    }

    protected override void Update() {
        base.Update();
        
        if(persistantZoneTimer != null) {
            persistantZoneTimer.UpdateClock();
        }

        if(windupTimer != null) {
            windupTimer.UpdateClock();
        }
    }

    protected override void OnDisable() {
        base.OnDisable();

        if(cleanTask != null && cleanTask.Running == true)
            cleanTask.Stop();
    }

    private void SetInfo() {
        applyVFX = zoneInfo.applyVFX;
        spawnEffectPrefab = zoneInfo.spawnVFX;
        deathEffectPrefab = zoneInfo.deathVFX;

    }

    private void SetupWindup() {
        if (Stats.Contains(StatName.EffectZoneWindupTime) == true && Stats[StatName.EffectZoneWindupTime] > 0) {
            myCollider.enabled = false;
            //growStartVFX.transform.localScale = new Vector3(1 / effectSize, 1 / effectSize, 1 / effectSize);
            //growStartVFX.transform.SetParent(null, true);
            activeGrowVFX = Instantiate(growStartVFX, transform.position, transform.rotation);
            //activeGrowVFX.transform.SetParent(transform, true);
            activeGrowVFX.transform.localScale = new Vector3(1f / effectSize, 1f /effectSize,  1f/effectSize);
            TweenHelper helper = activeGrowVFX.AddComponent<TweenHelper>();
            helper.preset = TweenHelper.TweenPreset.Grow;
            helper.endScale = effectSize;
            helper.scaleDuration = Stats[StatName.EffectZoneWindupTime];
            helper.startOnAwake = true;

            windupTimer = new Timer(Stats[StatName.EffectZoneWindupTime], OnWindupFinished, false);
            activeEffectTelegraph = Instantiate(effectTelegraph, transform.position, transform.rotation);
            activeEffectTelegraph.transform.SetParent(transform.parent, true);
            activeEffectTelegraph.transform.localScale = new Vector3(effectSize, effectSize, effectSize);
            //activeEffectTelegraph.gameObject.transform.localScale = new Vector3(effectSize, effectSize, effectSize);
            //ParticleSystem.MainModule main = activeEffectTelegraph.main;
            //main.startSize =
        }
    }

    private void SetSize() {
        if (parentEffect.Stats.Contains(StatName.EffectSize)) {
            effectSize = parentEffect.Stats[StatName.EffectSize];

            if (effectSize <= 0) {
                effectSize = 1f;
            }
        }

        if(carrier != null) {
            float carrierSize = carrier.Stats[StatName.ProjectileSize];
            //Debug.Log("Carrier Size: " + carrierSize);
            
            if(carrierSize > effectSize) {
                effectSize = carrierSize;
            }
        }
        //else {
        //    Debug.Log("Null projectile");
        //}

        //Debug.Log("Effect Size: " + effectSize);

        float globalSizeMod = 1f + parentEffect.Source.Stats[StatName.GlobalEffectSizeModifier];
        effectSize *= globalSizeMod;

        //Debug.Log("effect size: " + effectSize);
        if(tweenHelper != null &&  tweenHelper.startOnAwake == false) {
            //tweenHelper.endScale = effectSize;
            tweenHelper.StartTweeing();
        }


        transform.localScale = new Vector3(effectSize, effectSize, effectSize);
    }

    private void OnWindupFinished(EventData data) {
        myCollider.enabled = true;
        //CircleCollider2D circleCollider = myCollider as CircleCollider2D;
        //circleCollider.radius = effectSize;
        VFXUtility.SpawnVFX(windupFinishedVFX, transform, null, 1f, effectSize);
        Destroy(activeGrowVFX);
        Destroy(activeEffectTelegraph.gameObject);
    }
    private void ConfigureCollision() {
        if (zoneInfo.affectSource == true)
            return;

        if (parentEffect.Source == null) 
            return;

        Collider2D sourceCollider = parentEffect.Source.GetComponent<Collider2D>();
        Collider2D myCollider = GetComponent<Collider2D>();

        Physics2D.IgnoreCollision(sourceCollider, myCollider);
    }

    protected virtual void OnEffectInterval(EventData data) {
        ApplyToAllTargets();
    }

    protected virtual void Apply(Entity target) {
        targets.AddUnique(target);

        parentEffect.TrackActiveDelivery(carrier);

        //Debug.Log("Zone effect is applying: " + parentEffect.Data.effectName);
        parentEffect.Apply(target);
        CreateApplyVFX(target.transform.position);

        parentEffect.SendEffectAppliedEvent();
    }

    protected virtual void Remove(Entity target) {
        targets.RemoveIfContains(target);


        if(zoneInfo.removeEffectOnExit == true) {
            parentEffect.Remove(target);
        }

    }

    protected virtual void ApplyToAllTargets() {
        for (int i = 0; i < targets.Count; i++) {
            Apply(targets[i]);
        }
    }

    protected virtual void RemoveAllTargets() {
        for (int i = 0; i < targets.Count; i++) {
            Remove(targets[i]);
        }
    }

    public virtual bool IsTargetAlreadyAffected(Entity target) {
        return targets.Contains(target);
    }

    protected virtual IEnumerator CleanupAfterLifetime() {

        if (Stats[StatName.EffectLifetime] <= 0)
            yield break;

        //bool winding = myCollider != null && myCollider.enabled == false;
        WaitForEndOfFrame windupWaiter = new WaitForEndOfFrame();

        while (myCollider != null && myCollider.enabled == false) {
            //Debug.Log("Wiating for fuse");
            yield return windupWaiter;
        }

        
        WaitForSeconds waiter = new WaitForSeconds(Stats[StatName.EffectLifetime]);

        yield return waiter;
        CleanUp();
    }

    public virtual void CleanUp() {
        //Die();

        if(vfxParticles != null) {
            vfxParticles.transform.SetParent(null, true);
            vfxParticles.Stop();
            //vfxParticles.main.simulationSpeed = particleSpeed;

            ParticleSystem[] particles = vfxParticles.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < particles.Length; i++) {
                ParticleSystem.MainModule main = particles[i].main;
                main.simulationSpeed = particleSpeed;
            }
            
           
            
            
            Destroy(vfxParticles.gameObject, 2f);
        }


        SpawnDeathVFX();
        Destroy(gameObject);
    }


    protected virtual void CreateApplyVFX(Vector2 location, bool variance = true) {

        if(applyVFX == null) {
            //Debug.LogWarning("an effect zone: " + gameObject.name + " has no apply vfx");
            return;
        }

        Vector2 loc = location;
        if (variance)
            loc = new Vector2(location.x + Random.Range(-0.5f, 0.5f), location.y + Random.Range(-0.5f, 0.5f));


        GameObject activeVFX = Instantiate(applyVFX, loc, Quaternion.identity);

        activeVFX.transform.localScale = new Vector3(effectSize, effectSize, effectSize);

        Destroy(activeVFX, 2f);

    }


    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (LayerTools.IsLayerInMask(mask, other.gameObject.layer) == false)
            return;
        
        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity == null) {
            //Debug.LogWarning("An effect Zone: " + gameObject.name + " is trying to apply an effect to a non-entity: " + other.gameObject.name);
            return;
        }


        if (zoneInfo.applyOncePerTarget == true && IsTargetAlreadyAffected(otherEntity) == true) {
            return;
        }

        //Debug.LogWarning("An effect Zone: " + gameObject.name + " for effect: " + parentEffect.Data.effectName + " is applying to: " + otherEntity.EntityName);

        Apply(otherEntity);

    }

    protected virtual void OnTriggerExit2D(Collider2D other) {
        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity == null) {
            //Debug.LogWarning("An effect Zone: " + gameObject.name + " is trying to remove an effect to a non-entity: " + other.gameObject.name);
            return;
        }

        Remove(otherEntity);

    }

    protected virtual void OnTriggerStay2D(Collider2D other) {

      

    }



}

[System.Serializable]
public struct EffectZoneInfo {

    [Header("Options")]
    public bool removeEffectOnExit;
    public bool parentEffectToOrigin;
    public bool applyOncePerTarget;
    public bool applyOnInterval;

    [Header("Collision")]
    public bool affectSource;

    [Header("VFX")]
    public GameObject spawnVFX;
    public GameObject applyVFX;
    public GameObject deathVFX;

    [Header("Prefab")]
    public EffectZone effectZonePrefab;


}
