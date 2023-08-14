using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AffixDatabase;

public class AffixDataManager : Singleton<AffixDataManager>
{

    public AffixDatabase affixDatabase;

    public static Dictionary<EliteAffixType, NPCEliteAffixData> eliteAffixDict = new Dictionary<EliteAffixType, NPCEliteAffixData>();



    private void Awake() {
        InitAffixDict();
    }

    private void InitAffixDict() {
        for (int i = 0; i < affixDatabase.eliteAffixdata.Count; i++) {
            eliteAffixDict.Add(affixDatabase.eliteAffixdata[i].type, affixDatabase.eliteAffixdata[i]);
        }
    }

    public static NPCEliteAffixData GetEliteAffixDataByType(EliteAffixType type) {
        return eliteAffixDict[type];
    }

    public static List<AbilityDefinition> GetAffixAbilities(EliteAffixType type) {
        return Instance.affixDatabase.GetAffixAbilities(type);
    }

}
