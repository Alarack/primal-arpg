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

    }

    public void OnQuitClicked() {
        Application.Quit();
    }

}
