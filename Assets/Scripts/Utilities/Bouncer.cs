#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Events;



public class Bouncer : MonoBehaviour {

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;
    //public Transform shadowTransform;
    private Collider2D myCollider;

    [Header("Flight Variables")]
    public float maxYForce;
    public float minYForce;
    public float maxXForce;
    public float minXForce;
    public float fakeGravity = -100;

    [Header("Landing Event")]
    public UnityEvent onGroundHitUnityEvent;

    private Rigidbody2D rb;
    private Rigidbody2D[] myBodies;
    public bool IsGrounded { get; private set; }
    private Vector2 groundVelocity;
    private float verticalVelocity;
    private float lastVerticalVelocity;
    private int initalOrder;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        myBodies = GetComponentsInChildren<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();

        initalOrder = spriteRenderer.sortingOrder;
        
    }
    private void Start() {

        //yBounceValue = transform.localPosition.y + Random.Range()
        //Bounce();
        if (myCollider != null) {
            myCollider.enabled = false;
        }
        SetVelocities();
    }

    private void Update() {
        UpdatePositon();
        CheckGround();
#if DEBUG_MODE
        if (Input.GetKeyDown(KeyCode.B)) {
            DebugReset();
        }
#endif
    }


    private void DebugReset() {
        SetVelocities();
        lastVerticalVelocity = 0;
        onGroundHitUnityEvent.RemoveAllListeners();
        onGroundHitUnityEvent.AddListener(BounceAndReduce);
    }

    private void SetVelocities() {
        IsGrounded = false;
        groundVelocity = (Random.insideUnitCircle * Random.Range(minXForce, maxXForce)) /*+ new Vector2(Random.Range(minXForce, maxXForce), 0f)*/;
        verticalVelocity = Random.Range(minYForce, maxYForce);

        spriteRenderer.sortingOrder = 10;
        float randomSpin = Random.Range(-180f, 180f);
        foreach (var body in myBodies) {
            body.angularVelocity = randomSpin;
        }
    }

    private void UpdatePositon() {

        if (IsGrounded == false) {
            verticalVelocity += fakeGravity * Time.deltaTime;
            spriteRenderer.transform.position += new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;
        }



        transform.position += (Vector3)groundVelocity * Time.deltaTime;
    }

    private void PhysicsBounce() {
        float yForce = Random.Range(minYForce, maxYForce);
        float xForce = Random.Range(minXForce, maxXForce);

        Vector2 motion = new Vector2(xForce, yForce);


        rb.AddForce(motion, ForceMode2D.Impulse);
    }

    private void CheckGround() {
        if (spriteRenderer.transform.position.y < transform.position.y && IsGrounded == false) {

            spriteRenderer.transform.position = transform.position;

            IsGrounded = true;

            //onGroundHit?.Invoke();
            onGroundHitUnityEvent?.Invoke();
        }
    }

    public void StickToGround() {
        groundVelocity = Vector2.zero;
        IsGrounded = true;
        spriteRenderer.sortingOrder = initalOrder;

        myCollider.enabled = true;

    }

    public void BounceAndReduce() {
        IsGrounded = false;
        groundVelocity *= 0.8f;

        foreach (var body in myBodies) {
            body.angularVelocity *= 0.8f;
        }

        if (lastVerticalVelocity > 0) {
            lastVerticalVelocity *= 0.8f;
            verticalVelocity = lastVerticalVelocity;
        }
        else {
            verticalVelocity = Random.Range(minYForce, maxYForce) / 2f;
            lastVerticalVelocity = verticalVelocity;
        }


        if (verticalVelocity <= 5f) {
            onGroundHitUnityEvent.RemoveListener(BounceAndReduce);
            onGroundHitUnityEvent.AddListener(StickToGround);

            foreach (var body in myBodies) {
                body.angularVelocity = 0f;
            }
        }
    }

}
