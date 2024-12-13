using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{


    public TextMeshProUGUI progresText;
    public Slider progressSlider;


    private int savedThreat;
    private int currentThreat;

    public override void Open() {
        base.Open();
        savedThreat = PlayerPrefs.GetInt("Total Threat");
        currentThreat = GameManager.Instance.totalThreatFromKilledEnemies;

        progresText.text = "0/200";

        ShowMetaProgress();
    }


    private void ShowMetaProgress() {
        new Task(CountUpProgress());


    }

    private IEnumerator CountUpProgress() {
        WaitForSeconds waiter = new WaitForSeconds (0.01f);

        int currentValue = 0;

        while(currentValue < currentThreat) {
            yield return waiter;
            currentValue++;

            progresText.text = currentValue.ToString() + "/" + 200;
            progressSlider.value = currentValue / 200f;

            if(currentValue >= 200) {
                currentThreat = Mathf.Clamp(currentThreat - 200, 0, currentThreat);
                currentValue = 0;
                MetaPointFanfare();
            }

        }



    }


    private void MetaPointFanfare() {
        Debug.Log("Point Unlocked!");
    }


    public void OnStarOverClicked() {
        EntityManager.ActivePlayer.Inventory.ClearInventory();

      
        PlayerPrefs.SetInt("Total Threat", GameManager.Instance.totalThreatFromKilledEnemies + savedThreat);
        GameManager.Instance.totalThreatFromKilledEnemies = 0;

        PanelManager.OpenPanel<MainMenuPanel>();
        Close();
    }

    public void OnQuitClicked() {
        Application.Quit();
    }

}
