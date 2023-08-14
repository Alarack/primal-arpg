using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Affix Database", fileName = "Affix Database")]
public class AffixDatabase : ScriptableObject {

    public enum EliteAffixType {
        None,
        Overcharged
    }



    [Header("Elite Affixes")]
    public List<NPCEliteAffixData> eliteAffixdata = new List<NPCEliteAffixData>();




    public List<AbilityDefinition> GetAffixAbilities(EliteAffixType type) {
        for (int i = 0; i < eliteAffixdata.Count; i++) {
            if (eliteAffixdata[i].type == type)
                return eliteAffixdata[i].abilities;   
        }

        return null; 
    }





    [System.Serializable]
    public class NPCEliteAffixData {
        public EliteAffixType type;
        public float threatModifier;
        public GameObject vfxPrefab;
        public List<AbilityDefinition> abilities = new List<AbilityDefinition>();
    }

}
