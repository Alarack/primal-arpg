using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;

public class LightTools : MonoBehaviour
{

    public float minRadius;
    public float maxRadius;

    private Light2D myLight;

    private float fadeTime;
    private bool fading;
    private void Awake() {
        myLight = GetComponent<Light2D>();


        myLight.pointLightOuterRadius = Random.Range(minRadius, maxRadius);

        //SetFadeTime(1f, 0);
    }

    private void OnEnable() {
        //SetFadeTime(1f, 1f);
    }

    public void SetFadeTime(float fadeTime, float targetAlpha) {
        this.fadeTime = fadeTime;

        if(fading == false)
            new Task(FadeOut(targetAlpha));
    }

    public void SetAlpha(float targetAlpha) {
        myLight.color = new Color(1f, 1f, 1f, targetAlpha);
    }

    private IEnumerator FadeOut(float targetAlpha) {
        fading = true;

        WaitForEndOfFrame waiter = new WaitForEndOfFrame();
        while(myLight.color.a != targetAlpha) {
            float desiredAlpha = Mathf.MoveTowards(myLight.color.a, targetAlpha, Time.deltaTime * (1 / fadeTime));
            myLight.color = new Color(1f, 1f, 1f, desiredAlpha);

            yield return waiter;
        }

        fading = false;
        //if(myLight.color.a == 1f) {
        //    SetFadeTime(1f, 0f);
        //}
    }



}
