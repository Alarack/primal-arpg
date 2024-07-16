using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatModifierData;
using TMPro;
using LL.Events;

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


    private Task loadingChoicesTask;

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
            StartLoadingChoicesTask();
        
        UpdateRerollText();
    }
    public override void Close() {
        base.Close();

        int showControls = PlayerPrefs.GetInt("ShowBasicControls");

        if(showControls == 0) {
            PanelManager.OpenPanel<BasicControlsTutorial>();
        }
    }

    private void StartLoadingChoicesTask() {
        loadingChoicesTask = new Task(SetupAbilityChoices());
    }

    private IEnumerator SetupAbilityChoices() {
        WaitForSeconds waiter = new WaitForSeconds(0.3f);
        
        List<Ability> lockedAbilities = EntityManager.ActivePlayer.AbilityManager.GetLockedAbilities(AbilityCategory.KnownSkill);
        lockedAbilities.AddRange(EntityManager.ActivePlayer.AbilityManager.GetLockedAbilities(AbilityCategory.PassiveSkill));
        
        List<Ability> choices = new List<Ability>();

        List<string> currentRewardSkills = RoomManager.GetSKillRewardNames();


        int safetyCounter = 0;
        for (int i = 4; i >=0 ; i--) {
            lockedAbilities.Shuffle();
            
            if(lockedAbilities.Count > i) {
                if (currentRewardSkills.Contains(lockedAbilities[i].Data.abilityName)) {
                    //Debug.LogWarning("A skill: " + lockedAbilities[i].Data.abilityName + " would be a dupilicate");
                    safetyCounter++;
                    if(safetyCounter < 50)
                        i++;
                    continue;
                } 
                    
                
                choices.Add(lockedAbilities[i]);
                lockedAbilities.RemoveAt(i);
            }
            
        }

        if(choices.Count == 0) {
            Debug.LogWarning("No Locked abilities left");
            Close();
            yield break;
        }

        abilityChoiceEntries.PopulateList(choices.Count, abilityTemplate, abilityHolder, false);
        for (int i = 0; i < choices.Count; i++) {
            abilityChoiceEntries[i].Setup(choices[i], this);
            abilityChoiceEntries[i].gameObject.SetActive(true);
            yield return waiter;
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
            StartLoadingChoicesTask();
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

        if (loadingChoicesTask != null && loadingChoicesTask.Running == true)
            return;


        EntityManager.ActivePlayer.levelsStored--;
        PanelManager.GetPanel<HUDPanel>().UpdateStockpile();

        entry.AbilityChoice.Locked = false;
        EntityManager.ActivePlayer.AbilityManager.UnlockAbility(entry.AbilityChoice.Data.abilityName);
        

        EventData data = new EventData();
        data.AddAbility("Ability", entry.AbilityChoice);
        EventManager.SendEvent(GameEvent.LevelUpAbilitySelected, data);

        if (EntityManager.ActivePlayer.levelsStored == 0) {
            abilityChoiceEntries.ClearList();
            Close();
            TooltipManager.Hide();
        }
        else {
            StartLoadingChoicesTask();
        }
    }


}
