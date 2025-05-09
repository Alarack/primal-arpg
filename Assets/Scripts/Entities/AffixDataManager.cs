using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AffixDatabase;

public class AffixDataManager : Singleton<AffixDataManager>
{

    public AffixDatabase affixDatabase;

    public static Dictionary<EliteAffixType, NPCEliteAffixData> eliteAffixDict = new Dictionary<EliteAffixType, NPCEliteAffixData>();



    [RuntimeInitializeOnLoadMethod]
    private static void InitAffixDict() {

        //Debug.Log("Initing Elite Affix Dict");
        
        eliteAffixDict = new Dictionary<EliteAffixType, NPCEliteAffixData>();
        
        for (int i = 0; i < Instance.affixDatabase.eliteAffixdata.Count; i++) {
            eliteAffixDict.Add(Instance.affixDatabase.eliteAffixdata[i].type, Instance.affixDatabase.eliteAffixdata[i]);
        }
    }

    public static NPCEliteAffixData GetEliteAffixDataByType(EliteAffixType type) {
        return eliteAffixDict[type];
    }

    public static List<AbilityDefinition> GetAffixAbilities(EliteAffixType type) {
        return Instance.affixDatabase.GetAffixAbilities(type);
    }

}
