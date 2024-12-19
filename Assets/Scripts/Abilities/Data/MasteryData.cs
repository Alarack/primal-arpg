using System.Collections;
using System.Collections.Generic;
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
        public string featureName;
        public AbilityDefinition featureAbility;
        public List<AbilityDefinition> featurePathAbilities;
    }

}
