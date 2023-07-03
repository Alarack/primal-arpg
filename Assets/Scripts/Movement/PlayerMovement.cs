using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : EntityMovement
{

    private TrailRenderer dashTrail;

    private Vector2 direction;

    private PlayerInputActions playerInputActions;


    protected override void Awake() {
        base.Awake();
        dashTrail = GetComponentInChildren<TrailRenderer>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void OnEnable() {
        playerInputActions.Player.Dash.performed += OnDash;
        //playerInputActions.Player.Look.performed += OnLook;
        //playerInputActions.Player.Move.performed += OnMove;
        //playerInputActions.Player.Move.canceled += OnMoveRelease;
    }

    private void OnDisable() {
        playerInputActions.Player.Dash.performed -= OnDash;
    }

    private void Update()
    {
        GetInput();
    }

    private void OnDash(InputAction.CallbackContext context) {
        Dash();
    }




    protected override void Move()
    {
        RotateTowardMouse();

        Vector2 moveForce = new Vector2(direction.x, direction.y) * Owner.Stats[StatName.MoveSpeed] * Time.fixedDeltaTime;

        MyBody.AddForce(moveForce, ForceMode2D.Force);
    }


    public void GetInput()
    {
        //direction.x = Input.GetAxisRaw("Horizontal");
        //direction.y = Input.GetAxisRaw("Vertical");
        direction.x = playerInputActions.Player.Move.ReadValue<Vector2>().x;
        direction.y = playerInputActions.Player.Move.ReadValue<Vector2>().y;

    }

    private void Dash() {
        if (CanMove == false)
            return;

        CanMove = false;
        IsDashing = true;
        dashTrail.emitting = true;
        Vector2 dashForce = MyBody.velocity.normalized * Owner.Stats[StatName.DashSpeed];
        MyBody.AddForce(dashForce, ForceMode2D.Impulse);

        StartCoroutine(DashTimer());
    }

    private IEnumerator DashTimer() {
        WaitForSeconds waiter = new WaitForSeconds(Owner.Stats[StatName.DashDuration]);
        yield return waiter;
        MyBody.velocity = Vector2.zero;
        CanMove = true;
        IsDashing = false;
        dashTrail.emitting = false;
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

        transform.rotation = TargetUtilities.GetRotationTowardTarget(mousePos, transform.position);


        //This is for analog stick
        //Vector2 lookVector = playerInputActions.Player.Look.ReadValue<Vector2>();
        //float angle = Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg - 90f;
        //Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);

        //transform.rotation = targetRot; 

    }


}
