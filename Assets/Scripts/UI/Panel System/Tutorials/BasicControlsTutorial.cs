using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicControlsTutorial : BasePanel
{

    public Toggle dontShowAgain;





    public void OnOkayClicked() {
        int show = dontShowAgain.isOn ? 1 : 0;

        PlayerPrefs.SetInt("ShowBasicControls", show);
        Close();
    }

    public override void Close() {
        base.Close();
        HideInfo();
    }

    public void ShowInfo() {
        TooltipManager.Show("When you see this icon, hover over it for more info the about curren panel. It's usually in the top left corner.", "Info Box");
    }

    public void HideInfo() {
        TooltipManager.Hide();
    }

}
