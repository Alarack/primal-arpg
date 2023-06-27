using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Drunk Variables")]
    public float drunkInterval;
    public float drunkVariance;
    private Timer drunkTimer;

    private Quaternion lastDrunkRotation;


    protected override void Awake()
    {
        base.Awake();

        if (movementBehavior == MovementBehavior.Drunk)
        {
            RandomizeDirection(null);
            drunkTimer = new Timer(drunkInterval, RandomizeDirection, true);
        }


    }

    private void Update()
    {
        if (movementBehavior == MovementBehavior.Drunk && drunkTimer != null)
        {
            drunkTimer.UpdateClock();
        }
    }

    protected override void Move()
    {
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
        MyBody.AddForce(transform.up * Owner.Stats[StatName.MoveSpeed] * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    private void MoveSeeking()
    {
        MoveStraight();
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


    private void CheckForTargets()
    {
        if (CanMove == false)
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, seekRadius, targetLayers);

        if (colliders != null && colliders.Length > 0)
        {
            Collider2D nearest = TargetUtilities.FindNearestTarget(colliders, transform);

            if (nearest != null)
                seekTarget = nearest.transform;
        }
    }

}
