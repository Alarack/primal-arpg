using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour {

    public Entity Owner { get; private set; }


    public List<InventorySlot> slots = new List<InventorySlot>();

    private List<Item> ownedItems = new List<Item>();
    private List<Item> equippedItems = new List<Item>();

    private Dictionary<ItemSlot, List<Item>> inventoryDict = new Dictionary<ItemSlot, List<Item>>();

    public ItemWeapon CurrentWeapon { get; private set; }

    private void Awake() {
        Owner = GetComponent<Entity>();
    }

    private void SetupDictionary() {
        ItemSlot[] slots = System.Enum.GetValues(typeof(ItemSlot)) as ItemSlot[];

        for (int i = 0; i < slots.Length; i++) {
            inventoryDict.Add(slots[i], new List<Item>());

        }
    }

    public void Add(Item item) {
        if (ownedItems.AddUnique(item) == false) {
            Debug.LogWarning("An item: " + item.Data.itemName + " was added to " + Owner.EntityName + "'s inventory, but it was already there");
        }
    }

    public void Remove(Item item) {
        ownedItems.RemoveIfContains(item);
    }

    public void EquipItem2(Item item) {
        if(inventoryDict.TryGetValue(item.Data.slot, out List<Item> items) == true) {
            if (item.Data.slot == ItemSlot.Ring) {
                if (items.Count < 2) {
                    items.Add(item);
                    item.Equip();
                }
            }
            else {

            }
        }
    }

    public void EquipItem(Item item) {

        InventorySlot slot = GetSlot(item.Data.slot);

        if (slot == null) {
            Debug.LogError("Couldn't find a slot of type: " + item.Data.slot);
            return;
        }

        if (slot.slottedItem != null) {
            UnEquipItem(slot.slottedItem);
        }

        slot.slottedItem = item;

        equippedItems.Add(item);
        item.Equip();

        if (item is ItemWeapon) {
            CurrentWeapon = item as ItemWeapon;
        }
    }

    public void UnEquipItem(Item item) {

        InventorySlot slot = GetSlotByItem(item);

        if (slot == null) {
            Debug.LogError("Couldn't find an eqipped item: " + item.Data.itemName);
            return;
        }

        slot.slottedItem = null;

        if (equippedItems.RemoveIfContains(item) == true) {
            item.UnEquip();
        }
        else {
            Debug.LogWarning("An item: " + item.Data.itemName + " was told to unequip from " + Owner.EntityName + ", but it wasn't equipped");
        }
    }

    private Item GetItemInSlot(ItemSlot slot) {
        if (inventoryDict.TryGetValue(slot, out List<Item> items) == true) {
            return items[0];
        }
        return null;
    }



    private InventorySlot GetSlot(ItemSlot slot) {

        InventorySlot emptySlot = null;
        InventorySlot filledSlot = null;

        for (int i = 0; i < slots.Count; i++) {
            if (slots[i].slotType == slot) {
                if (slots[i].slottedItem == null) {
                    emptySlot = slots[i];
                    break;
                }
                else {
                    filledSlot = slots[i];
                    break;
                }
            }
        }

        return emptySlot != null ? emptySlot : filledSlot;
    }

    private InventorySlot GetSlotByItem(Item item) {
        for (int i = 0; i < slots.Count; i++) {
            if (slots[i].slottedItem == item)
                return slots[i];
        }

        return null;
    }



    public float GetDamageRoll() {
        ItemWeapon weapon = GetWeapon();
        if (weapon != null) {
            return weapon.DamageRoll;
        }

        return 0f;
    }

    public ItemWeapon GetWeapon() {
        for (int i = 0; i < equippedItems.Count; i++) {
            if (equippedItems[i] is ItemWeapon) {
                return equippedItems[i] as ItemWeapon;
            }
        }

        return null;
    }

}

[System.Serializable]
public class InventorySlot {
    public ItemSlot slotType;
    public Item slottedItem;


}
