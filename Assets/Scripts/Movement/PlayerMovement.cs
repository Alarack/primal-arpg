using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : EntityMovement
{


    private Vector2 direction;


    private void Update()
    {
        GetInput();

        if (Input.GetButtonDown("Jump") == true)
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
        direction.x = Input.GetAxisRaw("Horizontal");
        direction.y = Input.GetAxisRaw("Vertical");
    }

    private void Dash() {
        if (CanMove == false)
            return;

        CanMove = false;
        Vector2 dashForce = MyBody.velocity.normalized * Owner.Stats[StatName.DashSpeed];
        MyBody.AddForce(dashForce, ForceMode2D.Impulse);

        StartCoroutine(DashTimer());
    }

    private IEnumerator DashTimer() {
        WaitForSeconds waiter = new WaitForSeconds(Owner.Stats[StatName.DashDuration]);
        yield return waiter;
        MyBody.velocity = Vector2.zero;
        CanMove = true;
    }

    private void RotateTowardMouse()
    {
        Vector2 mousPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.rotation = TargetUtilities.GetRotationTowardTarget(mousPos, transform.position);

        //transform.rotation = TargetUtilities.GetRotationTowardTarget2(mousPos, transform.position);

    }

}
