using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    Class,
    ForgeSlot
}

public enum ItemType { 
    None,
    Equipment,
    Rune,
    Currency,
    Skill,
    ClassSelection,
    StatBooster,
    Experience,
    SkillPoint,
    HealthPotion

}


[System.Serializable]
public class ItemData 
{
    public ItemType Type;
    public StatName affixStatTarget = StatName.Vitality;
    public Entity.EntityClass entityClass;
    public string itemName;
    public string itemDescription;
    public string secondaryDescription;
    public float itemValue;
    public string runeAbilityTarget;
    public List<ItemSlot> validSlots = new List<ItemSlot>();
    public List<AbilityTag> abilityTags = new List<AbilityTag>();
    public string targetedAbilityName;
    public Sprite itemIcon;
    public Sprite pickupIcon;
    public bool pickupOnCollision;
    public int tier;
    public int baseAffixSlots = 2;

    public float minDamage;
    public float maxDamage;

    public List<StatModifierData> statModifierData = new List<StatModifierData>();
    public List<AbilityDefinition> abilityDefinitions = new List<AbilityDefinition>();
    public List<AbilityDefinition> learnableAbilities = new List<AbilityDefinition>();
    public List<AbilityDefinition> classPreviewAbilities = new List<AbilityDefinition>();
    public List<ItemDefinition> startingItemOptions = new List<ItemDefinition>();
    //public List<AbilityData> abilityData = new List<AbilityData>();

    //[System.NonSerialized]
    //[HideInInspector]
    //public List<ItemData> itemAffixes = new List<ItemData>();

    public ItemData() {

    }


    public ItemData (StatName stat, float value, int tier = 1) {
        Type = ItemType.StatBooster;
        affixStatTarget = stat;
        this.tier = tier;
        itemName = stat.ToString() + " Booster";
        StatModifierData modData = StatModifierData.CreateBaseStatBooster(stat, value);
        statModifierData.Add(modData);
    }

    //public void AddAffix(ItemData affixData) {
    //    itemAffixes.Add(affixData);
    //    //statModifierData.AddRange(affixData.statModifierData);
    //}

    //public void ClearAffixes() {
    //    itemAffixes.Clear();
    //}

    public Item GetDisplayItem() {
        return ItemFactory.CreateItem(this, EntityManager.ActivePlayer);
    }

    public List<StatModifier> CreateStatModifiers(object source) {
        List<StatModifier> results = new List<StatModifier>();
        for (int i = 0; i < statModifierData.Count; i++) {
            StatModifier mod = new StatModifier(statModifierData[i], source);
            results.Add(mod);
        }
        return results;
    }

    public string GetTier() {
        if (tier == 0)
            return "";

        string result = tier switch {
            1 => TextHelper.ColorizeText("I", Color.white),
            2 => TextHelper.ColorizeText("II", Color.green),
            3 => TextHelper.ColorizeText("III", Color.cyan),
            4 => TextHelper.ColorizeText("IV", Color.magenta),
            5 => TextHelper.ColorizeText("V", Color.red),
            _ => "",
        };

        return result;
    }

    public string GetShortTooltip() {
        StringBuilder builder = new StringBuilder();

        builder.Append(TextHelper.ColorizeText(statModifierData[0].targetStat.ToString().SplitCamelCase(), GetTierColor(tier)))
            .Append(TextHelper.ColorizeText(" ", GetTierColor(tier))).Append(GetTier());



        return builder.ToString();
    }

    public string GetAffixTooltip() {
        StringBuilder builder = new StringBuilder();
        
        builder.Append(TextHelper.ColorizeText("Tier - ", GetTierColor(tier))).Append(GetTier())
               .Append(" ")
               .Append(statModifierData[0].targetStat.ToString().SplitCamelCase())
               .Append(": ")
               .Append(TextHelper.FormatStat(statModifierData[0].targetStat, statModifierData[0].value))

               .AppendLine();


        return builder.ToString();
    }

    public Sprite GetAffixIcon() {
        Sprite targetSprite = GameManager.Instance.tooltipData.GetStatIcon(statModifierData[0].targetStat);

        return targetSprite;
    }

    public Color GetTierColor(int tier) {
        if (tier == 0)
            return Color.white;

        Color result = tier switch {
            1 => Color.white,
            2 => Color.green,
            3 => Color.cyan,
            4 => Color.magenta,
            5 => Color.red,
            _ => Color.white,
        };

        return result;
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
