using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Stat Tooltip Data")]
public class StatTooltipData : ScriptableObject
{


    public List<StatTooltipEntry> tooltipData = new List<StatTooltipEntry>();


    public string GetStatTooltip(StatName stat) {
        for (int i = 0; i < tooltipData.Count; i++) {
            if (stat == tooltipData[i].stat)
                return tooltipData[i].decription;
        }

        Debug.LogError("Stat not found in tooltip data: " + stat);

        return "";
    }

    public Sprite GetStatIcon(StatName stat) {
        for (int i = 0; i < tooltipData.Count; i++) {
            if (stat == tooltipData[i].stat)
                return tooltipData[i].icon;
        }

        //Debug.LogError("Stat not found in tooltip data: " + stat);

        return null;
    }




    [System.Serializable]
    public class StatTooltipEntry {
        public StatName stat;
        public string decription;
        public Sprite icon;


    }



}
