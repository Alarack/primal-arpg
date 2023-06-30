using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemSlot {
    None,
    Weapon,
    Armor,
    Gloves,
    Ring1,
    Ring2,
    Neck,
    Pants,
    Boots,
    Trinket1,
    Trinket2,
    Belt,
    Head,
}

[System.Serializable]
public class ItemData 
{
    public string itemName;
    public string itemDescription;
    public float itemValue;
    public ItemSlot slot;
    public List<ItemSlot> validSlots = new List<ItemSlot>();
    public Sprite itemIcon;
    public Sprite pickupIcon;
    public bool pickupOnCollision;

    public float minDamage;
    public float maxDamage;

    public List<StatModifierData> statModifierData = new List<StatModifierData>();
    public List<AbilityData> abilityData = new List<AbilityData>();


}
