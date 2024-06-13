using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class ColorUtilities {


    public static Color MoveTowards(this Color color, Color endColor, float speed) {

        Color result = new Color(
          Mathf.MoveTowards(color.r, endColor.r, speed * 2 * Time.deltaTime),
          Mathf.MoveTowards(color.g, endColor.g, speed * 2 * Time.deltaTime),
          Mathf.MoveTowards(color.b, endColor.b, speed * 2 * Time.deltaTime),
          Mathf.MoveTowards(color.a, endColor.a, speed * 2 * Time.deltaTime));

        return result;
    }


    private static void ColorTest() {
        Color test = Color.white;

        test = test.MoveTowards(Color.blue, 5f);

    }

}
