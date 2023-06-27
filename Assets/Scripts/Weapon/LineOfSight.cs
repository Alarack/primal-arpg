using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{

    public float range = -1f;
    public LayerMask mask;


    public bool Hit { get; private set; }



    private void Update() {

        float actualRange = range < 0f ? Mathf.Infinity : range;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, actualRange, mask);

        Hit = hit.collider != null;
    }




}
