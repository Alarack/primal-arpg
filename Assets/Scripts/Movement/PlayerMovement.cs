using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : EntityMovement
{

    //private TrailRenderer dashTrail;
    [SerializeField]
    private ParticleSystem dashParticles;

    private Vector2 direction;

    private PlayerInputActions playerInputActions;

    private Timer dashCooldownTimer;

    public float DashCooldownRatio { get { return dashCooldownTimer.Ratio; } }

    protected override void Awake() {
        base.Awake();
        //dashTrail = GetComponentInChildren<TrailRenderer>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

    }

    private void Start() {
        dashCooldownTimer = new Timer(Owner.Stats[StatName.DashCooldown], OnDashCooldownFinished, true);

    }

    private void OnEnable() {
        playerInputActions.Player.Dash.performed += OnDash;
        playerInputActions.Player.Potion.performed += OnPotion;
        //playerInputActions.Player.Look.performed += OnLook;
        //playerInputActions.Player.Move.performed += OnMove;
        //playerInputActions.Player.Move.canceled += OnMoveRelease;
    }

    private void OnDisable() {
        playerInputActions.Player.Dash.performed -= OnDash;
        playerInputActions.Player.Potion.performed -= OnPotion;
    }

    private void Update()
    {
        GetInput();

        if(CanDash == false && dashCooldownTimer != null) {
            dashCooldownTimer.UpdateClock();
        }
    }

    private void OnDash(InputAction.CallbackContext context) {
        Dash();
    }

    private void OnPotion(InputAction.CallbackContext context) {

        if (Owner.Stats.GetStatRangeRatio(StatName.Health) == 1)
            return;
        
        
        if (Owner.Stats[StatName.HeathPotions] > 0) {
            StatAdjustmentManager.ApplyStatAdjustment(Owner, -1, StatName.HeathPotions, StatModType.Flat, StatModifierData.StatVariantTarget.RangeCurrent, Owner, null);
            Owner.Stats.AdjustStatRangeByPercentOfMaxValue(StatName.Health, 0.75f, Owner);
            AudioManager.PlayPotionSound();
        }
    }




    protected override void Move()
    {
        RotateTowardMouse();

        if (Owner.IsChanneling() == true)
            return;


        float baseSpeed = Owner.Stats[StatName.MoveSpeed];

        float modifiedSpeed = baseSpeed * (1 + Owner.Stats[StatName.GlobalMoveSpeedModifier]);

        if(Owner.IsCasting() == true) {
            modifiedSpeed *= 1 + Owner.Stats[StatName.CastingMoveSpeedModifier];
        }


        Vector2 moveForce = new Vector2(direction.x, direction.y) * modifiedSpeed * Time.fixedDeltaTime;

        MyBody.AddForce(moveForce, ForceMode2D.Force);

        //Debug.Log("Speed: " + modifiedSpeed);
        SetMoveAnim(moveForce);
    }


    private void SetMoveAnim(Vector2 moveForce) {
        if(Owner.AnimHelper == null) {
            return;
        }

        bool moving = moveForce.magnitude > 0;


        Owner.AnimHelper.SetBool("Run", moving);
        
    }

    public void GetInput()
    {
        //direction.x = Input.GetAxisRaw("Horizontal");
        //direction.y = Input.GetAxisRaw("Vertical");
        direction.x = playerInputActions.Player.Move.ReadValue<Vector2>().x;
        direction.y = playerInputActions.Player.Move.ReadValue<Vector2>().y;

    }

    private void Dash() {
        if (CanMove == false || CanDash == false)
            return;

        CanMove = false;
        CanDash = false;
        IsDashing = true;
        //dashTrail.emitting = true;
        dashParticles.Play();

        Vector2 dashForce;

        if (MyBody.linearVelocity.magnitude <= 0f) {
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

        StartCoroutine(DashTimer());
    }

    private IEnumerator DashTimer() {
        WaitForSeconds waiter = new WaitForSeconds(Owner.Stats[StatName.DashDuration]);
        yield return waiter;
        MyBody.linearVelocity = Vector2.zero;
        CanMove = true;
        IsDashing = false;
        //dashTrail.emitting = false;
        dashParticles.Stop();
    }

    private void OnDashCooldownFinished(EventData data) {
        CanDash = true;

        EventData eventData = new EventData();

        EventManager.SendEvent(GameEvent.DashCooldownFinished, eventData);
    }


    private void OnLook(InputAction.CallbackContext context) {

 
        //Debug.Log("Control display Name: " + context.control.displayName);


        if (context.control.displayName == "Right Stick") {
            Vector2 lookVector = playerInputActions.Player.Look.ReadValue<Vector2>();
            float angle = Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);

            transform.rotation = targetRot;
        }

        if (context.control.displayName == "Delta") {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); //New Input System
            //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //OLD Input System
            transform.rotation = TargetUtilities.GetRotationTowardTarget(mousePos, transform.position);
        }

    }

    private void RotateTowardMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); //New Input System
        //Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //OLD Input System

        Owner.facingIndicator.transform.rotation = TargetUtilities.GetRotationTowardTarget(mousePos, Owner.facingIndicator.transform.position);

        //transform.rotation = TargetUtilities.GetRotationTowardTarget(mousePos, transform.position);

        if(Owner.mainSprite != null) {
            bool mouseLeft = mousePos.x < transform.position.x;

            Owner.mainSprite.flipX = mouseLeft;
        }

        //This is for analog stick
        //Vector2 lookVector = playerInputActions.Player.Look.ReadValue<Vector2>();
        //float angle = Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg - 90f;
        //Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);

        //transform.rotation = targetRot; 

    }


}
