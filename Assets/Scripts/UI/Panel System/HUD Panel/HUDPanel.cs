using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using TMPro;
using static UnityEditor.Progress;

public class HUDPanel : BasePanel
{
    [Header("Currency")]
    public TextMeshProUGUI goldText;

    [Header("Globes")]
    public ResourceGlobeDisplay healthGlobe;
    public ResourceGlobeDisplay essenceGlobe;


    protected override void Start() {
        base.Start();
    }

    protected override void OnEnable() {
        base.OnEnable();
        EventManager.RegisterListener(GameEvent.CurrencyChanged, OnCurrencyChanged);
    }

    protected override void OnDisable() {
        base.OnDisable();

        EventManager.RemoveMyListeners(this);
    }

    public override void Open() {
        base.Open();

        healthGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Health));
        essenceGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Essence));
    }


    private void OnCurrencyChanged(EventData data) {
        float value = data.GetFloat("Value");
        string currencyType = data.GetString("Currency Name");
        float balance = data.GetFloat("Current Balance");

        goldText.text = TextHelper.RoundTimeToPlaces(balance, 2);
    }

}
