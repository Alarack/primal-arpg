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

    public void MoveTowardPoint(Vector2 location, float speedModifier = 1f) {
        ApplyMovement(location, speedModifier);
    }

    public void MoveAwayFromPoint(Vector2 location, float speedModifier = 1f) {
        ApplyMovement(location, -1f * speedModifier);
    }

    public void MovePerpendicularToPointClockwise(Vector2 location, float speedModifier = 1f) {
        ApplyPerpendicularMovement(location, speedModifier);
    }

    public void MovePerpendicularToPointAntiClockwise(Vector2 location, float speedModifier = 1f) {
        ApplyPerpendicularMovement(location, -1 * speedModifier);
    }

    public void StrafeTarget(float modifier = 1f) {
        if (currentTarget == null)
            return;

        MovePerpendicularToPointClockwise(currentTarget.transform.position, modifier);
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
        TargetUtilities.RotateSmoothlyTowardTarget(location, Owner.GetOriginPoint(), Owner.Stats[StatName.RotationSpeed]);

    }

    private Vector2 BasicMovement(Vector2 location) {
        Vector2 direction = location - (Vector2)transform.position;
        Vector2 moveForce = direction.normalized * Owner.Stats[StatName.MoveSpeed] * Time.fixedDeltaTime;

        bool animMove = moveForce.magnitude > 0f;

        if(Owner.AnimHelper != null) {
            Owner.AnimHelper.SetBool("Run", animMove);
        }

        

        return moveForce;
    }

    public Vector2 PerpendicularMovement(Vector2 location) {
        Vector2 direction = location - (Vector2)transform.position;
        Vector2 perp = Vector2.Perpendicular(direction);
        Vector2 moveForce = perp.normalized * Owner.Stats[StatName.MoveSpeed] * Time.fixedDeltaTime;

        return moveForce;
    }

    private void ApplyPerpendicularMovement(Vector2 location, float modifer = 1f) {
        Vector2 desiredForce = PerpendicularMovement(location) * modifer;
        MyBody.AddForce(desiredForce, ForceMode2D.Force);
    }

    private void ApplyMovement(Vector2 location, float modifier = 1f) {
        Vector2 desiredForce = BasicMovement(location) * modifier;
        MyBody.AddForce(desiredForce, ForceMode2D.Force);
    }


}
