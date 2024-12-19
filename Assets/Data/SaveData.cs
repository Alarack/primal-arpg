using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData 
{

    public int primalEssencePoints;


    public List<MasteryDistributionData> distributions = new List<MasteryDistributionData>();

    public int CountOfMasteries { get { return distributions.Count; } }

    public void SetMasteryPoints(int value) {
        primalEssencePoints = value;
    }

    public void AddMastery(string masteryName, string mainAbilityName) {
        MasteryDistributionData data = new MasteryDistributionData(masteryName, mainAbilityName);
        distributions.Add(data);
    }

    public void UpdateMasteryPath(string masteryName, string abilityName, int level) {
        MasteryDistributionData targetData = GetMasteryDistribution(masteryName);

        if(targetData == null) {
            Debug.LogError("Can't Find: " + masteryName + " in data");
            return;
        }
        targetData.SetPathAbility(abilityName, level);
    }

    public void RemoveMasteryPath(string masteryName, string abilityName) {
        MasteryDistributionData targetData = GetMasteryDistribution(masteryName);

        if (targetData == null) {
            Debug.LogError("Can't Find: " + masteryName + " in data");
            return;
        }

        targetData.RemovePathAbility(abilityName);
    }


    private MasteryDistributionData GetMasteryDistribution(string masteryName) {
        for (int i = 0; i < distributions.Count; i++) {
            if (distributions[i].masteryName == masteryName) {
                return distributions[i];
            }
        }

        return null;
    }


    public string ToJSON() {

        string jsonData = JsonConvert.SerializeObject(this);

        Debug.Log("Saved Player JSON: " + jsonData);

        return jsonData;
    }

    public static SaveData FromJSON(string json) {
        return JsonConvert.DeserializeObject<SaveData>(json);
    }





    public class MasteryDistributionData {

        public string masteryName;
        public string mainAbilityName;

        public Dictionary<string, int> abilityDistribution = new Dictionary<string, int>();


        public MasteryDistributionData() { 
        
        }

        public MasteryDistributionData(string masteryName, string mainAbilityName) {
            this.masteryName = masteryName;
            this.mainAbilityName = mainAbilityName;
        }


        public void SetPathAbility(string name, int level) {
            if(abilityDistribution.TryGetValue(name, out int value)) {
                abilityDistribution[name] = level;
            }
            else {
                abilityDistribution.Add(name, level);
            }
        }

        public void RemovePathAbility(string name) {
            if(abilityDistribution.ContainsKey(name) == true) {
                abilityDistribution.Remove(name);
            }
        }


    }


}



