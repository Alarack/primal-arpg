using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData 
{

    public int primalEssencePoints;

    public List<MasteryDistributionData> savedMasteries = new List<MasteryDistributionData>();

    public int CountOfMasteries { get { return savedMasteries.Count; } }

    public void SetMasteryPoints(int value) {
        primalEssencePoints = value;
    }


    public bool CheckForDuplicateMastery(string masteryName) {
        MasteryDistributionData existingData = GetMasteryDistribution(masteryName);

        if (existingData != null) {
            PanelManager.OpenPanel<PopupPanel>().Setup("Existing Mastery", "Only one mastery per Element is allowed");
            return true;
        }

        return false;
    }

    public void AddMastery(string masteryName, string featureName, string mainAbilityName) {

        if (CheckForDuplicateMastery(masteryName) == true)
            return;
        
        MasteryDistributionData data = new MasteryDistributionData(masteryName, featureName, mainAbilityName);
        savedMasteries.Add(data);
    }

    public void UpdateMasteryPath(string masteryName, string featureName, string abilityName, int level) {
        MasteryDistributionData targetData = GetMasteryDistribution(masteryName);

        if(targetData == null) {
            Debug.LogError("Can't Find: " + masteryName + " in data");
            return;
        }
        targetData.SetPathAbility(featureName, abilityName, level);
    }

    public void RemoveMasteryPath(string masteryName, string featureName, string abilityName) {
        MasteryDistributionData targetData = GetMasteryDistribution(masteryName);

        if (targetData == null) {
            Debug.LogError("Can't Find: " + masteryName + " in data");
            return;
        }

        targetData.RemovePathAbility(featureName, abilityName);
    }

    public void RemoveMasteryFeature(string masteryName, string featureName) {
        MasteryDistributionData targetData = GetMasteryDistribution(masteryName);

        if (targetData == null) {
            Debug.LogError("Can't Find: " + masteryName + " in data");
            return;
        }

        targetData.RemoveFeature(featureName);
        savedMasteries.Remove(targetData);
    }


    private MasteryDistributionData GetMasteryDistribution(string masteryName) {
        for (int i = 0; i < savedMasteries.Count; i++) {
            if (savedMasteries[i].masteryName == masteryName) {
                return savedMasteries[i];
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
        public List <MasteryFeatureDistirbutionData> distirbutionData = new List<MasteryFeatureDistirbutionData> ();

        public MasteryDistributionData() { 
        
        }

        public MasteryDistributionData(string masteryName, string featureName, string mainAbilityName) {
            this.masteryName = masteryName;
           
            MasteryFeatureDistirbutionData feature = new MasteryFeatureDistirbutionData(featureName, mainAbilityName);
            distirbutionData.Add(feature);

        }


        public void SetPathAbility(string featureName, string abilityName, int level) {
          MasteryFeatureDistirbutionData targetData = GetFeatureByName(featureName);

            if(targetData != null) {
                targetData.SetPathAbility(abilityName, level);
            }
        }

        public void RemovePathAbility(string featureName, string abilityName) {
            MasteryFeatureDistirbutionData targetData = GetFeatureByName(featureName);

            if (targetData != null) {
                targetData.RemovePathAbility(abilityName);
            }
        }

        public MasteryFeatureDistirbutionData GetFeatureByName(string featureName) {
            for (int i = 0; i < distirbutionData.Count; i++) {
                if (distirbutionData[i].featureName == featureName) {
                    return distirbutionData[i];
                }
            }

            return null;
        }

        public void RemoveFeature(string featureName) {
            MasteryFeatureDistirbutionData targetData = GetFeatureByName(featureName);
            if (targetData != null) {
                distirbutionData.Remove(targetData);
            }
        }


    }


    public class MasteryFeatureDistirbutionData {
        public string featureName;
        public string mainAbilityName;

        public Dictionary<string, int> abilityDistribution = new Dictionary<string, int>();

        public MasteryFeatureDistirbutionData() {

        }

        public MasteryFeatureDistirbutionData(string featureName, string mainAbilityName) {
            this.mainAbilityName = mainAbilityName;
            this.featureName = featureName;
        }


        public void SetPathAbility(string name, int level) {
            if (abilityDistribution.TryGetValue(name, out int value)) {
                abilityDistribution[name] = level;
            }
            else {
                abilityDistribution.Add(name, level);
            }
        }

        public void RemovePathAbility(string name) {
            if (abilityDistribution.ContainsKey(name) == true) {
                abilityDistribution.Remove(name);
            }
        }
    }



}



