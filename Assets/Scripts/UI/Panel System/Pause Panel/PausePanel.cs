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

        float savedSpeed = PlayerPrefs.GetFloat("GameSpeed");

        if(savedSpeed > 0f) {
            Time.timeScale = savedSpeed;
        }
        else {
            Time.timeScale = 1f;
        }
    }


    public void OnStarOverClicked() {
        EntityManager.GameOver();
        EntityManager.ActivePlayer.Inventory.ClearInventory();

        PanelManager.OpenPanel<MainMenuPanel>();
        Close();
    }

    public void OnOptionsClicked() {
        PanelManager.OpenPanel<OptionsPanel>();
    }

    public void OnQuitClicked() {
        PanelManager.OpenPanel<PopupPanel>().Setup("Quit Game?", "Are you sure you want to quit?", OnQuitConfirm);
    }


    private void OnQuitConfirm() {
        Application.Quit();
    }
}
