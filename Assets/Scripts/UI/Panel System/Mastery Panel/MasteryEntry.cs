using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Burst.Intrinsics;
using System;

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
            if (featureEntries[i].FeatureData == null)
                continue;
            
            if (featureEntries[i].FeatureData.featureName == featureName) {
                return featureEntries[i];
            }
        }

        return null;
    }


    private void SetupFeatures() {
        featureEntries.PopulateList(Data.features.Count, featureTemplate, featureHolder, true);
        for (int i = 0; i < featureEntries.Count; i++) {
            if (Data.features[i].dev == true) {
                featureEntries[i].gameObject.SetActive(false);
                continue;
            }
            
            featureEntries[i].Setup(Data.features[i], this);
        }

        if(featureEntries.Count > 0) {
            MasteryFeatureEntry firstActive = GetFirstActiveEntry();
            if(firstActive != null) {
                OnFeatureSelected(firstActive);
            }
            else {
                Debug.LogError("No Active Feature Entries found for: " + Data.masteryName);
            }
        }
    }


    private MasteryFeatureEntry GetFirstActiveEntry() {
        for (int i = 0; i < featureEntries.Count; i++) {
            if (featureEntries[i].gameObject.activeSelf == true)
                return featureEntries[i];
        }

        return null;
    }

    public void OnFeatureSelected(MasteryFeatureEntry feature) {
        selectedFeature = feature;

        for (int i = 0; i < featureEntries.Count; i++) {
            if (featureEntries[i] == selectedFeature)
                continue;
            
            featureEntries[i].Deselect();
        }

        selectedFeature.Select();
        currentFeatureNameText.text = selectedFeature.FeatureData.featureName;

        //if(confirm == true) {

        //    if (PanelManager.GetPanel<MasteryPanel>().CheckMaxFeatures() == true)
        //        return;

        //    if (SaveLoadUtility.SaveData.CheckForDuplicateMastery(MasteryName) == true)
        //        return;
            
        //    PromptLearnMastery(feature);

        //}
    }

    public void OnLearnClicked() {
        if (selectedFeature == null)
            return;


        PromptLearnMastery(selectedFeature);

        //if (PanelManager.GetPanel<MasteryPanel>().CheckMaxFeatures() == true)
        //    return;

        //if (SaveLoadUtility.SaveData.CheckForDuplicateMastery(MasteryName) == true)
        //    return;


        //selectedFeature.OnLearnClicked();
    }

    private void PromptLearnMastery(MasteryFeatureEntry feature) {

        bool featureExists = feature.FeatureAbility != null;
        bool featureEquipped = featureExists && feature.FeatureAbility.IsEquipped;

        if(featureExists && featureEquipped == true) {
            PanelManager.OpenPanel<PopupPanel>().Setup("Unlearning: " 
                + feature.FeatureAbility.Data.abilityName, "Are you sure you want to unlearn this Mastery?", ConfirmUnlearnMastery);
            return;
        }

        if (featureExists == false ||  (featureExists && featureEquipped == false)) {
            PanelManager.OpenPanel<PopupPanel>().Setup("Learning: "
            + feature.FeatureData.featureName, "Are you sure you want to learn this mastery?"
            + ". You can change your mind later.", ConfirmLearnMastery);
            return;
        }
    }


    private void ConfirmLearnMastery() {
        if (PanelManager.GetPanel<MasteryPanel>().CheckMaxFeatures() == true)
            return;

        if (SaveLoadUtility.SaveData.CheckForDuplicateMastery(MasteryName) == true)
            return;


        selectedFeature.Learn();
    }

    private void ConfirmUnlearnMastery() {
        selectedFeature.Unlearn();
    }

    private void ConfirmMastery() {


        //if (PanelManager.GetPanel<MasteryPanel>().CheckMaxFeatures() == true)
        //    return;

        //if (SaveLoadUtility.SaveData.CheckForDuplicateMastery(MasteryName) == true)
        //    return;


        //selectedFeature.OnLearnClicked();


        //OnLearnClicked();
    }

}
