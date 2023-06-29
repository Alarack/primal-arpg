using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VFXUtility 
{



    public static void SpawnVFX(GameObject prefab, Vector2 location, Quaternion rotation, float destroyTimer = 0f) {
        GameObject activeVFX = GameObject.Instantiate(prefab, location, rotation);
        if(destroyTimer > 0f) {
            GameObject.Destroy(activeVFX, destroyTimer);
        }
    }

    public static void SpawnVFX(GameObject prefab, Transform location, float destroyTiemr = 0f) {
        SpawnVFX(prefab, location.position, location.rotation, destroyTiemr);
    }


}
