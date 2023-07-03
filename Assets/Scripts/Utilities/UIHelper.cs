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




}
