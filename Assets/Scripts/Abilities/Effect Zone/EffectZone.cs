using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectZone : Entity {

    //[Header("VFX")]
    //public GameObject spawnVFX;
    protected GameObject applyVFX;
    //public GameObject deathVFX;

    //[Header("Masking")]
    //private LayerMask mask;

    protected Effect parentEffect;
    protected List<Entity> targets = new List<Entity>();
    protected EffectZoneInfo zoneInfo;

    protected Timer persistantZoneTimer;

    public virtual void Setup(Effect parentEffect, EffectZoneInfo info, Transform parentToThis = null) {
        this.parentEffect = parentEffect;
        this.zoneInfo = info;
        //this.mask = mask;
        SetInfo();

        if (parentToThis != null) {
            transform.SetParent(parentToThis, false);
            transform.localPosition = Vector3.zero;
        }

        if(info.applyOnInterval == true) {
            persistantZoneTimer = new Timer(Stats[StatName.EffectInterval], OnEffectInterval, true);

            float effectDurationModifier = parentEffect.Source.Stats[StatName.GlobalEffectDurationModifier];

            if(effectDurationModifier != 0) {
                Stats.AddModifier(StatName.EffectLifetime, effectDurationModifier, StatModType.PercentAdd, parentEffect.Source);

                Debug.Log("Modifying a persistant effect's lifetime: " + effectDurationModifier + " :: " + Stats[StatName.EffectLifetime]);
            }
        }

        ConfigureCollision();
        new Task(CleanupAfterLifetime());
    }

    private void Update() {
        if(persistantZoneTimer != null) {
            persistantZoneTimer.UpdateClock();
        }
    }

    private void SetInfo() {
        applyVFX = zoneInfo.applyVFX;
        spawnEffectPrefab = zoneInfo.spawnVFX;
        deathEffectPrefab = zoneInfo.deathVFX;

    }


    private void ConfigureCollision() {
        if (zoneInfo.affectSource == true)
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

        parentEffect.Apply(target);
        CreateApplyVFX(target.transform.position);
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
        WaitForSeconds waiter = new WaitForSeconds(Stats[StatName.EffectLifetime]);

        Debug.Log("Cleaning up: " + gameObject.name + " after " + Stats[StatName.EffectLifetime] + " seconds");

        yield return waiter;
        CleanUp();
    }

    protected virtual void CleanUp() {
        //Die();
        SpawnDeathVFX();
        Destroy(gameObject);
    }


    protected virtual void CreateApplyVFX(Vector2 location, bool variance = true) {

        if(applyVFX == null) {
            Debug.LogWarning("an effect zone: " + gameObject.name + " has no apply vfx");
            return;
        }

        Vector2 loc = location;
        if (variance)
            loc = new Vector2(location.x + Random.Range(-0.5f, 0.5f), location.y + Random.Range(-0.5f, 0.5f));


        GameObject activeVFX = Instantiate(applyVFX, loc, Quaternion.identity);
        Destroy(activeVFX, 2f);

    }


    protected virtual void OnTriggerEnter2D(Collider2D other) {


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

        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity == null) {
            //Debug.LogWarning("An effect Zone: " + gameObject.name + " is trying to apply an effect to a non-entity: " + other.gameObject.name);
            return;
        }


        if(zoneInfo.applyOncePerTarget == true && IsTargetAlreadyAffected(otherEntity) == true) {
            return;
        }

        Apply(otherEntity);

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
