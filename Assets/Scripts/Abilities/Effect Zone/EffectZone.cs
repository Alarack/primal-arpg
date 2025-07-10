using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Status;
using static UnityEngine.GraphicsBuffer;

public class EffectZone : Entity {


    public ParticleSystem vfxParticles;
    public float particleSpeed = 1f;
    protected GameObject applyVFX;
    public GameObject windupFinishedVFX;
    public ParticleSystem effectTelegraph;
    public GameObject growStartVFX;

    private LayerMask mask;

    protected Effect parentEffect;
    protected List<Effect> additionalEffects = new List<Effect>();
    protected List<Entity> targets = new List<Entity>();
    protected EffectZoneInfo zoneInfo;

    protected Timer effectIntervalTimer;
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

        if (parentLayer != -1)
            mask = LayerTools.SetupHitMask(mask, parentLayer, targeting);

        if (info.affectProjectiles == true) {
            mask = LayerTools.AddToMask(mask, LayerMask.NameToLayer("Projectile"));
        }

        if (parentEffect != null && parentEffect.Source != null)
            ownerType = parentEffect.Source.ownerType;

        SetInfo();
        SetSize();

        SetupWindup();

        if (parentToThis != null) {
            transform.SetParent(parentToThis, false);
            transform.localPosition = Vector3.zero;
        }

        SetDurationTimer();
        SetIntervalTimer(); 

        if (info.parentEffectToOrigin == true) {
            transform.SetParent(parentEffect.Source.transform, true);
            transform.localPosition = Vector3.zero;
        }

        ConfigureCollision();
        cleanTask = new Task(CleanupAfterLifetime());

        SpawnEntranceEffect(effectSize);
    }

    private void SetDurationTimer() {
        float effectDurationModifier = parentEffect.Source.Stats[StatName.GlobalEffectDurationModifier];
        float comboDurationModifier = parentEffect.Source.Stats[StatName.GlobalComboDurationModifier];
        float totalDurationModifier = effectDurationModifier + comboDurationModifier;

        if (totalDurationModifier != 0) {
            Stats.AddModifier(StatName.EffectLifetime, totalDurationModifier, StatModType.PercentAdd, parentEffect.Source);
        }
    }

    private void SetIntervalTimer() {
        if (zoneInfo.applyOnInterval == false)
            return;


        float effectIntervalModifier = parentEffect.Source.Stats[StatName.GlobalEffectIntervalModifier];
        float comboIntervalModifier = parentEffect.Source.Stats[StatName.GlobalComboIntervalModifier];
        float totalIntervalModifier = effectIntervalModifier + comboIntervalModifier;
        if (totalIntervalModifier != 0) {
            Stats.AddModifier(StatName.EffectInterval, totalIntervalModifier, StatModType.PercentAdd, parentEffect.Source);
        }
        effectIntervalTimer = new Timer(Stats[StatName.EffectInterval], OnEffectInterval, true);
    }



    public void AddAdditionalEffect(List<Effect> effects) {
        for (int i = 0; i < effects.Count; i++) {
            additionalEffects.Add(effects[i]);
        }
    }

    public void AddAdditionalEffect(Effect effect) {
        additionalEffects.Add(effect);
    }


    protected override void Update() {
        base.Update();

        if (effectIntervalTimer != null) {
            effectIntervalTimer.UpdateClock();
        }

        if (windupTimer != null) {
            windupTimer.UpdateClock();
        }
    }

    protected override void OnDisable() {
        base.OnDisable();

        if (cleanTask != null && cleanTask.Running == true)
            cleanTask.Stop();
    }

    protected override void RegisterStatListeners() {
        base.RegisterStatListeners();

        if(zoneInfo.applyOnInterval == true)
            EventManager.RegisterListener(GameEvent.AbilityStatAdjusted, OnAbilityStatChanged);
    }

    protected override void OnStatChanged(EventData data) {
        base.OnStatChanged(data);

        if (zoneInfo.applyOnInterval == false)
            return;

        StatName stat = (StatName)data.GetInt("Stat");
        Entity target = data.GetEntity("Target");

        if (target != parentEffect.Source)
            return;

        if(stat == StatName.GlobalEffectSizeModifier) {
            SetSize();
        }

        if(stat == StatName.GlobalComboIntervalModifier || stat == StatName.GlobalEffectIntervalModifier) {
            UpdateIntervalTimer();
        }
    }

    private void OnAbilityStatChanged(EventData data) {

        Ability ability = data.GetAbility("Ability");
        StatName stat = (StatName)data.GetInt("Stat");

        if (ability != parentEffect.ParentAbility)
            return;

        if (stat == StatName.EffectSize) {
            SetSize();
        }

        if(stat == StatName.EffectInterval) {
            UpdateIntervalTimer();
        }

    }

    private void UpdateIntervalTimer() {
        if (zoneInfo.applyOnInterval == false)
            return;

        Stats.RemoveAllModifiersFromSource(StatName.EffectInterval, parentEffect.Source);

        float effectIntervalModifier = parentEffect.Source.Stats[StatName.GlobalEffectIntervalModifier];
        float comboIntervalModifier = parentEffect.Source.Stats[StatName.GlobalComboIntervalModifier];
        float totalIntervalModifier = effectIntervalModifier + comboIntervalModifier;

        if (totalIntervalModifier != 0) {
            Stats.AddModifier(StatName.EffectInterval, totalIntervalModifier, StatModType.PercentAdd, parentEffect.Source);
        }

        effectIntervalTimer.SetDuration(Stats[StatName.EffectInterval]);
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
            activeGrowVFX.transform.localScale = new Vector3(1f / effectSize, 1f / effectSize, 1f / effectSize);
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

        if (carrier != null) {
            float carrierSize = carrier.Stats[StatName.ProjectileSize];
            //Debug.Log("Carrier Size: " + carrierSize);

            if (carrierSize > effectSize) {
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
        if (tweenHelper != null && tweenHelper.startOnAwake == false) {
            //tweenHelper.endScale = effectSize;
            tweenHelper.StartTweeing();
        }

        if(parentEffect.ParentAbility.EssenceCostAsPercent == true) {
            float sizeMod = parentEffect.EssenceSpent * parentEffect.ParentAbility.Stats[StatName.EssenceScalingMultiplier];
            effectSize *= sizeMod;
        }


        transform.localScale = new Vector3(effectSize, effectSize, effectSize);
    }

    private void CleanUpGrowTweens() {
        if (activeGrowVFX == null)
            return;

        TweenHelper growTween = activeGrowVFX.GetComponent<TweenHelper>();

        if (growTween != null) {
            growTween.KillTweens();
        }

        Destroy(activeGrowVFX);

        if (activeEffectTelegraph != null)
            Destroy(activeEffectTelegraph.gameObject);
    }

    private void OnWindupFinished(EventData data) {
        myCollider.enabled = true;
        //CircleCollider2D circleCollider = myCollider as CircleCollider2D;
        //circleCollider.radius = effectSize;
        VFXUtility.SpawnVFX(windupFinishedVFX, transform, null, 1f, effectSize);

        //Debug.LogWarning("Windup Finished: " + gameObject.name);

        CleanUpGrowTweens();



        //Destroy(activeGrowVFX);
        //Destroy(activeEffectTelegraph.gameObject);
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
        CheckDoubleTick();

        if (zoneInfo.intervalVFX == null)
            return;

        VFXUtility.SpawnVFX(zoneInfo.intervalVFX, transform, 0.5f);
    }

    private void CheckDoubleTick() {
        float doubleTickStat = parentEffect.ParentAbility.Stats[StatName.DoubleTickChance] + parentEffect.Source.Stats[StatName.DoubleTickChance];

        if (doubleTickStat <= 0f)
            return;

        float roll = Random.Range(0f, 1f);

        //Debug.Log("Checking Double tick for: " + parentEffect.ParentAbility.Data.abilityName);

        if (roll <= doubleTickStat) {
            ApplyToAllTargets();
        }
    }

    protected virtual void Apply(Entity target, bool ignoreFirstHit = false) {

        ApplyEffect(target, parentEffect, ignoreFirstHit);

        if(additionalEffects.Count > 0) {
            for (int i = 0; i < additionalEffects.Count; i++) {
                ApplyEffect(target, additionalEffects[i], ignoreFirstHit);
            }
        }
        
        
        //targets.AddUnique(target);

        //parentEffect.TrackActiveDelivery(carrier);

        ////Debug.Log("Zone effect is applying: " + parentEffect.Data.effectName + " to " + target.EntityName);
        //bool applied = parentEffect.Apply(target);


        //if (applied == true) {
        //    CreateApplyVFX(target.transform.position);
        //    parentEffect.SendEffectAppliedEvent();
        //}

    }

    private void ApplyEffect(Entity target, Effect effect, bool ignoreFirstHit = false) {
        targets.AddUnique(target);

        if (ignoreFirstHit == true)
            return;


        effect.TrackActiveDelivery(carrier);

        //Debug.Log("Zone effect is applying: " + parentEffect.Data.effectName + " to " + target.EntityName);
        bool applied = effect.Apply(target);


        if (applied == true) {
            CreateApplyVFX(target.transform.position);
            effect.SendEffectAppliedEvent();
        }
    }

    private void RemoveEffect(Entity target, Effect effect) {
        targets.RemoveIfContains(target);

        if (zoneInfo.removeEffectOnExit == true) {

            if (CheckNonStacking(target) == true)
                return;

            effect.Remove(target);
        }
    }

    protected virtual void Remove(Entity target) {

        RemoveEffect(target, parentEffect);

        if (additionalEffects.Count > 0) {
            for (int i = 0; i < additionalEffects.Count; i++) {
                RemoveEffect(target, additionalEffects[i]);
            }
        }


        //targets.RemoveIfContains(target);

        //if (zoneInfo.removeEffectOnExit == true) {

        //    if (CheckNonStacking(target) == true)
        //        return;

        //    parentEffect.Remove(target);
        //}

    }

    private bool CheckNonStacking(Entity target) {
        bool inOtherZone = parentEffect.IsTargetInOtherZone(this, target);

        if (parentEffect.Data.nonStacking == false || inOtherZone == false)
            return false;

        //if (parentEffect.Data.nonStacking == true && inOtherZone) {
        if (parentEffect.PsudoStacks.ContainsKey(target) == true) {
            parentEffect.PsudoStacks[target]--;
            //Debug.LogWarning("Decrementing a count for : " + parentEffect.Data.effectName + " on " + target.EntityName + " :: " + count);

            if (parentEffect.PsudoStacks[target] == 0) {
                parentEffect.PsudoStacks.Remove(target);
            }
        }

       
        return true;
        //}

        //return false;
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

        //Debug.LogWarning("Destroying: " + EntityName + " after " + Stats[StatName.EffectLifetime] + " seconds");
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
        CleanUpGrowTweens();

        if (vfxParticles != null) {
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

        SpawnDeathVFX(effectSize);
        parentEffect.ParentAbility.SendAbilityEndedEvent(this);
        //Debug.LogWarning("Cleaning Up: " + EntityName);
        Destroy(gameObject);
    }


    //protected virtual void CreateApplyVFX(Vector2 location, bool variance = true) {

    //    if (applyVFX == null) {
    //        Debug.LogWarning("an effect zone: " + EntityName + " has no apply vfx");
    //        return;
    //    }

    //    //Debug.Log("Creating an Apply VFX: " + applyVFX.gameObject.name);

    //    Vector2 loc = location;
    //    if (variance)
    //        loc = new Vector2(location.x + Random.Range(-0.5f, 0.5f), location.y + Random.Range(-0.5f, 0.5f));


    //    GameObject activeVFX = Instantiate(applyVFX, loc, Quaternion.identity);

    //    float scale = zoneInfo.applyOnInterval == true ? 1f : 1f;

    //    activeVFX.transform.localScale = new Vector3(scale, scale, scale);

    //    Destroy(activeVFX, 2f);

    //}

    private void CreateApplyVFX(Vector2 location, bool variance = true) {
        if (applyVFX == null) {
            //Debug.LogWarning("an effect zone: " + EntityName + " has no apply vfx");
            return;
        }

        VFXUtility.SpawnVFX(applyVFX, location, Quaternion.identity, null, 2f, 1f, variance);
    }


    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (LayerTools.IsLayerInMask(mask, other.gameObject.layer) == false) {
            //Debug.Log(EntityName + " rejected a layer: " + other.gameObject.layer);
            return;
        }

        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity == null) {
            //Debug.LogWarning("An effect Zone: " + gameObject.name + " is trying to apply an effect to a non-entity: " + other.gameObject.name);
            return;
        }

        //bool inOtherZone = parentEffect.IsTargetInOtherZone(this, otherEntity);
        //if (Stats.Contains(StatName.MaxStackCount) == false && inOtherZone) {
        //    Debug.LogWarning("Cannot Add " + otherEntity.EntityName + " It is already in another effect zone");
        //    return;
        //}

        //if (Stats.Contains(StatName.MaxStackCount) == true) {
        //    if (Stats[StatName.MaxStackCount] > 1) {

        //    }
        //}


        if (zoneInfo.applyOncePerTarget == true && IsTargetAlreadyAffected(otherEntity) == true) {
            //Debug.LogWarning(otherEntity.EntityName + " has already been affected by " + parentEffect.Data.effectName);
            return;
        }

        //Debug.LogWarning("An effect Zone: " + gameObject.name + " for effect: " + parentEffect.Data.effectName + " is applying to: " + otherEntity.EntityName);

        bool ignoreEnterApply = zoneInfo.applyOnInterval && zoneInfo.dontTickOnEnter;

        Apply(otherEntity, ignoreEnterApply);

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
    public bool parentToTarget;
    public bool applyOncePerTarget;
    public bool applyOnInterval;
    public bool dontTickOnEnter;
    public bool affectProjectiles;

    [Header("Collision")]
    public bool affectSource;

    [Header("VFX")]
    public GameObject spawnVFX;
    public GameObject applyVFX;
    public GameObject deathVFX;
    public GameObject intervalVFX;

    [Header("Prefab")]
    public EffectZone effectZonePrefab;


}
