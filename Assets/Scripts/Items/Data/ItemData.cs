using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemSlot {
    None,
    Weapon,
    Armor,
    Gloves,
    Ring,
    Neck,
    Pants,
    Boots,
    Trinket,
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
    public Sprite itemIcon;

    public float minDamage;
    public float maxDamage;

    public List<StatModifierData> statModifierData = new List<StatModifierData>();
    public List<AbilityData> abilityData = new List<AbilityData>();


}
