using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : EntityMovement
{

    public enum MovementType {
        None,
        MoveTowardTarget,
        MoveAwayFromTarget
    }


    public MovementType moveType;


    private Transform currentTarget;


    public void SetTarget(Transform target) {

        if (currentTarget == target)
            return;

        currentTarget = target;
    }

    protected override void Move() {

        switch (moveType) {
            case MovementType.None:
                break;
            case MovementType.MoveTowardTarget:
                //MoveTowardTarget();
                break;

            case MovementType.MoveAwayFromTarget:
                //MoveAwayFromTarget();
                break;

        }
    }

    public void MoveTowardTarget() {
        if (currentTarget == null)
            return;

        MoveTowardPoint(currentTarget.transform.position);
    }

    public void MoveTowardPoint(Vector2 location) {
        ApplyMovement(location);
    }

    public void MoveAwayFromPoint(Vector2 location) {
        ApplyMovement(location, -1f);
    }


    public void MoveAwayFromTarget() {
        if (currentTarget == null)
            return;

        MoveAwayFromPoint(currentTarget.transform.position);
    }

    public void RotateTowardTarget() {
        if (currentTarget == null)
            return;

        RotateTowardPoint(currentTarget.transform.position);
    }

    public void RotateTowardPoint(Vector2 location) {
        TargetUtilities.RotateSmoothlyTowardTarget(location, Owner.transform, Owner.Stats[StatName.RotationSpeed]);

    }

    private Vector2 BasicMovement(Vector2 location) {
        Vector2 direction = location - (Vector2)transform.position;
        Vector2 moveForce = direction.normalized * Owner.Stats[StatName.MoveSpeed] * Time.fixedDeltaTime;

        return moveForce;
    }

    private void ApplyMovement(Vector2 location, float modifier = 1f) {
        Vector2 desiredForce = BasicMovement(location) * modifier;
        MyBody.AddForce(desiredForce, ForceMode2D.Force);
    }


}
