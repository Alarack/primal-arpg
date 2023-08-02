using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NPC Database")]
public class NPCDatabase : ScriptableObject
{


    public List<NPCBiomeEntry> biomeEntries = new List<NPCBiomeEntry>();


    public void Init(NPCDataManager manager) {
        for(int i = 0; i < biomeEntries.Count; i++) {
            biomeEntries[i].SetupThreatDict();
            manager.npcBiomeDict.Add(biomeEntries[i].biomeName, biomeEntries[i]);
        }
    }

    



    [System.Serializable]
    public class NPCBiomeEntry {
        public string biomeName;
        public List<NPCDataEntry> npcData = new List<NPCDataEntry>();

        public Dictionary<float, List<NPC>> npcsByThreat = new Dictionary<float, List<NPC>>();

        public NPCBiomeEntry() { 
        
        }

        public List<NPC> FillThreatList(float totalThreatLevel, float minIndividualThreat, float maxIndividualThreat) {
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

                    filledValue += entry.Key;
                    int randomIndex = Random.Range(0, entry.Value.Count);
                    results.Add(entry.Value[randomIndex]);

                    Debug.Log("Filled Value: " + filledValue + " : " + totalThreatLevel);

                    if (filledValue > totalThreatLevel)
                        break;
                }

                if(safetyCounter > 100) {
                    Debug.LogError("Infinite Loop in getting a threat list. No valid targets");
                    break;
                }

                safetyCounter++;

            }


            return results;
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

    }

    [System.Serializable]
    public class NPCDataEntry {
        public NPC npcPrefab;
        public float threatValue;
        public string biome;
    }

}
