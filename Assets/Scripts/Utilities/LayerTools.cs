using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MaskTargeting {
    Opposite,
    Same,
    Both
}


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

    public static LayerMask SetupHitMask(LayerMask mask, int sourceLayer, MaskTargeting targeting = MaskTargeting.Opposite) {
        //int sourceLayer = source.gameObject.layer;

        //Debug.Log("Layer: " + LayerMask.LayerToName(sourceLayer));


        mask = LayerMask.LayerToName(sourceLayer) switch {
            "Enemy" when targeting == MaskTargeting.Opposite => AddToMask(mask, LayerMask.NameToLayer("Player")),
            "Enemy" when targeting == MaskTargeting.Same => AddToMask(mask, LayerMask.NameToLayer("Enemy")),
            "Player" when targeting == MaskTargeting.Opposite => AddToMask(mask, LayerMask.NameToLayer("Enemy")),
            "Player" when targeting == MaskTargeting.Same => AddToMask(mask, LayerMask.NameToLayer("Player")),
            "Orbital" when targeting == MaskTargeting.Opposite => AddToMask(mask, LayerMask.NameToLayer("Enemy")),
            "Orbital" when targeting == MaskTargeting.Same => AddToMask(mask, LayerMask.NameToLayer("Player")),
            
            _ => mask,
        };


        //switch (LayerMask.LayerToName(sourceLayer)) {
        //    case "Enemy":
        //        return mask = AddToMask(mask, LayerMask.NameToLayer("Player"));

        //    case "Player":
        //        return mask = AddToMask(mask, LayerMask.NameToLayer("Enemy"));

        //    default:
        //        return mask;
        //}



        return mask;

    }


}
