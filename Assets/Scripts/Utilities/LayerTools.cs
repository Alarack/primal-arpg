using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerTools {


    public static bool IsLayerInMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }




}
