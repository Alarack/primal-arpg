using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{

    public Entity Owner { get; protected set; }
    public Rigidbody2D MyBody { get; protected set; }
    public bool CanMove { get; set; }

    protected virtual void Awake()
    {
        CanMove = true;
        MyBody = GetComponent<Rigidbody2D>();
        Owner = GetComponent<Entity>();
    }


    protected virtual void FixedUpdate()
    {
        if(CanMove == true)
            Move();
    }


    protected virtual void Move()
    {

    }

}
