using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanel : BasePanel
{




    public void OnStartGameClicked() {
        EntityManager.Instance.CreatePlayer();
        
        Close();
    }

    public void OnOptionsClicked() {
        PanelManager.OpenPanel<OptionsPanel>();
    }

    public void OnQuitClicked() {
        PanelManager.OpenPanel<PopupPanel>().Setup("Quit Game?", "Are you sure you want to quit?", OnQuitConfirmed);
    }


    private void OnQuitConfirmed() {
        Application.Quit();
    }
}
