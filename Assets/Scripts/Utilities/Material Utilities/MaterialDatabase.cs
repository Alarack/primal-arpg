using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Scriptable Objects/MaterialDatabase")]
public class MaterialDatabase : ScriptableObject
{
    public enum MaterialDesignation {
        Standard,
        OtherworldlyWarp
    }



    public SerializableDictionary<MaterialDesignation, MaterialDataEntry> materials = new SerializableDictionary<MaterialDesignation, MaterialDataEntry>();

    public Material GetMaterialByDesignation(MaterialDesignation designation) {
        if(materials.TryGetValue(designation, out MaterialDataEntry entry)) {
            return entry.material;
        }

        return null;
    }



    [System.Serializable]
    public class MaterialDataEntry {
        public Material material;
        public MaterialDesignation designation;

    }
}
