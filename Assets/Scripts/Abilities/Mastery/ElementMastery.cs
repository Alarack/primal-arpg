using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementMastery {
    public string MasteryName { get { return Data.masteryName; } }

    public MasteryData Data { get; private set; }

    public List<ElementalMasteryFeature> Features { get; private set; } = new List<ElementalMasteryFeature>();


    public ElementMastery(MasteryData data) {
        this.Data = data;

        for (int i = 0; i < data.features.Count; i++) {
            ElementalMasteryFeature feature = new ElementalMasteryFeature(data.features[i]);
            Features.Add(feature);
        }
    }



    public class ElementalMasteryFeature {
        public Ability MasterFeatureAbility { get; private set; }

        public List<Ability> MasteryPathAbilities { get; private set; } = new List<Ability>();

        public string FeatureName { get { return Data.featureName; } }  

        public MasteryData.MasteryFeatureData Data { get; private set; }


        public ElementalMasteryFeature(MasteryData.MasteryFeatureData data) {
            this.Data = data;

            MasterFeatureAbility = AbilityFactory.CreateAbility(data.featureAbility.AbilityData, EntityManager.ActivePlayer);

            for (int i = 0; i < data.featurePathAbilities.Count; i++) {
                Ability pathAbility = AbilityFactory.CreateAbility(data.featurePathAbilities[i].AbilityData, EntityManager.ActivePlayer);
                MasteryPathAbilities.Add(pathAbility);
            }
        }

    }

}
