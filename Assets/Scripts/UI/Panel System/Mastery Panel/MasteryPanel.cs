using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MasteryPanel : BasePanel {

    [Header("Template")]
    public MasteryEntry template;
    public Transform holder;

    [Space(10)]
    public SelectedMasteryDisplay activeFeatureTemplate;
    public Transform activeFeatureHolder;

    [Header("Text Fields")]
    public TextMeshProUGUI primalEssenceText;


    private List<MasteryEntry> entries = new List<MasteryEntry>();


    private List<MasteryFeatureEntry> activeFeatures = new List<MasteryFeatureEntry>();
    private List<SelectedMasteryDisplay> activeFeatureDisplays = new List<SelectedMasteryDisplay>();

    protected override void Awake() {
        base.Awake();
        template.gameObject.SetActive(false);
        activeFeatureTemplate.gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();

        SetupDisplay();
    }

    private void SetupDisplay() {
        UpdatePrimalEssenceText();


        PopulateMasteries();
    }

    public void UpdatePrimalEssenceText() {
        primalEssenceText.text = "Primal Essence: " + SaveLoadUtility.SaveData.primalEssencePoints;
    }

    private void PopulateMasteries() {
        entries.PopulateList(GameManager.Instance.masteryDatabase.masteryData.Count, template, holder, true);
        for (int i = 0; i < entries.Count; i++) {
            entries[i].Setup(GameManager.Instance.masteryDatabase.masteryData[i]);
        }
    }



    public bool CheckMaxFeatures() {

        if (SaveLoadUtility.SaveData.CountOfMasteries >= 2) {
            PanelManager.OpenPanel<PopupPanel>().Setup("Maximum Masteries", "You can only have 2 Masteries at a time. Right Click one of your existing Masteries to unlearn it.");
            return true;
        }

        return false;
    }

    public void TrackFeature(MasteryFeatureEntry feature) {
        if(SaveLoadUtility.SaveData.CountOfMasteries >= 2) {
            PanelManager.OpenPanel<PopupPanel>().Setup("Maximum Masteries", "You can only have 2 Masteries at a time. Right Click one of your existing Masteries to unlearn it.");
            return;
        }

        activeFeatures.AddUnique(feature);

        SelectedMasteryDisplay display = Instantiate(activeFeatureTemplate, activeFeatureHolder);
        display.gameObject.SetActive(true);
        display.Setup(feature);

        activeFeatureDisplays.Add(display);
    }

    public void RemoveFeature(MasteryFeatureEntry feature) {

        for (int i = activeFeatureDisplays.Count -1; i >=0 ; i--) {
            if (activeFeatureDisplays[i].Feature.FeatureAbility.Data == feature.FeatureData.featureAbility.AbilityData) {
                
                Destroy(activeFeatureDisplays[i].gameObject);
                activeFeatureDisplays.RemoveAt(i);
                //Debug.Log("removed: " + feature.FeatureData.featureName);
            }
        }
        
        activeFeatures.RemoveIfContains(feature);
    }


    public bool HasMetaPoints() {
        return SaveLoadUtility.SaveData.primalEssencePoints > 0;
    }

    public bool TrySpendMetaPoints(string masteryName, string featureName, string abilityName, int level) {
        if(SaveLoadUtility.SaveData.primalEssencePoints <= 0) {
            return false;
        }

        SaveLoadUtility.SaveData.primalEssencePoints--;
        SaveLoadUtility.SaveData.UpdateMasteryPath(masteryName, featureName, abilityName, level);
        SaveLoadUtility.SavePlayerData();
        UpdatePrimalEssenceText();
        return true;
    }

    public void RefundMetaPoints(string masteryName, string featureName, string abilityName, int level) {
        SaveLoadUtility.SaveData.primalEssencePoints++;
        
        if(level == 0) {
            SaveLoadUtility.SaveData.RemoveMasteryPath(masteryName, featureName, abilityName);
        }
        else {
            SaveLoadUtility.SaveData.UpdateMasteryPath(masteryName, featureName, abilityName, level);
        }
        
        SaveLoadUtility.SavePlayerData();
        UpdatePrimalEssenceText();
    }
}
