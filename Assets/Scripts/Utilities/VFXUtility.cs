using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VFXUtility 
{



    public static void DesaturateSprite(SpriteRenderer spriteRender, float amount) {

        Color baseColor = spriteRender.color;
        float r = baseColor.r * amount;
        float g = baseColor.g * amount;
        float b = baseColor.b * amount;


        spriteRender.color = new Color(r, g, b, baseColor.a);

    }

    public static void SpawnVFX(GameObject prefab, Vector2 location, Quaternion rotation, float destroyTimer = 0f) {
        if(prefab == null) 
            return;
        
        GameObject activeVFX = GameObject.Instantiate(prefab, location, rotation);
        if(destroyTimer > 0f) {
            GameObject.Destroy(activeVFX, destroyTimer);
        }
    }

    public static void SpawnVFX(GameObject prefab, Transform location, float destroyTiemr = 0f) {
        SpawnVFX(prefab, location.position, location.rotation, destroyTiemr);
    }


}
