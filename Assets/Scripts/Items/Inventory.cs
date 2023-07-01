using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LL.Events;
using static UnityEngine.EventSystems.EventTrigger;

public class Inventory : MonoBehaviour {

    public Entity Owner { get; private set; }



    private List<Item> ownedItems = new List<Item>();
    private Dictionary<ItemSlot, Item> equippedItems = new Dictionary<ItemSlot, Item>();

    public ItemWeapon CurrentWeapon { get { return GetWeapon(); } }

    private void Awake() {
        Owner = GetComponent<Entity>();
        SetupDictionary();
    }

    private void SetupDictionary() {
        ItemSlot[] slots = System.Enum.GetValues(typeof(ItemSlot)) as ItemSlot[];

        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] == ItemSlot.None)
                continue;
            if (slots[i] == ItemSlot.Inventory)
                continue;

            equippedItems.Add(slots[i], null);
        }
    }

    public List<Item> GetInventoryItems() {
        List<Item> results = new List<Item>();    
        for (int i = 0; i < ownedItems.Count; i++) {
            if (ownedItems[i].Equipped == false)
                results.Add(ownedItems[i]);
        }

        return results;
    }
    public bool ItemOwned(Item item) {
        return ownedItems.Contains(item);
    }

    public void Add(Item item) {
        if (ownedItems.AddUnique(item) == false) {
            Debug.LogWarning("An item: " + item.Data.itemName + " was added to " + Owner.EntityName + "'s inventory, but it was already there");
            return;
        }
        item.Owner = Owner;

        EventData data = new EventData();
        data.AddItem("Item", item);

        EventManager.SendEvent(GameEvent.ItemAquired, data);
    }

    public void Remove(Item item) {
        if (ownedItems.RemoveIfContains(item) == true) {
            EventData data = new EventData();
            data.AddItem("Item", item);

            EventManager.SendEvent(GameEvent.ItemDropped, data);

           
            if(equippedItems.TryGetValue(item.CurrentSlot, out Item equippedItem)) {
                //if(equippedItem != null)
                    UnEquipItem(equippedItem); 
            }


        }
    }


    public void EquipItemToSlot(Item item, ItemSlot slot) {
        Item existingItem = GetItemInSlot(slot);

        if (existingItem != null) {
            UnEquipItem(existingItem);
        }

        equippedItems[slot] = item;
        item.Equip(slot);
    }

    public void EquipItem(Item item) {

        bool equipSucessful = false;

        Dictionary<ItemSlot, Item> existingItems = new Dictionary<ItemSlot, Item>();
        foreach (ItemSlot slot in item.Data.validSlots) {
            Item existingItem = GetItemInSlot(slot);

            existingItems.Add(slot, existingItem);

        }


        foreach (var entry in existingItems) {
            if(entry.Value == null) {
                equippedItems[entry.Key] = item;
                item.Equip(entry.Key);
                equipSucessful = true;
                //Debug.Log("Found an empty slot for : " + item.Data.itemName + " :: " + entry.Key);
                break;
            }
        }

        if(equipSucessful == false) {
            ItemSlot firstSlot = item.Data.validSlots[0];

            Item replacedItem = existingItems[firstSlot];
            UnEquipItem(replacedItem);

            equippedItems[firstSlot] = item;
            item.Equip(firstSlot);

            //Debug.Log("Replacing an item : " + replacedItem.Data.itemName + " with " + item.Data.itemName + " in slot: " + firstSlot);

        }

        //Debug.Log("Equipped: " + item.Data.itemName);
    }

    public void UnEquipItem(Item item) {


        if(item.CurrentSlot == ItemSlot.None) {
            Debug.LogError("Tired to unequip an item: " + item.Data.itemName + ", but it had no Current Slot");
        }

        equippedItems[item.CurrentSlot] = null;

        item.UnEquip();
      
    }

    private Item GetItemInSlot(ItemSlot slot) {
        if (equippedItems.TryGetValue(slot, out Item item) == true) {
            
            if(item != null) {
                //Debug.Log("Found: " + item.Data.itemName + " in slot: " + slot);
                return item;
            }
        }


        //Debug.Log("No items in Slot: " + slot);

        return null;
    }

    //private List<Item> GetAllItemsInSlots(List<ItemSlot> slots) {

    //    List<Item> items = new List<Item>();

    //    foreach (ItemSlot slot in slots) {
    //        Item targetItem = GetItemInSlot(slot);

    //        if (targetItem != null) {
    //            items.Add(targetItem);
    //        }
    //    }

    //    return items;
    //}

    public float GetDamageRoll() {

        if (CurrentWeapon != null) {
            return CurrentWeapon.DamageRoll;
        }
        else {
            Debug.Log("Weapon is null");
        }

        return 5f;
    }

    public ItemWeapon GetWeapon() {

        ItemWeapon weapon = GetItemInSlot(ItemSlot.Weapon) as ItemWeapon;
        return weapon;
    }

}


