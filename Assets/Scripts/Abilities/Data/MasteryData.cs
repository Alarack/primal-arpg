using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class MasteryData 
{
    
    public string masteryName;

    public List<MasteryFeatureData> features = new List<MasteryFeatureData>();

    public MasteryFeatureData GetFeatureByName(string name) {
        for (int i = 0; i < features.Count; i++) {
            if (features[i].featureName == name)
                return features[i];
        }

        return null;
    }


    [System.Serializable]
    public class MasteryFeatureData {
        public bool dev;
        public string featureName;
        public AbilityDefinition featureAbility;
        public List<AbilityDefinition> featurePathAbilities;


        public AbilityDefinition GetPathAbilityByName(string name) {
            for (int i = 0; i < featurePathAbilities.Count; i++) {
                if (featurePathAbilities[i].AbilityData.abilityName == name) 
                    return featurePathAbilities[i];
            }

            return null;
        }

        public List<AbilityDefinition> GetNamedAbilityData(List<string> abilityNames) {
            List<AbilityDefinition> results = new List<AbilityDefinition>();

            for (int i = 0; i < featurePathAbilities.Count; i++) {
                if (abilityNames.Contains(featurePathAbilities[i].AbilityData.abilityName))
                    results.Add(featurePathAbilities[i]);
            }

            return results;
        }
    }

}
