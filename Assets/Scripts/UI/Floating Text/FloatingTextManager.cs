using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : Singleton<FloatingTextManager> {

    public FloatingText prefab;


   

    public static FloatingText SpawnFloatingText(Vector2 location, string value, float scale = 0.75f) {

        if(Instance.prefab == null) {
            Debug.LogWarning("No prefab has been assinged to the floating text manager");
            return null;
        }

        FloatingText newFloatingText = Instantiate(Instance.prefab, location, Quaternion.identity);
        newFloatingText.transform.localScale *= scale;
        newFloatingText.Setup(value);

        return newFloatingText;
    }

}
