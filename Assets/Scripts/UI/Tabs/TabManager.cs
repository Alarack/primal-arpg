using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabManager : MonoBehaviour
{

    public TabEntry[] tabs;


    public TabEntry selectedTab;

    private void Awake() {
        tabs = GetComponentsInChildren<TabEntry>(true);

        for (int i = 0; i < tabs.Length; i++) {
            tabs[i].Setup(this);
        }
    }




    public void OnTabSelected(TabEntry tab) {
        selectedTab = tab;
        for (int i = 0; i < tabs.Length; i++) {
            if(tab != tabs[i]) {
                tabs[i].Deselect();
            }
        }

        tab.Select();
    }

}
