using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Affix Database")]
public class ItemAffixDatabase : ScriptableObject
{


    public List<ItemAffixData> itemAffixData = new List<ItemAffixData>();


    public Sprite GetAffixRaritySprite(int tier) {
        for (int i = 0; i < itemAffixData.Count; i++) {
            if (itemAffixData[i].tier == tier)
                return itemAffixData[i].rarityIcon;
        }

        return null;
    }

    public Color GetAffixColor(int tier) {
        for (int i = 0; i < itemAffixData.Count; i++) {
            if (itemAffixData[i].tier == tier)
                return itemAffixData[i].Color;
        }

        return Color.white;
    }

    public string GetAffixRarityName(int tier) {
        for (int i = 0; i < itemAffixData.Count; i++) {
            if (itemAffixData[i].tier == tier)
                return itemAffixData[i].name;
        }

        return "Unknown";
    }

    public Gradient GetRarityGradient(int tier) {
        for (int i = 0; i < itemAffixData.Count; i++) {
            if (itemAffixData[i].tier == tier)
                return itemAffixData[i].backgroundGradient;
        }

        return null;
    }




    [System.Serializable]
    public class ItemAffixData {
        public string name;
        public int tier;
        public Sprite rarityIcon;
        public Color Color;
        public Gradient backgroundGradient;
    }

}
