using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIHelper
{


    public static void SetCanvasLayerOnTop(Canvas canvas) {
        if (canvas.overrideSorting == true && canvas.sortingOrder == 100)
            return;

        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
    }

    public static void ResetCanvasLayer(Canvas canvas, int baseLayer) {
        canvas.sortingOrder = baseLayer;
        canvas.overrideSorting = false;
    }


    public static void FadeCanvasGroup(CanvasGroup canvasGroup, float targetValue, float duration) {
        canvasGroup.DOFade(targetValue, duration);
    }

    public static void FadeInObject(GameObject target, float targetValue, float duration) {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if(canvasGroup == null) {
            Debug.LogError("Can't find canvas group on: " + target.name);
            return;
        }

        FadeCanvasGroup(canvasGroup, targetValue, duration);    
    }

}
