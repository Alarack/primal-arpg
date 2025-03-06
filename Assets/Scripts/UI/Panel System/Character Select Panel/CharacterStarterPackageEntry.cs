using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CharacterStarterPackageEntry : MonoBehaviour
{

    [Header("Template")]
    public StarterItemDisplayEntry template;
    public Transform holder;

    private List<StarterItemDisplayEntry> itemEntries = new List<StarterItemDisplayEntry>();
    private CharacterSelectPanel characterSelectPanel;


    private void Awake() {
        template.gameObject.SetActive(false);
    }

    public void Setup(CharacterSelectPanel selectionPanel, params ItemDefinition[] items) {
        characterSelectPanel = selectionPanel;
        
        itemEntries.PopulateList(items.Length, template, holder, true);
        for (int i = 0; i < items.Length; i++) {
            itemEntries[i].Setup(items[i]);
        }
        
    }


    public void OnSelectClicked() {
        for (int i = 0;i < itemEntries.Count;i++) {
            ItemSpawner.SpawnItem(itemEntries[i].ItemDef, transform.position, true);
        }

        characterSelectPanel.StartGame();
    }


}
