using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MasteryData 
{

    public string masteryName;

    public List<MasteryFeatureData> features = new List<MasteryFeatureData>();




    [System.Serializable]
    public class MasteryFeatureData {
        public string featureName;
        public AbilityDefinition featureAbility;
        public List<AbilityDefinition> featurePathAbilities;
    }

}
