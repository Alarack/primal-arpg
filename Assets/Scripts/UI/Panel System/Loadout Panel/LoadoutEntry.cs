using System.Collections.Generic;
using UnityEngine;

public class LoadoutEntry : MonoBehaviour
{

    public enum LoadoutEntryType {
        Weapon,
        Skill,
        Item
    }

    public LoadoutEntryType loadoutType;

    [Header("Template")]
    public LoadoutSelectionEntry template;
    public Transform holder;

    private List<LoadoutSelectionEntry> selectionOptions = new List<LoadoutSelectionEntry>();



    private void Awake() {
        template.gameObject.SetActive(false);
    }

    private void SetupDisplay() {

    }


    private void GetStartingWeapons() {
        List<ItemDefinition> items = ItemSpawner.Instance.lootDatabase.GetStarterWeapons();

    }

    private void GetStartingSkills() {
        List<ItemDefinition> items = ItemSpawner.Instance.lootDatabase.GetStarterSkills();
    }

    private void GetRecoveredItems() {
        List<ItemDefinition> items = ItemSpawner.Instance.lootDatabase.GetItemsByNames(SaveLoadUtility.SaveData.recoveredItems);

    }
}
