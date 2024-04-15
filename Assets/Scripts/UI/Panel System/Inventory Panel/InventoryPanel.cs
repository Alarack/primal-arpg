using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LL.Events;
using TMPro;

public class InventoryPanel : BasePanel {

    public List<InventoryItemEntry> paperDollEntries = new List<InventoryItemEntry>();
    private List<InventoryItemEntry> inventoryEntries = new List<InventoryItemEntry>();

    public InventoryItemEntry forgeSlot;

    [Header("Template")]
    public int slotCount = 60;
    public Transform inventoryHolder;
    public InventoryItemEntry inventoryEntryTemplate;

    public GameObject dropZone;

    [Header("Testing Debug Things")]
    public TextMeshProUGUI cdrText;

    protected override void Awake() {
        base.Awake();
        CreateEmptySlots();
        SetupPaperDollSlots();
        

        inventoryEntryTemplate.gameObject.SetActive(false);
    }

    protected override void OnEnable() {
        base.OnEnable();
        EventManager.RegisterListener(GameEvent.ItemAquired, OnItemAquired);
        EventManager.RegisterListener(GameEvent.ItemUnequipped, OnItemUnequipped);
        EventManager.RegisterListener(GameEvent.ItemEquipped, OnItemEquipped);
        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatChanged);
    }
    protected override void OnDisable() {
        base.OnDisable();

        EventManager.RemoveMyListeners(this);
    }

    public override void Open() {
        base.Open();

        if (EntityManager.ActivePlayer == null)
            return;

        PopulateInventory();
        SetStatValues();
    }

    public override void Close() {
        base.Close();

        TooltipManager.Hide();
    }

    private void OnStatChanged(EventData data) {
        Entity target = data.GetEntity("Target");
        StatName stat = (StatName)data.GetInt("Stat");

        if (target != EntityManager.ActivePlayer)
            return;

       
        if(stat == StatName.CooldownReduction) {
            SetStatValues();
        }

    }

    private void SetStatValues() {

        if(EntityManager.ActivePlayer == null) {
            return;
        }

        cdrText.text = "Cooldown Reduction: " + TextHelper.FormatStat(StatName.CooldownReduction, EntityManager.ActivePlayer.Stats[StatName.CooldownReduction]);

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

    public void CheckForDupeEquips(InventoryItemEntry entry) {
        for (int i = 0;i < paperDollEntries.Count;i++) {
            if (paperDollEntries[i] == entry) {
                continue;
            }

            if (paperDollEntries[i].MyItem == entry.MyItem) {
                paperDollEntries[i].Remove();
            }
        }
    }

    private void OnItemAquired(EventData data) {
        Item item = data.GetItem("Item");

        if(item.Data.Type == ItemType.Equipment)
            AddToFirstEmptySlot(item);
    }

    private void OnItemEquipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item == null)
            return;

        if (EntityManager.ActivePlayer.Inventory.ItemOwned(item) == true) {

            if (item.Data.Type == ItemType.Equipment)
                GetPaperDollSlot(item.CurrentSlot).Add(item);
        }
    }

    private void OnItemUnequipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item == null)
            return;

        if (EntityManager.ActivePlayer.Inventory.ItemOwned(item) == true) {
            
            if(IsItemInInventory(item) == false && item.Data.Type == ItemType.Equipment)
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

    private InventoryItemEntry GetPaperDollSlot(ItemSlot slot) {
        for (int i = 0; i < paperDollEntries.Count; i++) {
            if (paperDollEntries[i].slot == slot) {
                return paperDollEntries[i];
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

    public void OnForgeClicked() {
        if (forgeSlot.MyItem == null)
            return;

        List<ItemData> affixData = ItemSpawner.CreateItemAffixSet(5);

        for (int i = 0; i < affixData.Count; i++) {
            Debug.Log("Created an Affix: " + affixData[i].affixStatTarget + " " + TextHelper.FormatStat(affixData[i].affixStatTarget, affixData[i].statModifierData[0].value) + " Tier: " + affixData[i].tier);
        }
    }


}
