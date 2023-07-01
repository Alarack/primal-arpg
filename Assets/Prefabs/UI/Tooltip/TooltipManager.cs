using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : Singleton<TooltipManager>
{
    public Tooltip tooltip;


    public static void Show(string content, string header = "") {
        Instance.tooltip.Show();
        Instance.tooltip.SetText(content, header);

    }

    public static void Hide() {
        Instance.tooltip.Hide();
    }
}
