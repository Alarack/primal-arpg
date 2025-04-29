using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;

public class AISensor : MonoBehaviour {

    //Owner - Who's sensor am I?
    //range - how big is my sensor area?
    //Current Target - what have I sensed?

    public CircleCollider2D myCollider;
    public LayerMask detectionMask;
    public bool detectOnDamageTaken;
    public bool ignoreMinions;
    public MaskTargeting maskTargeting;
    public float forgetDistance;

    private float baseForgetDistance;

    public Entity LatestTarget { get; private set; }
    public Entity PriorityTarget { get; private set; }


    private NPC owner;
    private List<Entity> targets = new List<Entity>();
    private AIBrain brain;
    private ProjectileSensor projectileSensor;


    public void Initialize(NPC owner, AIBrain brain) {
        this.owner = owner;
        this.brain = brain;
        baseForgetDistance = forgetDistance;
        myCollider.radius = owner.Stats[StatName.DetectionRange];

        SetDetectionMask();

        projectileSensor = GetComponentInChildren<ProjectileSensor>();
        if(projectileSensor != null ) {
            projectileSensor.Setup(this, OnProjectileDetected);
        }

        
    }


    public void UpdateTargeting(MaskTargeting targeting) {
        
        maskTargeting = targeting;
        FlushTagets();
        detectionMask = 0;
        SetDetectionMask();
    }

    public void SetDetectionMask() {
        myCollider.enabled = true;
        //Debug.Log("Setting detection Layer based on: " + LayerMask.LayerToName(owner.gameObject.layer));

        detectionMask = LayerTools.SetupHitMask(detectionMask, owner.gameObject.layer, maskTargeting);
        //AddToDetectionMask(LayerMask.NameToLayer("Projectile"));
    }

    public void AddToDetectionMask(int layer) {
        detectionMask = LayerTools.AddToMask(detectionMask, layer);
    }

    public void RemoveFromDetectionMask(int layer) {
        detectionMask = LayerTools.RemoveFromMask(detectionMask, layer);
    }


    private void OnEnable() {
        RegisterEvents();
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }


    private void Update() {
        if (LatestTarget != null && forgetDistance > 0f) {
            float distance = GetDistanceToTarget();

            if (distance > forgetDistance) {
                Debug.LogWarning("Too far, forgetting: " + distance + " :: " + forgetDistance);

                OnDetectionLost(LatestTarget);
            }
        }
        
        if (LatestTarget == null && targets != null && targets.Count > 0) {
            FindNextClosestTarget();
        }
    }

    private void RegisterEvents() {
        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatChanged);
    }


    private void OnStatChanged(EventData data) {
        Entity target = data.GetEntity("Target");
        Entity cause = data.GetEntity("Source");
        float value = data.GetFloat("Value");

        StatName targetStat = (StatName)data.GetInt("Stat");

        if (target != owner)
            return;

        if (targetStat == StatName.Health && value < 0f) {
            if (detectOnDamageTaken == true 
                && cause != null 
                && cause != owner 
                && cause.subtypes.Contains(Entity.EntitySubtype.Orbital) == false) {
                OnTargetDetected(cause);
            }
        }

    }


    private void OnTriggerEnter2D(Collider2D other) {
        
        //Projectile detectedProjectile = IsDetectionProjectile(other);

        //if(detectedProjectile != null) {
        //    OnProjectileDetected(detectedProjectile);
        //}
 
        Entity detectedTarget = IsDetectionValid(other);

        if (detectedTarget == null)
            return;

        OnTargetDetected(detectedTarget);

    }

    private void OnTriggerExit2D(Collider2D other) {
        //Entity detectedTarget = IsDetectionValid(other);

        //if (detectedTarget == null)
        //    return;

        //OnDetectionLost(detectedTarget);
    }

    public float GetDistanceToTarget(Entity target = null) {
        if (target != null)
            return Vector2.Distance(owner.transform.position, target.transform.position);

        if (target == null && LatestTarget != null)
            return Vector2.Distance(owner.transform.position, LatestTarget.transform.position);

        return -1f;
    }

    private Entity IsDetectionValid(Collider2D other) {

        if (LayerTools.IsLayerInMask(detectionMask, other.gameObject.layer) == false)
            return null;

        Entity detectedTarget = other.gameObject.GetComponent<Entity>();


        if (ignoreMinions == true && detectedTarget.subtypes.Contains(Entity.EntitySubtype.Minion) == true)
            return null;

        if (detectedTarget.subtypes.Contains(Entity.EntitySubtype.Obstical) == true)
            return null;

        return detectedTarget;
    }

    //private Projectile IsDetectionProjectile(Collider2D other) {
    //    if (other.gameObject.layer == LayerMask.NameToLayer("Projectile") == false)
    //            return null;

    //    Projectile projectile = other.GetComponent<Projectile>();

    //    if (projectile == null) 
    //        return null;

    //    if(projectile.Source.ownerType == OwnerConstraintType.Friendly)
    //        return projectile;

    //    return null;
    //}


    private void OnProjectileDetected(Projectile target) {

        //Debug.Log("Detecting Projectile: " + target.EntityName);
        
        EventData data = new EventData();
        data.AddEntity("Detector", owner);
        data.AddEntity("Projectile", target);

        EventManager.SendEvent(GameEvent.ProjectileDetected, data);
    }

    private void OnTargetDetected(Entity target) {
        LatestTarget = target;

        if (targets.Contains(target) == false)
            targets.Add(target);


        float spotDistance = Vector2.Distance(target.transform.position, owner.transform.position);

        if (spotDistance > baseForgetDistance && baseForgetDistance > 0f) {
            forgetDistance = spotDistance * 1.2f;
        }

        //Debug.Log(owner.EntityName + " has detected: " + target.EntityName);

        EventData data = new EventData();
        data.AddEntity("Target", target);
        data.AddEntity("Cause", owner);
        EventManager.SendEvent(GameEvent.UnitDetected, data);
    }

    private void OnDetectionLost(Entity target) {

        forgetDistance = baseForgetDistance;

        if (targets.Contains(target) == true) {
            targets.Remove(target);
        }

        EventData data = new EventData();
        data.AddEntity("Target", target);
        data.AddEntity("Cause", owner);

        EventManager.SendEvent(GameEvent.UnitForgotten, data);

        if (LatestTarget == target) {

            if (targets.Count == 0) {
                LatestTarget = null;
                return;
            }

            LatestTarget = TargetUtilities.FindNearestTarget(targets, owner.transform);
        }

    }

    private void FindNextClosestTarget() {
        //if(owner == null) 
        //    return;

        //Debug.Log(owner.gameObject.name + " is the owner");
        //Debug.Log(targets.Count + " targets are in the list");


        LatestTarget = TargetUtilities.FindNearestTargetFromList(targets, owner.transform);
    }

    private void FlushTagets() {
        targets.Clear();
        LatestTarget = null;
        myCollider.enabled = false;
    }

}
