using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDatabase : ScriptableObject
{


    public List<NPCBiomeEntry> biomeEntries = new List<NPCBiomeEntry>();






    [System.Serializable]
    public class NPCBiomeEntry {
        public string biomeName;
        public List<NPCDataEntry> npcData = new List<NPCDataEntry>();

        public Dictionary<float, List<NPC>> npcsByThreat = new Dictionary<float, List<NPC>>();

        public NPCBiomeEntry() { 
        
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
