using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CharacterSelectPanel : BasePanel
{

    [Header("Template")]
    public CharacterChoiceEntry template;
    public Transform holder;

    [Header("Anchors")]
    public Transform selectedClassAnchor;

    [Header("Class Data")]
    public List<ItemDefinition> classItems = new List<ItemDefinition>();

    private List<CharacterChoiceEntry> entries = new List<CharacterChoiceEntry>();


    private CharacterChoiceEntry selectedClass;

    protected override void Awake() {
        base.Awake();
        template.gameObject.SetActive(false);
    }
    
    public override void Open() {
        base.Open();

        new Task(PopulateClassOptions());

        //entries.PopulateList(classItems.Count, template, holder, true);
        //for (int i = 0; i < entries.Count; i++) {
        //    entries[i].Setup(classItems[i], this);
        //}
    }

    public override void Close() {
        base.Close();

        TooltipManager.Hide();
    }


    private IEnumerator PopulateClassOptions() {
        WaitForSeconds waiter = new WaitForSeconds(0.2f);
        
        entries.PopulateList(classItems.Count, template, holder, true);
        for (int i = 0; i < entries.Count; i++) {
            entries[i].Setup(classItems[i], this);
            yield return waiter;
        }

    }


    public void OnBackClicked() {
        PanelManager.OpenPanel<MainMenuPanel>();
        Close();
    }

    public void OnClassInfoClicked(CharacterChoiceEntry entry) {
        selectedClass = entry;
        selectedClass.Select();
        selectedClass.Show();

        for (int i = 0; i < entries.Count; i++) {
            if (entries[i] != entry)
                entries[i].Hide();
        }
    }

    public void UnhideAllEntries(CharacterChoiceEntry entry) {
        for (int i = 0; i < entries.Count; i++) {
            if (entries[i] != entry)
                entries[i].Show();
        }
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
