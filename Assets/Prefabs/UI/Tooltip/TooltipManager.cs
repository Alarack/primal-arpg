using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : Singleton<TooltipManager>
{
    public Tooltip tooltip;


    public static void Show(string content, string header = "", float xOff = 0f, float yOff = 0f) {
        Instance.tooltip.Show(xOff, yOff);
        Instance.tooltip.SetText(content, header);

    }

    public static void Hide() {
        Instance.tooltip.Hide();
    }
}
