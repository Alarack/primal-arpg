using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LL.Events;
using static UnityEngine.EventSystems.EventTrigger;
using System;

public class Inventory : MonoBehaviour {

    public Entity Owner { get; private set; }

    public Dictionary<string, float> currencyDictionary = new Dictionary<string, float>();

    private List<Item> ownedItems = new List<Item>();
    private Dictionary<ItemSlot, Item> equippedItems = new Dictionary<ItemSlot, Item>();

    private List<Item> equippedRunes = new List<Item>();

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

        return GetItems(ItemType.Equipment);
        
        
        //List<Item> results = new List<Item>();    
        //for (int i = 0; i < ownedItems.Count; i++) {
        //    if (ownedItems[i].Equipped == false && ownedItems[i].Data.Type == ItemType.Equipment)
        //        results.Add(ownedItems[i]);
        //}

        //return results;
    }

    public List<Item> GetItems(ItemType type, bool unequippedOnly = true) {
        List<Item> results = new List<Item>();
        for (int i = 0; i < ownedItems.Count; i++) {

            if(unequippedOnly == true && ownedItems[i].Equipped == true) {
                continue;
            }
            
            if (ownedItems[i].Data.Type == type)
                results.Add(ownedItems[i]);
        }

        return results;
    }

    public bool ItemOwned(Item item) {
        return ownedItems.Contains(item);
    }

    public bool ItemOwned(ItemDefinition item) {
        for (int i = 0; i < ownedItems.Count; i++) {
            if (ownedItems[i].Data.itemName == item.itemData.itemName) {
                return true;
            }
        }

        return false;
    }

    public void Add(Item item) {
        
        if(item.Data.Type == ItemType.Currency) {
            AdjustCurrency(item);
            return;
        }
        
        if (ownedItems.AddUnique(item) == false) {
            Debug.LogWarning("An item: " + item.Data.itemName + " was added to " + Owner.EntityName + "'s inventory, but it was already there");
            return;
        }
        item.Owner = Owner;

        EventData data = new EventData();
        data.AddItem("Item", item);

        EventManager.SendEvent(GameEvent.ItemAquired, data);


        if(item.Data.Type == ItemType.Equipment) {
            Item existingItem = GetItemInSlot(item.Data.validSlots[0]);

            if (existingItem == null) {
                EquipItemToSlot(item, item.Data.validSlots[0]);
            }
        }
    }


    private void AdjustCurrency(Item item) {

        if(currencyDictionary.TryGetValue(item.Data.itemName, out float count) == true) {
            currencyDictionary[item.Data.itemName] += item.Data.itemValue;
        }
        else {
            currencyDictionary.Add(item.Data.itemName, item.Data.itemValue);
        }


        SendCurrencyChangedEvent(item);
    }

    private void SendCurrencyChangedEvent(Item item) {
        EventData data = new EventData();
        data.AddFloat("Value", item.Data.itemValue);
        data.AddFloat("Current Balance", currencyDictionary[item.Data.itemName]);
        data.AddString("Currency Name", item.Data.itemName);


        EventManager.SendEvent(GameEvent.CurrencyChanged, data);
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
            
            if(equippedRunes.RemoveIfContains(item) == true) {
                item.UnEquip();
            }


        }
    }

    public void EquipRune(Item item, Ability targetAbility) {
        if(equippedRunes.AddUnique(item) == true) {
            item.Equip(ItemSlot.RuneSlot);

            EventData data = new EventData();
            data.AddItem("Item", item);
            data.AddAbility("Ability", targetAbility);
            data.AddAbility("Cause", item.Abilities[0]);

            EventManager.SendEvent(GameEvent.RuneEquipped, data);

        }
        else {
            Debug.LogError("Tried to equip a rune: " + item.Data.itemName + " but it was already equipped");

        }
    }

    public void UnEquipRune(Item item, Ability targetAbility) {
        if(equippedRunes.RemoveIfContains(item) == true) {
            item.UnEquip();

            EventData data = new EventData();
            data.AddItem("Item", item);
            data.AddAbility("Ability", targetAbility);
            data.AddAbility("Cause", item.Abilities[0]);

            EventManager.SendEvent(GameEvent.RuneUnequipped, data);

        }
        else {
            Debug.LogError("Tried to Unequip a rune: " + item.Data.itemName + " but it wasn't equipped");

        }
    }

    public void EquipItemToSlot(Item item, ItemSlot slot) {
        
        if(item.Equipped == true) {
            UnEquipItem(item);
        }
        
        
        Item existingItem = GetItemInSlot(slot);

        if (existingItem != null && existingItem != item) {
            UnEquipItem(existingItem);
        }

        //Debug.Log("Equipping: " + item.Data.itemName);

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
        //else {
        //    Debug.Log("Weapon is null");
        //}

        return 5f;
    }

    public float GetAverageDamageRoll() {
        if (CurrentWeapon != null) {
            return CurrentWeapon.Averagedamage;
        }

        return 5f;
    }

    public Tuple<float, float> GetDamageRange() {
        Tuple<float, float> result = new Tuple<float, float>(5f, 5f);
        
        if (CurrentWeapon != null) {
            result = new Tuple<float, float>(CurrentWeapon.minDamage, CurrentWeapon.maxDamage);
        }

        return result;
    }

    public ItemWeapon GetWeapon() {

        ItemWeapon weapon = GetItemInSlot(ItemSlot.Weapon) as ItemWeapon;
        return weapon;
    }

}


