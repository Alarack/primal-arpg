using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MasteryEntry : MonoBehaviour
{
    [Header("Templates")]
    public MasteryFeatureEntry featureTemplate;
    public Transform featureHolder;
    public MasteryPathEntry pathTemplate;
    public Transform pathHolder;

    [Header("UI Bits")]
    public TextMeshProUGUI masteryNameText;
    public TextMeshProUGUI currentFeatureNameText;


    private MasteryFeatureEntry selectedFeature;

    private List<MasteryFeatureEntry> featureEntries = new List<MasteryFeatureEntry>();
    private List<MasteryPathEntry> currentPathEntries = new List<MasteryPathEntry>();


    //private Dictionary<MasteryFeatureEntry, List<MasteryPathEntry>> allPaths = new Dictionary<MasteryFeatureEntry, List<MasteryPathEntry>>();

    private MasteryData Data;

    public void Setup(MasteryData data) {
        this.Data = data;

        SetupFeatures();
    }


    private void SetupFeatures() {
        featureEntries.PopulateList(Data.features.Count, featureTemplate, featureHolder, true);
        for (int i = 0; i < featureEntries.Count; i++) {
            featureEntries[i].Setup(Data.features[i], this);
        }
    }



    public void OnFeatureSelected(MasteryFeatureEntry feature) {
        selectedFeature = feature;

        for (int i = 0; i < featureEntries.Count; i++) {
            featureEntries[i].Deselect();
        }

        selectedFeature.Select();
        currentFeatureNameText.text = selectedFeature.FeatureData.featureName;
    }

    //private void ShowFeaturePaths() {
    //    currentPathEntries.PopulateList(selectedFeature.FeatureData.featurePathAbilities.Count, pathTemplate, pathHolder, true);
    //    for (int i = 0; i < currentPathEntries.Count; i++) {
    //        currentPathEntries[i].Setup(selectedFeature.FeatureData.featurePathAbilities[i], this);
    //    }
    //}
}
