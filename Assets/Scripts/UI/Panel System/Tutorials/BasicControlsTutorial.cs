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

}
