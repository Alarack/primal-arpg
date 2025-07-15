using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;
using System.Text;

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

        List<ItemSlot> validSlots = ItemSpawner.Instance.lootDatabase.GetValidSlotsForStat(stat);

        StringBuilder builder = new StringBuilder();

        builder.Append(GameManager.Instance.tooltipData.GetStatTooltip(stat));

        if(validSlots.Count > 0) {
            builder.AppendLine().AppendLine();
            builder.AppendLine("Found On:");
            for (int i = 0; i < validSlots.Count; i++) {
                builder.AppendLine(validSlots[i].ToString());
            }
        }


        TooltipManager.Show(builder.ToString(), statName, -170f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

}
