using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/NPC Database", fileName ="NPC Database")]
public class NPCDatabase : ScriptableObject
{


    public List<NPCBiomeEntry> biomeEntries = new List<NPCBiomeEntry>();


    public void Init(NPCDataManager manager) {
        for(int i = 0; i < biomeEntries.Count; i++) {
            biomeEntries[i].SetupThreatDict();
            biomeEntries[i].SetupDataDict();
            manager.npcBiomeDict.Add(biomeEntries[i].biomeName, biomeEntries[i]);

            for(int j = 0; j < biomeEntries[i].npcData.Count; j++) {
                NPC current = biomeEntries[i].npcData[j].npcPrefab;

                manager.npcsByName.Add(current.EntityName, current);
                manager.threatTable.Add(current.EntityName, biomeEntries[i].npcData[j].threatValue);
            }

        }
    }

    



    [System.Serializable]
    public class NPCBiomeEntry {
        public string biomeName;
        public List<NPCDataEntry> npcData = new List<NPCDataEntry>();

        public Dictionary<float, List<NPC>> npcsByThreat = new Dictionary<float, List<NPC>>();

        public Dictionary<NPC, NPCDataEntry> dataDictionary = new Dictionary<NPC, NPCDataEntry>();

        public NPCBiomeEntry() { 
        
        }

        public NPC GetBoss() {
            List<NPC> bosses = new List<NPC>();

            for (int i = 0; i < npcData.Count; i++) {
                if (npcData[i].npcPrefab.subtypes.Contains(Entity.EntitySubtype.Boss)) {
                    bosses.Add(npcData[i].npcPrefab);
                }
            }

            int randomIndex = Random.Range(0, bosses.Count);
            return bosses[randomIndex];

        }


        public NPCDataEntry GetEntryFromNPC(NPC npc) {
            for (int i = 0; i < npcData.Count; i++) {
                if (npcData[i].npcPrefab.EntityName == npc.EntityName)
                    return npcData[i];
            }

            return null;
        }

        public bool CheckMinRoomIndexThreshold(NPC npc) {
            if(dataDictionary.TryGetValue(npc, out NPCDataEntry entry) == true) {
                return entry.CheckMinRoomThreshold();
            }

            return false;
        }

        public List<NPC> FillThreatList(float totalThreatLevel, float minIndividualThreat, float maxIndividualThreat, bool includeBosses = false) {
            float filledValue = 0f;
            List<NPC> results = new List<NPC>();

            int safetyCounter = 0;

            while(filledValue < totalThreatLevel) {

                foreach (var entry in npcsByThreat) {
                    if (entry.Key > totalThreatLevel)
                        continue;
                    if (entry.Key < minIndividualThreat)
                        continue;
                    if (entry.Key > maxIndividualThreat)
                        continue;

                    if(entry.Key < maxIndividualThreat) {
                        float reducedChance = 0.66f;
                        float roll = Random.Range(0f, 1f);

                        if(roll < reducedChance) {

                            int randomIndex = Random.Range(0, entry.Value.Count);
                            NPC target = entry.Value[randomIndex];

                            if (CheckMinRoomIndexThreshold(target) == true)
                                continue;

                            filledValue += entry.Key;
                            results.Add(target);

                        }
                    }
                    else {
                        int randomIndex = Random.Range(0, entry.Value.Count);
                        NPC target = entry.Value[randomIndex];

                        if (CheckMinRoomIndexThreshold(target) == true)
                            continue;

                        if (includeBosses == false && target.subtypes.Contains(Entity.EntitySubtype.Boss)) {
                            continue;
                        }
                        
                        filledValue += entry.Key;
                        results.Add(target);
                    }

                    //filledValue += entry.Key;
                    //int randomIndex = Random.Range(0, entry.Value.Count);
                    //results.Add(entry.Value[randomIndex]);

                    //Debug.Log("Filled Value: " + filledValue + " : " + totalThreatLevel);

                    if (filledValue > totalThreatLevel)
                        break;
                }

                if(safetyCounter > 100) {
                    Debug.LogError("Infinite Loop in getting a threat list. No valid targets");
                    break;
                }

                safetyCounter++;

            }
            //Debug.Log("Filled Value: " + filledValue + " : " + totalThreatLevel);

            if (results.Count == 1) {
                if (results[0].subtypes.Contains(Entity.EntitySubtype.Buffer)) {

                    int threat = (int)NPCDataManager.GetThreatLevel(results[0].EntityName);

                    results.Add(GetTargetOfEqualOrLesserThreat(threat, Entity.EntitySubtype.Buffer));
                }
            }

            return results;
        }


        private NPC GetTargetOfEqualOrLesserThreat(float threat, params Entity.EntitySubtype[] exceptions) {
            List<NPC> results = new List<NPC>();
            
            foreach (NPCDataEntry npc in npcData) {
                if (npc.threatValue > threat)
                    continue;

                if (npc.ContainsSubtype(exceptions))
                    continue;

                results.Add(npc.npcPrefab);
            }

            results.Shuffle();

            return results[Random.Range(0, results.Count)];
        }

        public void SetupThreatDict() {
            npcsByThreat.Clear();

            for (int i = 0; i < npcData.Count; i++) {
                if (npcsByThreat.TryGetValue(npcData[i].threatValue, out List<NPC> list) == true) {
                    list.Add(npcData[i].npcPrefab);
                }
                else {
                    npcsByThreat.Add(npcData[i].threatValue, new List<NPC> { npcData[i].npcPrefab });
                }
            }
        }

        public void SetupDataDict() {
            dataDictionary.Clear();
            for (int i = 0; i < npcData.Count; i++) {
                dataDictionary.Add(npcData[i].npcPrefab, npcData[i]);
            }
        }

    }

    [System.Serializable]
    public class NPCDataEntry {
        public NPC npcPrefab;
        public float threatValue;
        public int roomCountThreshold = -1;
        public List<Entity.EntitySubtype> subTypes = new List<Entity.EntitySubtype>();
        //public string biome;



        public bool CheckMinRoomThreshold() {
            if (roomCountThreshold < 0)
                return false;


            return RoomManager.Instance.CurrentRoomIndex +1 < roomCountThreshold;
        }

        public bool ContainsSubtype(Entity.EntitySubtype[] subtypes) {
            for (int i = 0; i < subTypes.Count; i++) {
                if(subTypes.Contains(subtypes[i])) return 
                        true;
            }

            return false;
        }
    }

}
