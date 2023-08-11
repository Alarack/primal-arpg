using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public static class LayerTools {


    public static bool IsLayerInMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    public static LayerMask AddToMask(LayerMask mask, int layer) {
        //Debug.Log("Adding: " + LayerMask.LayerToName(layer));
        
       return mask |= (1 << layer);
    }

    public static LayerMask RemoveFromMask(LayerMask mask, int layer) {
        return mask &= ~(1 << layer);
    }

    //public static void AddToMask(this LayerMask mask, int layer) {
    //    mask |= (1 << layer);
    //}

    public static LayerMask SetupHitMask(LayerMask mask, int sourceLayer) {
        //int sourceLayer = source.gameObject.layer;

        //Debug.Log("Layer: " + LayerMask.LayerToName(sourceLayer));

        switch (LayerMask.LayerToName(sourceLayer)) {
            case "Enemy":
                return mask = AddToMask(mask, LayerMask.NameToLayer("Player"));

            case "Player":
                return mask = AddToMask(mask, LayerMask.NameToLayer("Enemy"));

            default:
                return mask;
        }
    }


}
