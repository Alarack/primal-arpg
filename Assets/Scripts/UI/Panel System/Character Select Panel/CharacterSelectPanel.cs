using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectPanel : BasePanel
{

    [Header("Template")]
    public CharacterChoiceEntry template;
    public Transform holder;

    [Header("Class Data")]
    public List<ItemDefinition> classItems = new List<ItemDefinition>();

    private List<CharacterChoiceEntry> entries = new List<CharacterChoiceEntry>();

    protected override void Awake() {
        base.Awake();
        template.gameObject.SetActive(false);
    }
    
    public override void Open() {
        base.Open();

        entries.PopulateList(classItems.Count, template, holder, true);
        for (int i = 0; i < entries.Count; i++) {
            entries[i].Setup(classItems[i], this);
        }
    }


    public void OnBackClicked() {
        PanelManager.OpenPanel<MainMenuPanel>();
        Close();
    }

    public void OnClassSelected(CharacterChoiceEntry entry) {
        ItemSpawner.SpawnItem(entry.ClassItem, transform.position, true);
        ItemSpawner.SpawnItem(entry.ChosenItem, transform.position, true);
        Close();

        RoomManager.SpawnRoomPortals();

        EntityManager.ActivePlayer.Inventory.AddEXP(25f);
        PanelManager.OpenPanel<HotbarPanel>();
        PanelManager.OpenPanel<HUDPanel>();
        
        PanelManager.OpenPanel<LevelUpPanel>();
    }


}
