using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsPanel : BasePanel
{

    public TabManager tabmanager;


    public override void Open() {
        base.Open();

        tabmanager.OnTabSelected(tabmanager.selectedTab);
    }

}
