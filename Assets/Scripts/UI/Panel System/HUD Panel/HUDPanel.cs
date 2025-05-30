using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class HUDPanel : BasePanel {
    [Header("Currency")]
    public TextMeshProUGUI goldText;

    [Header("Exp")]
    public Slider expSlider;
    public TextMeshProUGUI currentXPText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI stockPileText;
    public GameObject levelUpButton;

    [Header("Globes")]
    public ResourceGlobeDisplay healthGlobe;
    public ResourceGlobeDisplay essenceGlobe;

    [Header("Potion UI")]
    public HealthPotionUIManager healthPotionUIManager;

    [Header("Dashes UI")]
    public Image dashImage;
    public Transform dashShimmer;


    [Header("Status Bar")]
    public Transform buffHolder;
    public Transform debuffHolder;
    public StatusIndicatorEntry statusTemplate;

    private List<StatusIndicatorEntry> statusIndicators = new List<StatusIndicatorEntry>();
    private PlayerMovement playerMovement;

    protected override void Awake() {
        base.Awake();

        statusTemplate.gameObject.SetActive(false);
        goldText.text = "0";
    }

    protected override void Start() {
        base.Start();
    }

    protected override void OnEnable() {
        base.OnEnable();
        EventManager.RegisterListener(GameEvent.CurrencyChanged, OnCurrencyChanged);
        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatAdjusted);

        EventManager.RegisterListener(GameEvent.EntityLeveled, OnEntityLeveled);
        EventManager.RegisterListener(GameEvent.StatusApplied, OnStatusApplied);
        EventManager.RegisterListener(GameEvent.StatusRemoved, OnStatusRemoved);
        EventManager.RegisterListener(GameEvent.StatusStacked, OnStatusStacked);

        EventManager.RegisterListener(GameEvent.DashCooldownFinished, OnDashCooldownFinished);
    }

    protected override void OnDisable() {
        base.OnDisable();

        EventManager.RemoveMyListeners(this);
    }

    protected override void Update() {
        base.Update();

        UpdateDashUI();


    }

    public override void Open() {
        base.Open();

        healthGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Health));
        essenceGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Essence));
        playerMovement = EntityManager.ActivePlayer.Movement as PlayerMovement;

        UpdateEXPBar();
        UpdateStockpile();
        healthPotionUIManager.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.HeathPotions));

    }


    public void ClearStatusUI() {
        statusIndicators.ClearList();
    }

    private void UpdateDashUI() {
        if (playerMovement == null)
            return;
        
        dashImage.fillAmount = Mathf.Abs(playerMovement.DashCooldownRatio - 1);

        if (dashImage.fillAmount != 0f && playerMovement.CanDash == true) {
            dashImage.fillAmount = 0f;
        }

    }

    private void OnDashCooldownFinished(EventData data) {
        dashShimmer.localPosition = new Vector2(-50f, 0f);
        dashShimmer.DOLocalMove(new Vector2(50f, 0f), 0.5f);

    }

    private void OnStatusApplied(EventData data) {
        Status status = data.GetStatus("Status");
        Entity target = data.GetEntity("Target");

        if (target != EntityManager.ActivePlayer)
            return;

        StatusIndicatorEntry newStatus = Instantiate(statusTemplate, buffHolder);
        newStatus.gameObject.SetActive(true);
        newStatus.Setup(status);
        statusIndicators.Add(newStatus);

    }

    private void OnStatusStacked(EventData data) {
        Status status = data.GetStatus("Status");
        Entity target = data.GetEntity("Target");

        if (target != EntityManager.ActivePlayer)
            return;

        if (status.Data.stackMethod == Status.StackMethod.None)
            return;

        StatusIndicatorEntry targetStatus = GetStatusIndicator(status);

        if (targetStatus != null) {
            targetStatus.UpdateStackCount();
        }
    }

    private void OnStatusRemoved(EventData data) {
        Status status = data.GetStatus("Status");
        Entity target = data.GetEntity("Target");

        if (target != EntityManager.ActivePlayer)
            return;

        StatusIndicatorEntry targetStatus = GetStatusIndicator(status);

        if (targetStatus != null) {
            statusIndicators.Remove(targetStatus);
            Destroy(targetStatus.gameObject);
        }
        else {
            Debug.LogError("Couldn't find a status: " + status.Data.statusName + " in the indicators");
        }


        //for (int i = statusIndicators.Count -1; i >=0; i--) {
        //    if (statusIndicators[i].activeStatus == status) {
        //        Destroy(statusIndicators[i].gameObject);
        //        statusIndicators.RemoveAt(i);
        //    }
        //    else {
        //        Debug.Log(statusIndicators[i].activeStatus.Data.statusName + " is not " + status.Data.statusName);
        //    }
        //}
    }

    private StatusIndicatorEntry GetStatusIndicator(Status status) {
        for (int i = 0; i < statusIndicators.Count; i++) {
            if (statusIndicators[i].activeStatus == status) {
                return statusIndicators[i];
            }
        }

        return null;
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


        if (target != EntityManager.ActivePlayer)
            return;

        UpdateLevel(level);
    }



    public void UpdateStockpile() {
        if (EntityManager.ActivePlayer.levelsStored > 0) {
            stockPileText.text = "Stockpile: " + EntityManager.ActivePlayer.levelsStored;
            levelUpButton.SetActive(true);
        }
        else {
            stockPileText.text = "";
            levelUpButton.SetActive(false);
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



    public void OnLevelUpClicked() {
        if (EntityManager.ActivePlayer.levelsStored > 0) {
            PanelManager.OpenPanel<LevelUpPanel>();
        }
    }

    public void OnSkillsClicked() {
        PanelManager.TogglePanel<SkillsPanel>();
    }

    public void OnMasteryClicked() {
        PanelManager.OpenPanel<MasteryPanel>();
    }

    public void OnInventoryClicked() {
        PanelManager.TogglePanel<InventoryPanel>();
    }

}
