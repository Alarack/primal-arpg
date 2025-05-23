using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProjectileMovement : EntityMovement
{
    public enum MovementBehavior
    {
        Straight,
        Seeking,
        Drunk
    }

    public MovementBehavior movementBehavior;

    [Header("Seeking Variable")]
    public Transform seekTarget;
    public LayerMask targetLayers;
    public float seekRadius;
    public bool seekForwardWithoutTarget;
    public float seekDuration = -1f;
    public float forwardPointDistance = 8f;

    private Vector2 seekPoint;
    private Timer seekTimer;

    [Header("Delay Variables")]
    public float delayTime = 0f;
    public float hoverSpeed = 25f;
    private Timer delayTimer;
    private bool hovering;

    private float hoverDirection = 1f;
    Tween hoverTween;


    [Header("Drunk Variables")]
    public float drunkInterval;
    public float drunkVariance;
    private Timer drunkTimer;

    private Quaternion lastDrunkRotation;

    private Entity projectileSource;
    private Projectile projectileOwner;

    protected override void Awake()
    {
        base.Awake();

        SetupBehaviors(seekDuration, drunkInterval);

    }

    private void SetupBehaviors(float seekDuration = -1, float drunkInterval = 0.5f) {
        if (movementBehavior == MovementBehavior.Drunk) {
            RandomizeDirection(null);
            drunkTimer = new Timer(drunkInterval, RandomizeDirection, true);
        }

        if (movementBehavior == MovementBehavior.Seeking && seekDuration > 0f) {
            seekTimer = new Timer(seekDuration, OnSeekTimerFinished);
        }

        if (delayTime > 0f) {
            hovering = true;
            delayTimer = new Timer(delayTime, OnDelayTimerFinished);


            hoverTween = DOTween.To(() => hoverDirection, x => hoverDirection = x, -1f, 1f).
                SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
  
        }
    }

    public void ChangeBehaviour(MovementBehavior newBehavior, float seektimer = -1, float drunkTimer = 0.5f) {
        
        if (movementBehavior == newBehavior) {
            //Debug.LogWarning("A projectile was told to change behavior to: " + newBehavior + " but it already was");
            return;
        }

        movementBehavior = newBehavior;

        SetupBehaviors(seektimer, drunkTimer);
    }

    private void Start() {
        projectileOwner = Owner as Projectile;
        projectileSource = projectileOwner != null ? projectileOwner.Source : null;

        if (seekForwardWithoutTarget == true)
            SetSeekPoint();
    }

    private void OnEnable() {

        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnSpeedChanged);
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }

    private void OnSpeedChanged(EventData data) {

        Entity target = data.GetEntity("Target");
        StatName stat = (StatName)data.GetInt("Stat");
        bool removal = data.GetBool("Removal");
        float value = data.GetFloat("Value");

        if (target != Owner)
            return;

        if(stat != StatName.MoveSpeed) 
            return;


        if(removal == false) {
            //Debug.Log(Owner.EntityName + "has had their speed changed by " + value);

            float speedHack = 1 + value;
            MyBody.linearVelocity *= speedHack;
        }
        else {
            //Debug.Log(Owner.EntityName + "has a stat mod of " + value + " removed");
            float speedHack = 1 - value;
            MyBody.linearVelocity *= speedHack;
        }




    }

    public void SetSeekMask(int parentLayer, MaskTargeting targeting) {

        targetLayers = LayerTools.SetupHitMask(targetLayers, parentLayer, targeting);
    }

    private void SetSeekPoint() {
        //Entity source = (Owner as Projectile) != null ? (Owner as Projectile).Source : null;

        if (projectileSource == null) {
            Debug.LogError("A projectile: " + Owner.EntityName + " has a null source entity");
            return;
        }

        Ray2D ray = new Ray2D(projectileSource.GetOriginPoint().position, projectileSource.GetOriginPoint().up);

        seekPoint = ray.GetPoint(forwardPointDistance);
    }

    private void OnSeekTimerFinished(EventData data) {
        movementBehavior = MovementBehavior.Straight;
    }

    private void OnDelayTimerFinished(EventData data) {
        hovering = false;
    }

    private void Update()
    {
        if (movementBehavior == MovementBehavior.Drunk && drunkTimer != null)
        {
            drunkTimer.UpdateClock();
        }

        if(movementBehavior == MovementBehavior.Seeking && seekTimer != null) {
            seekTimer.UpdateClock();
        }

        if(hovering == true && delayTimer != null) {
            delayTimer.UpdateClock();
        }
    }

    protected override void Move()
    {

        if(hovering == true) {
            Hover();
            return;
        }

        switch (movementBehavior)
        {
            case MovementBehavior.Straight:
                MoveStraight();
                break;
            case MovementBehavior.Seeking:
                MoveSeeking();
                break;
            case MovementBehavior.Drunk:
                MoveDrunk();
                break;

        }
    }



    private void MoveStraight()
    {
        float globalProjectileSpeed = 1f + (projectileSource != null ? projectileSource.Stats[StatName.GlobalProjectileSpeedModifier] : 0);

        //Debug.Log("Global Projectile Speed Modifier: " + globalProjectileSpeed);
        //Debug.Log("Projectile Source is Null: " + projectileSource);

        MyBody.AddForce(transform.up * Owner.Stats[StatName.MoveSpeed] * globalProjectileSpeed * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    private void Hover() {
        Vector2 moveForce = new Vector2(0f, hoverDirection) * Time.fixedDeltaTime * hoverSpeed;

        MyBody.AddForce(moveForce, ForceMode2D.Force);
    }

    private void MoveSeeking()
    {
        MoveStraight();
        
        
        if(seekForwardWithoutTarget == false) {
            RotateTowardTarget();
        }
        else {
            RotateTowardForwardPoint();
        }
        
       

    }

    private void RotateTowardTarget() {
        CheckForTargets();

        if (seekTarget == null)
            return;
        TargetUtilities.RotateSmoothlyTowardTarget(seekTarget, transform, Owner.Stats[StatName.RotationSpeed]);
    }

    private void MoveDrunk()
    {
        MoveStraight();
        SmoothDrunkRotate();
    }


    private void SmoothDrunkRotate()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lastDrunkRotation, Owner.Stats[StatName.RotationSpeed] * 1.5f * Time.fixedDeltaTime);
    }

    private void RandomizeDirection(EventData timerEventData)
    {

        Vector3 currentEuler = transform.eulerAngles;

        float currentZ = currentEuler.z;

        float alterdZ = currentZ + Random.Range(-drunkVariance, drunkVariance);

        Vector3 drunkVector = new Vector3(0f, 0f, alterdZ);

        lastDrunkRotation = Quaternion.Euler(drunkVector);

        //transform.eulerAngles = drunkVector;

        //float randomAngle = Random.Range(0f, 360f);
        //transform.eulerAngles = new Vector3(0f, 0f, randomAngle);

        //Quaternion randomRot = Random.rotation;
        //Vector3 euler = new Vector3(0f, 0f, randomRot.eulerAngles.z);
        //transform.rotation = Quaternion.Euler(euler);
    }


    private void RotateTowardForwardPoint() {

        TargetUtilities.RotateSmoothlyTowardTarget(seekPoint, transform, Owner.Stats[StatName.RotationSpeed]);
    }

    private void CheckForTargets()
    {
        if (CanMove == false)
            return;

        if(projectileOwner == null) {
            Debug.LogError("Projectile owner is null on: " + Owner.EntityName);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, seekRadius, projectileOwner.projectileHitMask);

        if (colliders != null && colliders.Length > 0)
        {
            Collider2D nearest = TargetUtilities.FindNearestTarget(colliders, transform);

            if (nearest != null)
                seekTarget = nearest.transform;
        }
    }

}
