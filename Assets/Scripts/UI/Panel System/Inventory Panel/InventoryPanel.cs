using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LL.Events;

public class InventoryPanel : BasePanel {

    public List<InventoryItemEntry> paperDollEntries = new List<InventoryItemEntry>();
    private List<InventoryItemEntry> inventoryEntries = new List<InventoryItemEntry>();

    [Header("Template")]
    public int slotCount = 60;
    public Transform inventoryHolder;
    public InventoryItemEntry inventoryEntryTemplate;

    public GameObject dropZone;

    protected override void Awake() {
        base.Awake();
        CreateEmptySlots();
        SetupPaperDollSlots();
        PopulateInventory();

        inventoryEntryTemplate.gameObject.SetActive(false);
    }

    protected override void OnEnable() {
        base.OnEnable();
        EventManager.RegisterListener(GameEvent.ItemAquired, OnItemAquired);
        EventManager.RegisterListener(GameEvent.ItemUnequipped, OnItemUnequipped);
    }
    protected override void OnDisable() {
        base.OnDisable();

        EventManager.RemoveMyListeners(this);
    }

    public override void Open() {
        base.Open();

    }

    private void CreateEmptySlots() {
        inventoryEntries.PopulateList(slotCount, inventoryEntryTemplate, inventoryHolder, true);
        for (int i = 0; i < inventoryEntries.Count; i++) {
            inventoryEntries[i].Setup(null, this);
        }
    }

    private void SetupPaperDollSlots() {
        for (int i = 0; i < paperDollEntries.Count; i++) {
            paperDollEntries[i].Setup(null, this);
        }
    }

    public void AddToFirstEmptySlot(Item item) {

        InventoryItemEntry emptySlot = GetEmptyInventorySlot();
        if (emptySlot != null) {
            emptySlot.Add(item);
        }
        else {
            Debug.LogWarning("Inventory is full");
            //TODO: Drop item;
        }
    }

    private void OnItemAquired(EventData data) {
        Item item = data.GetItem("Item");
        AddToFirstEmptySlot(item);
    }

    private void OnItemEquipped(EventData data) {

    }

    private void OnItemUnequipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item == null)
            return;

        if (EntityManager.ActivePlayer.Inventory.ItemOwned(item) == true) {
            
            if(IsItemInInventory(item) == false)
                GetEmptyInventorySlot().Add(item);
        }
    }


    private void OnItemDropped(EventData data) {

    }

    private InventoryItemEntry GetEmptyInventorySlot() {
        for (int i = 0; i < inventoryEntries.Count; i++) {
            if (inventoryEntries[i].MyItem == null) {
                return inventoryEntries[i];
            }
        }

        return null;
    }

    private bool IsItemInInventory(Item item) {
        for (int i = 0; i < inventoryEntries.Count; i++) {
            if (inventoryEntries[i].MyItem == item) 
                return true;
        }

        return false;
    }

    public void HighlightValidSLots() {
        if (InventoryItemEntry.DraggedInventoryItem == null)
            return;

        for (int i = 0; i < paperDollEntries.Count; i++) {
            if (InventoryItemEntry.DraggedInventoryItem.MyItem.Data.validSlots.Contains(paperDollEntries[i].slot)) {
                paperDollEntries[i].ShowHighlight();
            }
            else {
                paperDollEntries[i].HideHighlight();
            }
        }

    }

    public void HideAllHighlights() {
        for (int i = 0; i < paperDollEntries.Count; i++) {
            paperDollEntries[i].HideHighlight();
        }
    }

    private void PopulateInventory() {
        ClearInventory();
        List<Item> items = EntityManager.ActivePlayer.Inventory.GetInventoryItems();
        for (int i = 0; i < items.Count; i++) {
            inventoryEntries[i].Add(items[i]);
        }
    }

    private void ClearInventory() {
        for (int i = 0; i < inventoryEntries.Count; i++) {
            inventoryEntries[i].Remove();
        }
    }


}
