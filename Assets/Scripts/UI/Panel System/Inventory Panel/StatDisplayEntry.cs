using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class StatDisplayEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI statText;
    private string statName;
    private StatName stat;

    public void Setup(string text, StatName stat) {
        statText.text = text;
        this.stat = stat;
    }

    public void Setup(string text, string statName, StatName stat) {
        statText.text = statName + " " + text;
        this.statName = statName;
        this.stat = stat;
    }



    public void OnPointerEnter(PointerEventData eventData) {
        //string statName = TextHelper.PretifyStatName(stat);
        TooltipManager.Show(GameManager.Instance.tooltipData.GetStatTooltip(stat), statName, -170f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

}
