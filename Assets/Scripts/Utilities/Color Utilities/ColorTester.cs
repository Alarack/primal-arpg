using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorTester : MonoBehaviour
{

    public Color targetColor;
    public float fadeSpeed = 1.0f;

    private Image testImage;

    private void Awake() {
        testImage = GetComponent<Image>();
    }

    private void Start() {
        StartCoroutine(FadeColor());
    }


    private IEnumerator FadeColor() {

        while(testImage.color != targetColor) {
            testImage.color = testImage.color.MoveTowards(targetColor, fadeSpeed);
            yield return new WaitForEndOfFrame();
        }

    }



}
