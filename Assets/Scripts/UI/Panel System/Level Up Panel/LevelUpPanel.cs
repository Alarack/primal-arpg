using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatModifierData;
using TMPro;

public class LevelUpPanel : BasePanel
{

    [Header("Template")]
    public StatBoostEntry template;
    public Transform holder;

    [Header("Rerolls")]
    public TextMeshProUGUI rerollText;

    private List<StatBoostEntry> entries = new List<StatBoostEntry>();

    protected override void Awake() {
        base.Awake();

        template.gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();

        if(entries == null || entries.Count == 0)
            SetupStatChoices();

        UpdateRerollText();
    }

    private void SetupStatChoices() {
        entries.ClearList();

        List<ItemData> statBoosterItems = ItemSpawner.CreateStatBoosterSet(5);

        entries.PopulateList(statBoosterItems.Count, template, holder, true);


        for (int i = 0; i < entries.Count; i++) {
            entries[i].Setup(this, statBoosterItems[i]);
        }
    }

    public void OnRerollClicked() {
        Entity player = EntityManager.ActivePlayer;


        int availableRolls = (int)player.Stats[StatName.StatReroll];

        if(availableRolls > 0) {
            StatAdjustmentManager.AdjustStatRerolls(-1);
            SetupStatChoices();
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

        if(EntityManager.ActivePlayer.levelsStored == 0) {
            Close();
        }
        else {
           SetupStatChoices();
        }
    }


}
