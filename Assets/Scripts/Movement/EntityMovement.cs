using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovement : MonoBehaviour {

    public Entity Owner { get; protected set; }
    public Rigidbody2D MyBody { get; protected set; }
    public bool CanMove { get; set; }
    public bool IsDashing { get; protected set; }
    public bool CanDash { get; set; } = true;

    [Header("Dash Fields")]
    public ParticleSystem dashParticles;

    protected virtual void Awake() {
        CanMove = true;
        MyBody = GetComponent<Rigidbody2D>();
        Owner = GetComponent<Entity>();
    }


    protected virtual void FixedUpdate() {
        if (CanMove == true)
            Move();
    }


    protected virtual void Move() {

    }

    public virtual float IsMoving() {
        if (MyBody != null) {
            return MyBody.linearVelocity.magnitude;
        }

        return 0f;
    }

    public void StopMovement() {
        MyBody.linearVelocity = Vector3.zero;
        CanMove = false;
    }




    #region DASHING

    public void BeginDash() {
        if (CanMove == false || CanDash == false)
            return;

        CanMove = false;
        //CanDash = false;
        IsDashing = true;

        ToggleDashTrail(true);

        Vector2 dashForce;

        if (MyBody.linearVelocity.magnitude <= 0.02f) {
            Vector2 lookDirection = Owner.facingIndicator.transform.up;

            dashForce = lookDirection.normalized * Owner.Stats[StatName.DashSpeed];
        }
        else {
            dashForce = MyBody.linearVelocity.normalized * Owner.Stats[StatName.DashSpeed];
        }

        MyBody.AddForce(dashForce, ForceMode2D.Impulse);

        EventData data = new EventData();
        data.AddEntity("Entity", Owner);

        EventManager.SendEvent(GameEvent.DashStarted, data);

        RemoveCollisison();

        StartCoroutine(DashTimer());

        
    }

    protected IEnumerator DashTimer() {
        WaitForSecondsRealtime waiter = new WaitForSecondsRealtime(Owner.Stats[StatName.DashDuration]);
        yield return waiter;
        EndDash();

    }

    private void EndDash() {
        MyBody.linearVelocity = Vector2.zero;
        CanMove = true;
        IsDashing = false;
        //CanDash = true;
        RestoreCollisison();
        ToggleDashTrail(false);

        EventData data = new EventData();
        data.AddEntity("Entity", Owner);

        EventManager.SendEvent(GameEvent.DashEnded, data);
    }

    private void RemoveCollisison() {
        Physics2D.IgnoreLayerCollision(Owner.gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
        Physics2D.IgnoreLayerCollision(Owner.gameObject.layer, LayerMask.NameToLayer("Projectile"), true);
        Physics2D.IgnoreLayerCollision(Owner.gameObject.layer, LayerMask.NameToLayer("Effect Zone"), true);
    }

    private void RestoreCollisison() {
        Physics2D.IgnoreLayerCollision(Owner.gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
        Physics2D.IgnoreLayerCollision(Owner.gameObject.layer, LayerMask.NameToLayer("Projectile"), false);
        Physics2D.IgnoreLayerCollision(Owner.gameObject.layer, LayerMask.NameToLayer("Effect Zone"), false);
    }

    public void ForceDashEnd() {
        if (IsDashing == false)
            return;

        EndDash();
    }

    public void ToggleDashTrail(bool toggle) {
        if (dashParticles == null) {
            Debug.LogWarning(Owner.EntityName + " has no dash particles");
            return;
        }

        if(toggle == true)
            dashParticles.Play();
        else
            dashParticles.Stop();

    }


    #endregion




}
