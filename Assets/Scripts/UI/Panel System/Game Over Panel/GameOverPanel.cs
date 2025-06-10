using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{

    [Header("Recovred Items")]
    public RecoveredItemEntry template;
    public Transform holder;
    public CanvasGroup recoveredItemsFader;
    public GameObject recoveredItemsArea;

    private bool selectingRecoveredItems;


    [Header("Meta Progress")]
    public ParticleSystem starsParticles;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI pointsGainText;
    public TextMeshProUGUI sessionScoreText;
    public Slider progressSlider;


    private int savedThreat;
    private int currentThreat;
    private int sessionScore;
    //private int pointsGained;

    private MasteryPanel masteryPanel;

    private List<RecoveredItemEntry> recoveredItemEntries = new List<RecoveredItemEntry>();
    private List<ItemDefinition> recoveredItems = new List<ItemDefinition>();


    protected override void Awake() {
        base.Awake();

        masteryPanel = PanelManager.GetPanel<MasteryPanel>();
        template.gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();
        recoveredItems = EntityManager.ActivePlayer.Inventory.GetRecoverableItems();
        if (recoveredItems.Count > 0) {
            recoveredItemsArea.SetActive(true);
            recoveredItemsFader.alpha = 0f;
            ShowRecoveredItems();
            selectingRecoveredItems = true;
        }
        else {
            recoveredItemsArea.SetActive(false);
        }

        savedThreat = PlayerPrefs.GetInt("Total Threat");
        currentThreat = GameManager.Instance.totalThreatFromKilledEnemies;
        sessionScore = currentThreat;
        //pointsGained = 0;
        progressText.text = "0/" + ItemSpawner.Instance.lootDatabase.metaUnlockValue;
        pointsGainText.text = "";
        sessionScoreText.text = "Total Score: " + sessionScore;

        ShowMetaProgress();

        List<string> items = EntityManager.ActivePlayer.Inventory.GetEquipmentNames();
        List<ItemDefinition> foundItems = ItemSpawner.Instance.lootDatabase.GetItemsByNames(items);
        foreach (ItemDefinition item in foundItems) {
            if (item.startingItem == true)
                continue;

            if(SaveLoadUtility.SaveData.recoveredItems.Contains(item.itemData.itemName))
                 continue; 

            Debug.Log(item.itemData.itemName + " is a valid chose to save");
        }

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

            //Debug.Log("Current Value: " + currentValue);
            //Debug.Log("Slider Value: " + progressValue);

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

        SaveLoadUtility.SaveData.primalEssencePoints++;

        pointsGainText.text = "Primal Essence: " + SaveLoadUtility.SaveData.primalEssencePoints;
        SaveLoadUtility.SavePlayerData();

    }


    private void MetaPointFanfare() {
        Debug.Log("Point Unlocked!");
       
        starsParticles.Play();

      
    }


    public void ShowRecoveredItems() {
        recoveredItemEntries.PopulateList(recoveredItems.Count, template, holder, true);
        for (int i = 0; i < recoveredItems.Count; i++) {
            recoveredItemEntries[i].Setup(recoveredItems[i], this);
        }
        recoveredItemsFader.DOFade(1f, 0.2f);
    }

    public void OnRecoveredItemSelected(RecoveredItemEntry entry) {
        if (selectingRecoveredItems == false)
            return;

        selectingRecoveredItems = false;

        entry.Select();
        HideRecoveredItems();

    }

    public void HideRecoveredItems() {
        Tween fadeout = recoveredItemsFader.DOFade(0f, 0.2f);
        fadeout.OnComplete(() => recoveredItemsArea.SetActive(false));
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

    public void OnMasteryClicked() {
        masteryPanel.SetOnCloseCallback(OnMasteryFinished);
        PanelManager.OpenPanel<MasteryPanel>();
    }

    private void OnMasteryFinished() {
        masteryPanel.SetOnCloseCallback(null);
        OnStarOverClicked();
        Close();
        
    }

}
