using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using TMPro;
using static UnityEditor.Progress;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    [Header("Currency")]
    public TextMeshProUGUI goldText;

    [Header("Exp")]
    public Slider expSlider;
    public TextMeshProUGUI currentXPText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI stockPileText;

    [Header("Globes")]
    public ResourceGlobeDisplay healthGlobe;
    public ResourceGlobeDisplay essenceGlobe;


    protected override void Start() {
        base.Start();
    }

    protected override void OnEnable() {
        base.OnEnable();
        EventManager.RegisterListener(GameEvent.CurrencyChanged, OnCurrencyChanged);
        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatAdjusted);
        EventManager.RegisterListener(GameEvent.EntityLeveled, OnEntityLeveled);
    }

    protected override void OnDisable() {
        base.OnDisable();

        EventManager.RemoveMyListeners(this);
    }

    public override void Open() {
        base.Open();

        healthGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Health));
        essenceGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Essence));

        UpdateEXPBar();
        UpdateStockpile();
    }


    private void OnCurrencyChanged(EventData data) {
        float value = data.GetFloat("Value");
        string currencyType = data.GetString("Currency Name");
        float balance = data.GetFloat("Current Balance");

        goldText.text = TextHelper.RoundTimeToPlaces(balance, 2);
    }

    private void OnStatAdjusted(EventData data) {
        Entity target = data.GetEntity("Target");
        StatName stat = (StatName)data.GetInt("Stat");

        if (stat != StatName.Experience)
            return;

        if (target != EntityManager.ActivePlayer)
            return;


        UpdateEXPBar();
    }

    private void OnEntityLeveled(EventData data) {
        Entity target = data.GetEntity("Target");
        int level = data.GetInt("Level");
    

        if(target != EntityManager.ActivePlayer) 
            return;

        UpdateLevel(level);
    }

    public void UpdateStockpile() {
        if (EntityManager.ActivePlayer.levelsStored > 0) {
            stockPileText.text = "Stockpile: " + EntityManager.ActivePlayer.levelsStored;
        }
        else {
            stockPileText.text = "";
        }
    }

    private void UpdateLevel(int level) {
        levelText.text = "Level " + level;
        UpdateStockpile();
    }


    private void UpdateEXPBar() {
        float ratio = EntityManager.ActivePlayer.Stats.GetStatRangeRatio(StatName.Experience);

        expSlider.value = ratio;

        string currentEXP = TextHelper.RoundTimeToPlaces(EntityManager.ActivePlayer.Stats[StatName.Experience], 0);
        string maxEXP = TextHelper.RoundTimeToPlaces(EntityManager.ActivePlayer.Stats.GetStatRangeMaxValue(StatName.Experience), 0);


        currentXPText.text = currentEXP + "/" + maxEXP;
    }

}
