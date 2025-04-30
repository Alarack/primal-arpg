using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotionUIManager : MonoBehaviour
{

    [Header("Health Potions")]
    public PotionEntry potionTemplate;
    public Transform potionHolder;

    private List<PotionEntry> potionEntries = new List<PotionEntry>();

    private StatRange potionsStat;

    private void Awake() {
        potionTemplate.gameObject.SetActive(false);
    }

    public void Setup(StatRange potionsStat) {
        this.potionsStat = potionsStat;
        SetupDisplay();
        potionsStat.onValueChanged += OnStatValueChanged;
        potionsStat.MaxValueStat.onValueChanged += OnStatMaxValueChanged;
    }

    private void SetupDisplay() {
        potionEntries.PopulateList((int)potionsStat.MaxValueStat.ModifiedValue, potionTemplate, potionHolder, true);
        UpdatePotionUI();
    }


    private void OnDisable() {
        //EventManager.RemoveMyListeners(this);

        if (potionsStat == null)
            return;

        potionsStat.onValueChanged -= OnStatValueChanged;
        potionsStat.MaxValueStat.onValueChanged -= OnStatMaxValueChanged;

    }

    private void OnStatValueChanged(BaseStat stat, object source, float value) {
        UpdatePotionUI();
     
    }

    private void OnStatMaxValueChanged(BaseStat stat, object source, float value) {
        SetupDisplay();
    }


    private void OnPotionsChanged(EventData data) {
        StatName stat = (StatName)data.GetInt("Stat");

        if (stat != StatName.HeathPotions)
            return;
    }

    private void UpdatePotionUI() {
        float currentPotions = EntityManager.ActivePlayer.Stats[StatName.HeathPotions];
        float maxPotions = EntityManager.ActivePlayer.Stats.GetStatRangeMaxValue(StatName.HeathPotions);

        for (int i = potionEntries.Count - 1; i >= 0; i--) {
            if(currentPotions - 1 < i) {
                potionEntries[i].Empty();
            }
            else {
                potionEntries[i].Fill();
            }
        }
    }
}
