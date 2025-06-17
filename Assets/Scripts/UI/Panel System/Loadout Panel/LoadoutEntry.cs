using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoadoutEntry : MonoBehaviour
{

    public enum LoadoutEntryType {
        Weapon,
        Skill,
        Item,
        UtilitySkill
    }

    public LoadoutEntryType loadoutType;

    [Header("Template")]
    public LoadoutSelectionEntry template;
    public Transform holder;

    [Header("No Items Text")]
    public GameObject noItemsText;

    private List<LoadoutSelectionEntry> selectionOptions = new List<LoadoutSelectionEntry>();

    public LoadoutSelectionEntry SelectedEntry { get; private set; }


    private List<ItemDefinition> items = new List<ItemDefinition>();

    private void Awake() {
        template.gameObject.SetActive(false);
    }

    public void Setup() {
        CreateItems();

        if(selectionOptions.Count > 0 )
            OnItemSelected(selectionOptions[0]);
    }


    public void OnItemSelected(LoadoutSelectionEntry entry) {
        SelectedEntry = entry;

        entry.Select();

        for (int i = 0; i < selectionOptions.Count; i++) {
            if (SelectedEntry != selectionOptions[i]) {
                selectionOptions[i].Deselect();
            }
        
        }

    }

    public void Unselect(LoadoutSelectionEntry entry) {
        entry.Deselect();
        SelectedEntry = null;
    }


    private void CreateItems() {
        Action creationMethod = loadoutType switch {
            LoadoutEntryType.Weapon => GetStartingWeapons,
            LoadoutEntryType.Skill => GetStartingSkills,
            LoadoutEntryType.Item => GetRecoveredItems,
            LoadoutEntryType.UtilitySkill => GetStartingUtilitySkills,
            _ => null
        };

        creationMethod?.Invoke();

        selectionOptions.PopulateList(items.Count, template, holder, true);
        for (int i = 0; i < items.Count; i++) {
            selectionOptions[i].Setup(items[i], this);
        }

    }


    private void GetStartingWeapons() {
        items = ItemSpawner.Instance.lootDatabase.GetStarterWeapons();
    }

    private void GetStartingSkills() {
        items = ItemSpawner.Instance.lootDatabase.GetStarterSkills();
    }

    private void GetStartingUtilitySkills() {
        items = ItemSpawner.Instance.lootDatabase.GetStarterUtilitySkills();
    }

    private void GetRecoveredItems() {
        items = ItemSpawner.Instance.lootDatabase.GetItemsByNames(SaveLoadUtility.SaveData.recoveredItems);

        if(items.Count < 1) {
            noItemsText.SetActive(true);
        }
        else {
            noItemsText.SetActive(false);
        }

    }

    public void SpawnSelectedItem() {
        if (SelectedEntry == null)
            return;
        
        ItemSpawner.SpawnItem(SelectedEntry.ItemDef, transform.position, true);
    }

}
