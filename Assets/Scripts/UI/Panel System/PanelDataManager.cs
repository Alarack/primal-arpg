using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelDataManager : Singleton<PanelDataManager>
{
    public PanelMapData panelMapData;
    public Transform canvasRoot;

    public static List<string> blockingPanels = new List<string>();


    private void Start() {
        for (int i = 0; i < panelMapData.panelPrefabs.Count; i++) {
            if (panelMapData.panelPrefabs[i].autoOpen == true) {
                PanelManager.OpenPanel(panelMapData.panelPrefabs[i].panelID);
            }
        }

        blockingPanels = panelMapData.GetBlockingPanels();
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.C)) {
            PanelManager.TogglePanel<InventoryPanel>();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            PanelManager.TogglePanel<SkillsPanel>();
        }

        if(Input.GetKeyDown(KeyCode.Escape)) {
            PanelManager.TogglePanel<PausePanel>();
        }


#if UNITY_EDITOR



        //if(Input.GetKeyDown(KeyCode.L)) {
        //    PanelManager.OpenPanel<LevelUpPanel>();
        //}
#endif

        if (EntityManager.ActivePlayer != null && EntityManager.ActivePlayer.levelsStored > 0 && Input.GetKeyDown(KeyCode.L)) {
            PanelManager.OpenPanel<LevelUpPanel>();
        }

    }

}
