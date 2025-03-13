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
    //private List<MasteryPathEntry> currentPathEntries = new List<MasteryPathEntry>();


    //private Dictionary<MasteryFeatureEntry, List<MasteryPathEntry>> allPaths = new Dictionary<MasteryFeatureEntry, List<MasteryPathEntry>>();

    public string MasteryName { get { return Data.masteryName; } }
    private MasteryData Data;


    private void Awake() {
        featureTemplate.gameObject.SetActive(false);
        pathTemplate.gameObject.SetActive(false);
    }

    public void Setup(MasteryData data) {
        this.Data = data;
        masteryNameText.text = data.masteryName;
        SetupFeatures();
    }

    //public MasteryFeatureEntry GetFeatureEntryByData(MasteryData.MasteryFeatureData data) {
    //    for (int i = 0; i < featureEntries.Count; i++) {
    //        if (featureEntries[i].FeatureData.featureName == data.featureName) {
    //            return featureEntries[i];
    //        }
    //    }

    //    return null;
    //}

    public MasteryFeatureEntry GetFeatureEntryByFeatureName(string featureName) {
        for (int i = 0; i < featureEntries.Count; i++) {
            if (featureEntries[i].FeatureData.featureName == featureName) {
                return featureEntries[i];
            }
        }

        return null;
    }


    private void SetupFeatures() {
        featureEntries.PopulateList(Data.features.Count, featureTemplate, featureHolder, true);
        for (int i = 0; i < featureEntries.Count; i++) {
            featureEntries[i].Setup(Data.features[i], this);
        }

        if(featureEntries.Count > 0)
            OnFeatureSelected(featureEntries[0], false);
    }


    public void OnFeatureSelected(MasteryFeatureEntry feature, bool confirm = true) {
        selectedFeature = feature;

        for (int i = 0; i < featureEntries.Count; i++) {
            if (featureEntries[i] == selectedFeature)
                continue;
            
            featureEntries[i].Deselect();
        }

        selectedFeature.Select();
        currentFeatureNameText.text = selectedFeature.FeatureData.featureName;

        if(confirm == true) {

            if (PanelManager.GetPanel<MasteryPanel>().CheckMaxFeatures() == true)
                return;

            if (SaveLoadUtility.SaveData.CheckForDuplicateMastery(MasteryName) == true)
                return;
            
            PromptLearnMastery(feature);

        }
    }

    public void OnLearnClicked() {
        if (selectedFeature == null)
            return;
        
        selectedFeature.OnLearnClicked();
    }

    private void PromptLearnMastery(MasteryFeatureEntry feature) {
        PanelManager.OpenPanel<PopupPanel>().Setup("Learning " + feature.FeatureData.featureName, "Are you sure you want to choose this mastery? You can change your mind later.", ConfirmMastery);
    }


    private void ConfirmMastery() {
        OnLearnClicked();
    }

}
