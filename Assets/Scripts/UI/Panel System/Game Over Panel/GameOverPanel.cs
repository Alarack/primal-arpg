using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{

    public ParticleSystem starsParticles;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI pointsGainText;
    public TextMeshProUGUI sessionScoreText;
    public Slider progressSlider;


    private int savedThreat;
    private int currentThreat;
    private int sessionScore;
    //private int pointsGained;

    public override void Open() {
        base.Open();
        savedThreat = PlayerPrefs.GetInt("Total Threat");
        currentThreat = GameManager.Instance.totalThreatFromKilledEnemies;
        sessionScore = currentThreat;
        //pointsGained = 0;
        progressText.text = "0/" + ItemSpawner.Instance.lootDatabase.metaUnlockValue;
        pointsGainText.text = "";
        sessionScoreText.text = "Total Score: " + sessionScore;

        ShowMetaProgress();
    }


    private void ShowMetaProgress() {
        new Task(CountUpProgress());


    }

    private IEnumerator CountUpProgress() {
        WaitForSeconds waiter = new WaitForSeconds (0.05f);

        int currentValue = 0;

        while(currentValue < currentThreat) {
            yield return waiter;
            currentValue++;
            sessionScore--;
            sessionScoreText.text = "Total Score: " + sessionScore;

            progressText.text = currentValue.ToString() + "/" + ItemSpawner.Instance.lootDatabase.metaUnlockValue;
            
            float progressValue = (float)currentValue / (float)ItemSpawner.Instance.lootDatabase.metaUnlockValue;
            progressSlider.value = progressValue;

            Debug.Log("Current Value: " + currentValue);
            Debug.Log("Slider Value: " + progressValue);

            if (currentValue >= ItemSpawner.Instance.lootDatabase.metaUnlockValue) {

                yield return new WaitForSeconds(0.2f);

                currentThreat = Mathf.Clamp(currentThreat - ItemSpawner.Instance.lootDatabase.metaUnlockValue, 0, currentThreat);
                currentValue = 0;


                GainMetaPoint();
                MetaPointFanfare();

            }

        }



    }


    private void GainMetaPoint() {
        int metaPoints = PlayerPrefs.GetInt("Meta Points");
        metaPoints++;

        pointsGainText.text = "Primal Essence: " + metaPoints;
        PlayerPrefs.SetInt("Meta Points", metaPoints);

    }


    private void MetaPointFanfare() {
        Debug.Log("Point Unlocked!");
       
        starsParticles.Play();

      
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
