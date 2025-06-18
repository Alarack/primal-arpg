using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DebugPanel : BasePanel
{

    [Header("Item Template")]
    public DebugItemEntry itemTemplate;
    public Transform holder;

    public Toggle godModToggle;
    public TMP_InputField searchInputField;


    private List<DebugItemEntry> debugItemEntries = new List<DebugItemEntry>();

    protected override void Awake() {
        base.Awake();

        itemTemplate.gameObject.SetActive(false);   
    }


    public void OnGodModeToggeled() {
        EntityManager.ActivePlayer.debugGodMode = godModToggle.isOn;
    }

    public void OnSearchFieldChanged() {
        List<ItemDefinition> results = ItemSpawner.Instance.lootDatabase.SearchItems(searchInputField.text);

        debugItemEntries.ClearList();
        debugItemEntries.PopulateList(results.Count, itemTemplate, holder, true);
        for (int i = 0; i < debugItemEntries.Count; i++) {
            debugItemEntries[i].Setup(results[i]);
        }

    }


}
