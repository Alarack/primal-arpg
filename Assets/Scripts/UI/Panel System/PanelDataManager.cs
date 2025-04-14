using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelDataManager : Singleton<PanelDataManager>
{
    public PanelMapData panelMapData;
    public Transform canvasRoot;

    public static List<string> blockingPanels = new List<string>();
    public static List<string> closeOnEscapePanels = new List<string>();


    private void Start() {
        for (int i = 0; i < panelMapData.panelPrefabs.Count; i++) {
            if (panelMapData.panelPrefabs[i].autoOpen == true) {
                PanelManager.OpenPanel(panelMapData.panelPrefabs[i].panelID);
            }
        }

        blockingPanels = panelMapData.GetBlockingPanels();
        closeOnEscapePanels = panelMapData.GetEscapeClosingPanels();
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.I)) {
            PanelManager.TogglePanel<InventoryPanel>();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            PanelManager.TogglePanel<SkillsPanel>();
        }

        if(Input.GetKeyDown(KeyCode.Escape)) {
            if (PanelManager.IsEscapeClosingPanelOpen() == true)
                return;
            
            PanelManager.TogglePanel<PausePanel>();
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            PanelManager.OpenPanel<MasteryPanel>();
        }


#if UNITY_EDITOR

        //if (Input.GetKeyDown(KeyCode.M)) {
        //    PanelManager.OpenPanel<MasteryPanel>();
        //}

        //if(Input.GetKeyDown(KeyCode.L)) {
        //    PanelManager.OpenPanel<LevelUpPanel>();
        //}
#endif

        if (EntityManager.ActivePlayer != null && EntityManager.ActivePlayer.levelsStored > 0 && Input.GetKeyDown(KeyCode.L)) {
            PanelManager.OpenPanel<LevelUpPanel>();
        }

    }


    private bool ClosePanelsOnEscape() {
        bool closed = false;




        return closed;

    }

}
