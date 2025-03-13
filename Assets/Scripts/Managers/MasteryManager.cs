using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasteryManager : Singleton<MasteryManager>
{




    public List<MasteryData> GetSavedMasteries() {
        List<MasteryData> loadedData = new List<MasteryData>();

        List<SaveData.MasteryDistributionData> savedMasteries = SaveLoadUtility.SaveData.savedMasteries;

        for (int i = 0; i < savedMasteries.Count; i++) {
            MasteryData masteryData = GameManager.Instance.masteryDatabase.GetMasteryDataByName(savedMasteries[i].masteryName);
            loadedData.Add(masteryData);
        }

        return loadedData;
    }

    public void LoadSavedMasteries() {
        List<SaveData.MasteryDistributionData> savedMasteries = SaveLoadUtility.SaveData.savedMasteries;

        for (int i = 0; i < savedMasteries.Count; i++) {
            MasteryData masteryData = GameManager.Instance.masteryDatabase.GetMasteryDataByName(savedMasteries[i].masteryName);
            foreach (var masteryDistribution in savedMasteries[i].distirbutionData) {

                MasteryData.MasteryFeatureData featureData = masteryData.GetFeatureByName(masteryDistribution.featureName);
                EntityManager.ActivePlayer.AbilityManager.LearnAbility(featureData.featureAbility.AbilityData, true);

                foreach (var pathAbilitData in masteryDistribution.abilityDistribution) {
                    AbilityData targetAbilityData = featureData.GetPathAbilityByName(pathAbilitData.Key).AbilityData;
                    EntityManager.ActivePlayer.AbilityManager.LearnAbility(targetAbilityData, true, pathAbilitData.Value);
                }
            }
        }
    }



}
