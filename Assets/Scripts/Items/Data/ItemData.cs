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
    Inventory,
    RuneSlot,
}

public enum ItemType { 
    None,
    Equipment,
    Rune,
    Currency,
    Skill

}


[System.Serializable]
public class ItemData 
{
    public ItemType Type; 
    public string itemName;
    public string itemDescription;
    public float itemValue;
    public string runeAbilityTarget;
    public List<ItemSlot> validSlots = new List<ItemSlot>();
    public Sprite itemIcon;
    public Sprite pickupIcon;
    public bool pickupOnCollision;

    public float minDamage;
    public float maxDamage;

    public List<StatModifierData> statModifierData = new List<StatModifierData>();
    public List<AbilityDefinition> abilityDefinitions = new List<AbilityDefinition>();
    public List<AbilityDefinition> learnableAbilities = new List<AbilityDefinition>();
    //public List<AbilityData> abilityData = new List<AbilityData>();


}
