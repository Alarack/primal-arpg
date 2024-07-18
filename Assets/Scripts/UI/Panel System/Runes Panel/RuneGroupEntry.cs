using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class RuneGroupEntry : MonoBehaviour {

    [Header("Template")]
    public RuneChoiceEntry runeChoiceTemplate;
    public Transform runeChoiceHolder;

    [Header("Other UI Bits")]
    public TextMeshProUGUI tierText;
    public GameObject dimmer;

    [Header("VFX")]
    public GameObject shimmer;
    public CanvasGroup blockerFade;

    private RuneChoiceEntry currentChoice;

    private List<RuneChoiceEntry> entries = new List<RuneChoiceEntry>();

    private RunesPanel runesPanel;
    private bool revealed;

    public AbilityRuneGroupData RuneGroupData { get; private set; }


    private void Awake() {
        runeChoiceTemplate.gameObject.SetActive(false);
    }

    public void Setup(RunesPanel runesPanel, AbilityRuneGroupData runeGroupData) {
        this.runesPanel = runesPanel;
        this.RuneGroupData = runeGroupData;

        SetupDisplay();
    }


    private void SetupDisplay() {
        entries.PopulateList(RuneGroupData.runes.Count, runeChoiceTemplate, runeChoiceHolder, true);

        if (runesPanel.CurrentAbility.runeItemsByTier.TryGetValue(RuneGroupData.tier, out List<Item> runeItems) == true) {
            for (int i = 0; i < entries.Count; i++) {
                entries[i].Setup(runeItems[i], this, runesPanel);

                if (runeItems[i].Equipped == true) {
                    entries[i].Select();
                }
                else {
                    entries[i].Deselect();
                }
            }
        }

        tierText.text = RuneGroupData.tier.ToString();

        UpdateLockout();
    }

    public void UpdateLockout() {
        bool unlocked = runesPanel.CurrentAbility.AbilityLevel >= RuneGroupData.tier;

        if(unlocked == true && revealed == false) {
            Tween fadeout = blockerFade.DOFade(0f, 0.25f);
            fadeout.onComplete = () => {
                dimmer.SetActive(false);
                revealed = true;
            };

            shimmer.transform.DOLocalMove(new Vector2(525f, 0f), 1f);

            return;
        }

        dimmer.SetActive(!unlocked);
    }

    public void OnChoiceSelected(RuneChoiceEntry choice) {
        currentChoice = choice;

        for (int i = 0; i < entries.Count; i++) {
            if (entries[i] != currentChoice) {
                entries[i].Deselect();
            }
        }

        currentChoice.Select();
    }

    public void ResetEntries() {
        for (int i = 0; i < entries.Count; i++) {

            entries[i].Deselect();
            Debug.Log("Resetting a rune group entry: " + entries[i].RuneItem.Data.itemName);
        }

        UpdateLockout();
    }


}
