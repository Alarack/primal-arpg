using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

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
        LoadActiveFeatures(SaveLoadUtility.SaveData.savedMasteries);
    }

    public void UpdatePrimalEssenceText() {
        primalEssenceText.text = SaveLoadUtility.SaveData.primalEssencePoints.ToString();
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


    public void LoadActiveFeatures(List<SaveData.MasteryDistributionData> loadedDistribution) {

        activeFeatureDisplays.ClearList();

       

        List<MasteryFeatureEntry> loadedEntries = new List<MasteryFeatureEntry>();
        for (int i = 0; i < loadedDistribution.Count; i++) {
            loadedEntries.AddRange(GetFeatureEntiresFromLoadedMasteries(loadedDistribution[i].distirbutionData));
            
        }

        for (int i = 0; i < loadedEntries.Count; i++) {
            //loadedEntries[i].LoadFeature();
            CreateActiveFeatureEntry(loadedEntries[i]);
        }
    }

    private List<MasteryFeatureEntry> GetFeatureEntiresFromLoadedMasteries(List<SaveData.MasteryFeatureDistirbutionData> loadedDistributionData) {
        List<MasteryFeatureEntry> results = new List<MasteryFeatureEntry>();

        for (int i = 0; i < loadedDistributionData.Count; i++) {
            MasteryFeatureEntry target = GetFeatureEntryByFeatureName(loadedDistributionData[i].featureName);

            if(target != null) {
                results.Add(target);
            }
        }

        return results;
    }

    
    private MasteryFeatureEntry GetFeatureEntryByFeatureName(string featureName) {
        MasteryFeatureEntry targetEntry;

        foreach (MasteryEntry mastery in entries) {
            targetEntry = mastery.GetFeatureEntryByFeatureName(featureName);

            if (targetEntry != null) {
                return targetEntry;
            }
        }

        return null;
    }
    //private MasteryFeatureEntry GetFeatureEntryByData(MasteryData.MasteryFeatureData data) {

    //    MasteryFeatureEntry targetEntry;

    //    foreach (MasteryEntry mastery in entries) {
    //        targetEntry = mastery.GetFeatureEntryByData(data);

    //        if (targetEntry != null) {
    //            return targetEntry;
    //        }
    //    }

    //    return null;
    //}


    public void TrackFeature(MasteryFeatureEntry feature) {


       CreateActiveFeatureEntry(feature);
    }

    private void CreateActiveFeatureEntry(MasteryFeatureEntry feature) {
        activeFeatures.AddUnique(feature);

        SelectedMasteryDisplay display = Instantiate(activeFeatureTemplate, activeFeatureHolder);
        display.gameObject.SetActive(true);
        display.Setup(feature);

        activeFeatureDisplays.Add(display);

        //Debug.Log("Creating active display for: " + feature.FeatureData.featureName);
    }

    public void RemoveFeature(MasteryFeatureEntry feature) {

        for (int i = activeFeatureDisplays.Count - 1; i >= 0; i--) {
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
        if (SaveLoadUtility.SaveData.primalEssencePoints <= 0) {
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

        if (level == 0) {
            SaveLoadUtility.SaveData.RemoveMasteryPath(masteryName, featureName, abilityName);
        }
        else {
            SaveLoadUtility.SaveData.UpdateMasteryPath(masteryName, featureName, abilityName, level);
        }

        SaveLoadUtility.SavePlayerData();
        UpdatePrimalEssenceText();
    }

    public void RefundMetaPointsFromFullPath(string masteryName, string featureName, string abilityName) {

    }



    public void ShowInfoTooltip() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("You can learn up to two different Elemental Masteries.");
        builder.AppendLine();
        builder.AppendLine("The Feature skill, at the top of each section, is free to learn or unlearn at any time.");
        builder.AppendLine();
        builder.AppendLine("Spend Primal Essence Points in each Feature's Path to enhance that feature's power.");
        builder.AppendLine();
        builder.AppendLine("Right Click any Feature Path option to refund Primal Essence from that Path.");


        TooltipManager.Show(builder.ToString(), "Mastery Info");
    }

    public void HideInfoTooltip() {
        TooltipManager.Hide();
    }


    public void ShowEssenceInfo() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Gain Primal Essence after each run depending on how many monsters you defeat.");


        TooltipManager.Show(builder.ToString(), "Primal Essence");
    }


    public void DEV_GiveMetaPoints() {
        SaveLoadUtility.SaveData.primalEssencePoints++;
        SaveLoadUtility.SavePlayerData();
        UpdatePrimalEssenceText();
    }




}
