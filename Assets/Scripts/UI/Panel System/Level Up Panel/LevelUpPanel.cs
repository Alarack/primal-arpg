using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatModifierData;
using TMPro;

public class LevelUpPanel : BasePanel
{

    [Header("Stat Boost Template")]
    public StatBoostEntry statBoostTemplate;
    public Transform statBoostHolder;

    [Header("Ability Template")]
    public AbilityChoiceEntry abilityTemplate;
    public Transform abilityHolder;

    [Header("Rerolls")]
    public TextMeshProUGUI rerollText;

    private List<StatBoostEntry> statBoostEntries = new List<StatBoostEntry>();
    private List<AbilityChoiceEntry> abilityChoiceEntries = new List<AbilityChoiceEntry>();

    protected override void Awake() {
        base.Awake();

        statBoostTemplate.gameObject.SetActive(false);
        abilityTemplate.gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();

        if(statBoostEntries == null || statBoostEntries.Count == 0)
            SetupStatChoices();

        if(abilityChoiceEntries == null || abilityChoiceEntries.Count == 0)
            SetupAbilityChoices();
        
        UpdateRerollText();
    }

    private void SetupAbilityChoices() {
        List<Ability> lockedAbilities = EntityManager.ActivePlayer.AbilityManager.GetLockedAbilities(AbilityCategory.KnownSkill);
        List<Ability> choices = new List<Ability>();

        for (int i = 0; i < 5; i++) {
            lockedAbilities.Shuffle();
            
            if(lockedAbilities.Count > 0) {
                choices.Add(lockedAbilities[0]);
                lockedAbilities.RemoveAt(0);
            }

        }

        if(choices.Count == 0) {
            Debug.LogWarning("No Locked abilities left");
            return;
        }

        abilityChoiceEntries.PopulateList(choices.Count, abilityTemplate, abilityHolder, true);
        for (int i = 0; i < choices.Count; i++) {
            abilityChoiceEntries[i].Setup(choices[i], this);
        }


    }

    private void SetupStatChoices() {
        statBoostEntries.ClearList();

        List<ItemData> statBoosterItems = ItemSpawner.CreateStatBoosterSet(5);

        statBoostEntries.PopulateList(statBoosterItems.Count, statBoostTemplate, statBoostHolder, true);


        for (int i = 0; i < statBoostEntries.Count; i++) {
            statBoostEntries[i].Setup(this, statBoosterItems[i]);
        }
    }

    public void OnRerollClicked() {
        Entity player = EntityManager.ActivePlayer;


        int availableRolls = (int)player.Stats[StatName.StatReroll];

        if(availableRolls > 0) {
            StatAdjustmentManager.AdjustStatRerolls(-1);
            SetupStatChoices();
            SetupAbilityChoices();
            UpdateRerollText();
        }
    }

    private void UpdateRerollText() {
        rerollText.text = "Rerolls: " + EntityManager.ActivePlayer.Stats[StatName.StatReroll];
    }


    public void OnStatSelected(StatBoostEntry entry) {
        Debug.Log(entry.StatItem.statModifierData[0].targetStat + " selected for " + entry.StatItem.statModifierData[0].value);

        StatModifier mod = new StatModifier(entry.StatItem.statModifierData[0], EntityManager.ActivePlayer);

        StatAdjustmentManager.ApplyStatAdjustment(EntityManager.ActivePlayer, mod, mod.TargetStat, mod.VariantTarget, null);

        EntityManager.ActivePlayer.levelsStored--;
        PanelManager.GetPanel<HUDPanel>().UpdateStockpile();

        if (EntityManager.ActivePlayer.levelsStored == 0) {
            Close();
        }
        else {
            SetupStatChoices();
        }
    }

    public void OnAbilitySelected(AbilityChoiceEntry entry) {

        EntityManager.ActivePlayer.levelsStored--;
        PanelManager.GetPanel<HUDPanel>().UpdateStockpile();

        entry.AbilityChoice.Locked = false;
        

        //EntityManager.ActivePlayer.AbilityManager.AutoEquipAbilityToHotbar(entry.AbilityChoice, 4);

        if (EntityManager.ActivePlayer.levelsStored == 0) {
            abilityChoiceEntries.ClearList();
            Close();
            TooltipManager.Hide();
        }
        else {
            SetupAbilityChoices();
        }
    }


}
