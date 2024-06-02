using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPanel : BasePanel
{




    public void OnStarOverClicked() {
        EntityManager.ActivePlayer.Inventory.ClearInventory();

        PanelManager.OpenPanel<MainMenuPanel>();
        Close();
    }

    public void OnQuitClicked() {
        Application.Quit();
    }

}
