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
    Skill,
    ClassSelection,
    StatBooster,
    Experience

}


[System.Serializable]
public class ItemData 
{
    public ItemType Type;
    public Entity.EntityClass entityClass;
    public string itemName;
    public string itemDescription;
    public float itemValue;
    public string runeAbilityTarget;
    public List<ItemSlot> validSlots = new List<ItemSlot>();
    public List<AbilityTag> abilityTags = new List<AbilityTag>();
    public Sprite itemIcon;
    public Sprite pickupIcon;
    public bool pickupOnCollision;

    public float minDamage;
    public float maxDamage;

    public List<StatModifierData> statModifierData = new List<StatModifierData>();
    public List<AbilityDefinition> abilityDefinitions = new List<AbilityDefinition>();
    public List<AbilityDefinition> learnableAbilities = new List<AbilityDefinition>();
    //public List<AbilityData> abilityData = new List<AbilityData>();

    public ItemData() {

    }

    public ItemData (StatName stat, float value) {
        Type = ItemType.StatBooster;
        itemName = stat.ToString() + " Booster";
        StatModifierData modData = StatModifierData.CreateBaseStatBooster(stat, value);
        statModifierData.Add(modData);
    }

    public Item GetDisplayItem() {
        return ItemFactory.CreateItem(this, EntityManager.ActivePlayer);
    }

    public string GetItemInfo() {

        if(Type == ItemType.Equipment) {
            if(validSlots.Count > 0) {

                if (validSlots[0] == ItemSlot.Ring1 || validSlots[0] == ItemSlot.Ring2) {
                    return "Ring";
                }
                else {
                    return validSlots[0].ToString();
                }
            }
            else {
                Debug.LogError("An eqipment item has no valid slots");
            }
        }

        if(Type == ItemType.Skill) {
            for(int i = 0; i < learnableAbilities.Count; i++) {
                if (learnableAbilities[i].AbilityData.tags.Count > 0) {
                    string result = learnableAbilities[i].AbilityData.tags[0].ToString() + " Skill";
                    return result;
                }
            }
        }

        if(Type == ItemType.Rune) {
            return abilityDefinitions[0].AbilityData.runeAbilityTarget + " Rune";
        }


        return "";
    }


}
