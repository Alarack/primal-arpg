using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ElectricArcEffect : MonoBehaviour
{

    public VisualEffect effect;

    public Entity source;
    public Entity target;


    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform pos4;

    private void Start() {
        //SetPositions(source, target);
    }


    [ContextMenu("Reset Lightning")]
    public void ResetPos() {
        SetPositions(source, target);

        

    }


    public void SetPositions(Vector3 start, Vector3 end) {

        effect.SetFloat("Thickness", Random.Range(0.1f, 0.8f));

        pos1.position = start;
        pos4.position = end;

        float upperBound = Random.Range(2f, 5f);
        float lowerBound = Random.Range(-2f, -5f);

        int roll = Random.Range(0, 2);

        Vector2 randomOffset = roll > 0 ? new Vector2(0f, upperBound) : new Vector2(0f, lowerBound);
        Vector2 midPoint = Vector2.Lerp(start, end, 0.5f);
        Vector2 perp = midPoint + randomOffset; // (Random.insideUnitCircle * 3f);

        pos2.position = perp;
        pos3.position = perp;
    }

    public void SetPositions(Entity source, Entity target) {

        SetPositions(source.transform.position, target.transform.position);

        //pos1.position = source.transform.position;
        //pos4.position = target.transform.position;

        //float upperBound = Random.Range(2f, 5f);
        //float lowerBound = Random.Range(-2f, -5f);

        //int roll = Random.Range(0, 2);

        //Vector2 randomOffset = roll > 0 ? new Vector2(0f, upperBound) : new Vector2(0f, lowerBound);


        //Vector2 midPoint = Vector2.Lerp(source.transform.position, target.transform.position, 0.5f);
        //Vector2 perp = midPoint + randomOffset; // (Random.insideUnitCircle * 3f);



        //pos2.position = perp;
        //pos3.position = perp;

        //effect.SetVector3("Pos1", source.transform.position);
        //effect.SetVector3("Pos4", target.transform.position);

        //Vector2 midPoint = Vector2.Lerp(source.transform.position, target.transform.position, 0.5f);

        //Vector2 perp = Vector2.Perpendicular(midPoint).normalized;

        //effect.SetVector3("Pos2", perp);
        //effect.SetVector3("Pos3", perp);

    }



}
