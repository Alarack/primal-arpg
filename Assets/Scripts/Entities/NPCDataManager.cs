using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NPCDatabase;

public class NPCDataManager : Singleton<NPCDataManager>
{

    public NPCDatabase database;
    public Dictionary<string, NPCBiomeEntry> npcBiomeDict = new Dictionary<string, NPCBiomeEntry>();

    public Dictionary<string, NPC> npcsByName = new Dictionary<string, NPC>();
    public Dictionary<string, float> threatTable = new Dictionary<string, float>();


    private void Awake() {
        database.Init(this);


        //List<NPC> testList = GetSpawnList("Grasslands", 10, 1, 5);

        //foreach (NPC test in testList) {
        //    Debug.Log(test.gameObject.name + " selected");
        //}
    }

    public static NPC GetNPCPrefabByName(string name) {
        if(Instance.npcsByName.TryGetValue(name, out NPC npc)) 
            return npc;

        return null;
    }

    public static float GetThreatLevel(string name) {
        if (Instance.threatTable.TryGetValue(name, out float threat))
            return threat;

        return -1f;
    }

    public static List<NPC> GetSpawnList(string biome, float totalThreat, float minSingleThreat, float maxSingleThreat) {
        if(Instance.npcBiomeDict.TryGetValue(biome, out NPCBiomeEntry biomeEntry)) {
            return biomeEntry.FillThreatList(totalThreat, minSingleThreat, maxSingleThreat);
        }

        return null;
    }

    public static NPC GetBoss(string biome) {
        if (Instance.npcBiomeDict.TryGetValue(biome, out NPCBiomeEntry biomeEntry)) {
            return biomeEntry.GetBoss();
        }

        return null;
    }

}
