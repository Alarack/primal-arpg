using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXUtility : Singleton<VFXUtility> 
{

    public Debris debrisTemplate;
    public MaterialDatabase materialDatabase;


    public static Material GetMaterialByDesignation(MaterialDatabase.MaterialDesignation designation) {
        return Instance.materialDatabase.GetMaterialByDesignation(designation);
    }

    public static void DesaturateSprite(SpriteRenderer spriteRender, float amount) {

        Color baseColor = spriteRender.color;
        float r = baseColor.r * amount;
        float g = baseColor.g * amount;
        float b = baseColor.b * amount;


        spriteRender.color = new Color(r, g, b, baseColor.a);

    }

    public static GameObject SpawnVFX(GameObject prefab, Vector2 location, Quaternion rotation, Transform parent = null, float destroyTimer = 0f, float scaleModifier = 1f, bool variance = false) {
        if(prefab == null) 
            return null;


        Vector2 loc = location;
        if (variance)
            loc = new Vector2(location.x + Random.Range(-0.5f, 0.5f), location.y + Random.Range(-0.5f, 0.5f));


        GameObject activeVFX = GameObject.Instantiate(prefab, loc, rotation);
        if(parent != null) {
            activeVFX.transform.SetParent(parent.transform, false);
            activeVFX.transform.localPosition = Vector3.zero;
        }

        activeVFX.transform.localScale *= scaleModifier;


        if(destroyTimer > 0f) {
            GameObject.Destroy(activeVFX, destroyTimer);
        }

        //Debug.LogWarning("Spawned: " + activeVFX.name);

        return activeVFX;
    }

    public static GameObject SpawnVFX(GameObject prefab, Transform location, Transform parent, float destroyTiemr = 0f, float scaleModifer = 1f) {
        return SpawnVFX(prefab, location.position, location.rotation, parent, destroyTiemr, scaleModifer);
    }

    public static GameObject SpawnVFX(GameObject prefab, Transform parent, float destroyTiemr = 0f, float scaleModifier = 1f) {
        return SpawnVFX(prefab, parent.position, parent.rotation, parent, destroyTiemr, scaleModifier);
    }


    public static void SpawnDebirs(int count, Vector2 location) {
        for (int i = 0; i < count; i++) {
            Instantiate(Instance.debrisTemplate, location, Quaternion.identity);
        }
    }

}
