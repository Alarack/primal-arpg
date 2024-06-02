using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : BasePanel
{


    public override void Open() {
        base.Open();
        Time.timeScale = 0f;
    }

    public override void Close() {
        base.Close();
        Time.timeScale = 1f;
    }


    public void OnStarOverClicked() {
        EntityManager.GameOver();
        EntityManager.ActivePlayer.Inventory.ClearInventory();

        PanelManager.OpenPanel<MainMenuPanel>();
        Close();
    }

    public void OnQuitClicked() {
        Application.Quit();
    }

}
