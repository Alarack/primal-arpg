using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StatBoostEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Image statIcon;
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statBoostText;

    private LevelUpPanel levelUpPanel;

    public ItemData StatItem { get; private set; }



    public void Setup(LevelUpPanel levelUpPanel, ItemData statItem) {
        this.levelUpPanel = levelUpPanel;
        this.StatItem = statItem;

        SetupDisplay();
    }

    private void SetupDisplay() {
        statNameText.text = TextHelper.PretifyStatName(StatItem.statModifierData[0].targetStat);
        statBoostText.text = TextHelper.FormatStat(StatItem.statModifierData[0].targetStat, StatItem.statModifierData[0].value);
    }


    #region UI EVENTS
    public void OnPointerClick(PointerEventData eventData) {
        levelUpPanel.OnStatSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        
    }

    public void OnPointerExit(PointerEventData eventData) {
        
    }

    #endregion


}
