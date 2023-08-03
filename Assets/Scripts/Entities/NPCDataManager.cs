using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NPCDatabase;

public class NPCDataManager : Singleton<NPCDataManager>
{

    public NPCDatabase database;
    public Dictionary<string, NPCBiomeEntry> npcBiomeDict = new Dictionary<string, NPCBiomeEntry>();



    private void Awake() {
        database.Init(this);


        //List<NPC> testList = GetSpawnList("Grasslands", 10, 1, 5);

        //foreach (NPC test in testList) {
        //    Debug.Log(test.gameObject.name + " selected");
        //}
    }

    public static List<NPC> GetSpawnList(string biome, float totalThreat, float minSingleThreat, float maxSingleThreat) {
        if(Instance.npcBiomeDict.TryGetValue(biome, out NPCBiomeEntry biomeEntry)) {
            return biomeEntry.FillThreatList(totalThreat, minSingleThreat, maxSingleThreat);
        }

        return null;
    }

}
