using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Loot Database", fileName = "Loot Database")]
public class LootDatabase : ScriptableObject {

    public Dictionary<ItemType, List<ItemDefinition>> itemDict = new Dictionary<ItemType, List<ItemDefinition>>();

    public Dictionary<ItemSlot, List<ItemDefinition>> itemsBySlot = new Dictionary<ItemSlot, List<ItemDefinition>>();

    public Dictionary<AbilityTag, List<ItemDefinition>> itemsByTag = new Dictionary<AbilityTag, List<ItemDefinition>>();

    public Dictionary<string, ItemDefinition> itemsByName = new Dictionary<string, ItemDefinition>();

    public Dictionary<StatName, ItemData> statBoosters = new Dictionary<StatName, ItemData>();


    private ItemDefinition[] allItems;
    private AbilityTag[] allTags;

    public float baseEnchantingCost = 25f;
    public int metaUnlockValue = 50;

    public List<ItemStatAffixData> itemStatAffixes = new List<ItemStatAffixData>();
    public List<ItemIconData> itemIcons = new List<ItemIconData>();

    public Dictionary<StatName, ItemStatAffixData> itemAffixes = new Dictionary<StatName, ItemStatAffixData>();


    public Sprite GetItemIconByType(ItemType type) {

        Sprite target = itemIcons.Where(i => i.type == type).FirstOrDefault().icon;
        
        return target;
        
        //Sprite sprite = type switch {
        //    ItemType.None => throw new System.NotImplementedException(),
        //    ItemType.Equipment => throw new System.NotImplementedException(),
        //    ItemType.Rune => throw new System.NotImplementedException(),
        //    ItemType.Currency => throw new System.NotImplementedException(),
        //    ItemType.Skill => throw new System.NotImplementedException(),
        //    ItemType.ClassSelection => throw new System.NotImplementedException(),
        //    ItemType.StatBooster => throw new System.NotImplementedException(),
        //    ItemType.Experience => throw new System.NotImplementedException(),
        //    ItemType.SkillPoint => throw new System.NotImplementedException(),
        //    ItemType.HealthPotion => throw new System.NotImplementedException(),
        //    _ => null
        //};
    }


    public ItemDefinition GetItemByName(string name) {
        ItemDefinition result = null;

        itemsByName.TryGetValue(name, out result);

        return result;
    }

    public List<ItemDefinition> GetItemsByNames(List<string> names) {
        List<ItemDefinition> results = new List<ItemDefinition>();
        foreach (string name in names) {
            ItemDefinition item = GetItemByName(name);
            if (item != null) {
                results.Add(item);
            }
        }

        return results;
    }

    public List<ItemDefinition> GetStarterSkills() {
        List<ItemDefinition> results = new List<ItemDefinition>();

        foreach (var entry in itemDict[ItemType.Skill]) {
            if (entry.startingItem == true)
                results.Add(entry);
        }

        return results;
    }

    public List<ItemDefinition> GetStarterWeapons() {
        List<ItemDefinition> results = new List<ItemDefinition> ();

        foreach (var entry in itemsBySlot[ItemSlot.Weapon]) {
            if (entry.devItem == true)
                continue;
            
            if(entry.startingItem == true)
                results.Add(entry);
        }

        return results;
    }

    private ItemDefinition GetRandomEquipment<TKey>(Dictionary<TKey, List<ItemDefinition>> dict, TKey key, List<ItemDefinition> exclusions) where TKey : struct {
        List<ItemDefinition> allPossibleItems = dict[key];
        List<ItemDefinition> filteredList = new List<ItemDefinition>();

        allPossibleItems.Shuffle();

        for (int i = 0; i < allPossibleItems.Count; i++) {
            if (allPossibleItems[i].devItem == true || allPossibleItems[i].startingItem == true || allPossibleItems[i].itemData.Type == ItemType.ClassSelection)
                continue;
            
            
            if (exclusions.Contains(allPossibleItems[i])) {
                //Debug.Log(allPossibleItems[i].itemData.itemName + " is a dupe");
                continue;
            }

            if (GameManager.AllowDuiplicatSkills == false ) {
                if(allPossibleItems[i].itemData.Type == ItemType.Skill) {
                    if(CheckForDuplicateSkills(allPossibleItems[i]) == true) {
                        continue;
                    }
                }
            }

            if (allPossibleItems[i].itemData.Type == ItemType.Equipment) {
                if (CheckForDupeEquipment(allPossibleItems[i]) == true)
                    continue;

                if (CheckForInvalidItemTag(allPossibleItems[i]) == true) 
                    continue;

                if (CheckForTargetAbility(allPossibleItems[i]) == false)
                    continue;

                
            }

            if (allPossibleItems[i].itemData.Type == ItemType.Rune) {
                if (CheckForInvalidRune(allPossibleItems[i]) == true) 
                    continue;
            }

            filteredList.Add(allPossibleItems[i]);
        }

        if (filteredList.Count == 0)
            return null;

        int randomIndex = Random.Range(0, filteredList.Count);
        return filteredList[randomIndex];

    }

    private bool CheckForDuplicateSkills(ItemDefinition item) {
        //Debug.LogWarning("Checking if " + item.itemData.itemName + " is a skill");

        if (item.itemData.learnableAbilities.Count > 0) {
            if (EntityManager.ActivePlayer.AbilityManager.HasAbility(item.itemData.learnableAbilities[0]) == true) {

                //Debug.LogWarning("Duplicate Skill Detected: " + item.itemData.learnableAbilities[0].AbilityData.abilityName);

                return true;
            }
            //else {
            //    Debug.LogWarning("Not yet seen: " + item.itemData.learnableAbilities[0].AbilityData.abilityName);
            //    return false;
            //}
        }
        //else {
        //    Debug.LogWarning("No learnable abilites found on: " + item.itemData.itemName);
        //    return false;
        //}

        return false;
    }

    private bool CheckForInvalidRune(ItemDefinition rune) {

        string targetAbility = rune.itemData.runeAbilityTarget;

        if(string.IsNullOrEmpty(targetAbility) == true)
            return false;

        if (EntityManager.ActivePlayer.AbilityManager.HasAbility(targetAbility) == false) {
            return true;
        }


        return false;

    }

    private bool CheckForDupeEquipment(ItemDefinition item) {
        return EntityManager.ActivePlayer.Inventory.ItemOwned(item);
    }

    private bool CheckForTargetAbility(ItemDefinition item) {
        
        if(string.IsNullOrEmpty(item.itemData.targetedAbilityName) == true) 
            return true;

        bool hasAbility = EntityManager.ActivePlayer.AbilityManager.HasAbility(item.itemData.targetedAbilityName);



        return hasAbility;
    }

    private bool CheckForInvalidItemTag(ItemDefinition item) {

        List<AbilityTag> relevantTags = EntityManager.ActivePlayer.AbilityManager.GetRelevantTags();

        //for (int i = 0; i < relevantTags.Count; i++) {
        //    Debug.Log(relevantTags[i] + " is a relevant tag");
        //}

        //for (int i = 0; i < item.itemData.abilityTags.Count; i++) {
        //    Debug.Log("Item Tag: " + item.itemData.abilityTags[i]);
        //}

        if (item.itemData.abilityTags.Count == 0)
            return false;

        for (int i = 0; i < item.itemData.abilityTags.Count; i++) {
            if (relevantTags.Contains(item.itemData.abilityTags[i])) 
                return false;
        }

        //Debug.Log(item.itemData.itemName + " is INVALID");

        return true;
    }


    //private ItemDefinition GetRandomItemBySlot(ItemSlot slot, List<ItemDefinition> exclusions) {
    //    List<ItemDefinition> allItemsOfSlot = itemsBySlot[slot];
    //    List<ItemDefinition> filteredList = new List<ItemDefinition>();

    //    for (int i = 0; i < allItemsOfSlot.Count; i++) {
    //        if (exclusions.Contains(allItemsOfSlot[i])) {
    //            continue;
    //        }
    //        filteredList.Add(allItemsOfSlot[i]);
    //    }

    //    if (filteredList.Count == 0)
    //        return null;

    //    int randomIndex = Random.Range(0, filteredList.Count);
    //    return filteredList[randomIndex];
    //}

    //private ItemDefinition GetRandEquipment(List<ItemDefinition> exclusions) {
    //    List<ItemDefinition> allEquipment = itemDict[ItemType.Equipment];
    //    List<ItemDefinition> filteredList = new List<ItemDefinition>();
    //    for (int i = 0; i < allEquipment.Count; i++) {
    //        if (exclusions.Contains(allEquipment[i])) {
    //            continue;
    //        }
    //        filteredList.Add(allEquipment[i]);
    //    }

    //    int randomIndex = Random.Range(0, filteredList.Count);
    //    return filteredList[randomIndex];
    //}



    public List<ItemDefinition> GetRandomSkillsByTag(List<AbilityTag> tags) {
        List<ItemDefinition> results = new List<ItemDefinition> ();
        
        for (int i = 0; i < tags.Count; i++) {
            ItemDefinition skill = GetItem(ItemType.Skill, results, tags[i]);
            results.Add(skill);
        }

        return results;
    }

    public ItemDefinition GetItem(ItemType type, List<ItemDefinition> exclusions, AbilityTag tag = AbilityTag.None, ItemSlot slot = ItemSlot.None) {

        if (type == ItemType.Equipment) {
            if (slot == ItemSlot.None) {
               
                return GetRandomEquipment(itemDict, ItemType.Equipment, exclusions);
                
                //return GetRandEquipment(exclusions);
            }
            else {

                return GetRandomEquipment(itemsBySlot, slot, exclusions);

                //return GetRandomItemBySlot(slot, exclusions);

            }
        }

        if(type == ItemType.Rune) {
            return GetRandomEquipment(itemDict, ItemType.Rune, exclusions);
        }

        if(type == ItemType.Skill) {
            if (tag == AbilityTag.None) {
                return GetRandomEquipment(itemDict, ItemType.Skill, exclusions);
            }
            else {
                return GetRandomEquipment(itemsByTag, tag, exclusions);
            }
        }


        if(type == ItemType.ClassSelection) {

            return GetRandomEquipment(itemDict, ItemType.ClassSelection, exclusions);
        }

        if(type == ItemType.Currency) {
            return itemDict[ItemType.Currency][0];
        }

        if(type == ItemType.SkillPoint) {
            return itemDict[ItemType.SkillPoint][0];
        }

        if (type == ItemType.HealthPotion) {
            return itemDict[ItemType.HealthPotion][0];
        }


        return null;
    }


    public List<AbilityTag> GetItemSkillTags(ItemDefinition item) {

        if (item.itemData.learnableAbilities == null || item.itemData.learnableAbilities.Count < 1)
            return null;

        AbilityDefinition learneableAbility = item.itemData.learnableAbilities[0];


        return learneableAbility.AbilityData.tags;
    }
    

    public void InitDict() {
        itemDict.Clear();

        allTags = System.Enum.GetValues(typeof(AbilityTag)) as AbilityTag[];

        for (int i = 0; i < allTags.Length; i++) {
            if (allTags[i] == AbilityTag.None)
                continue;

            itemsByTag.Add(allTags[i], new List<ItemDefinition>());
        }

        allItems = Resources.LoadAll<ItemDefinition>("");

        for (int i = 0; i < allItems.Length; i++) {
            if (itemDict.TryGetValue(allItems[i].itemData.Type, out List<ItemDefinition> items) == true) {
                itemDict[allItems[i].itemData.Type].AddUnique(allItems[i]);
            }
            else {
                itemDict.Add(allItems[i].itemData.Type, new List<ItemDefinition> { allItems[i] });
            }

            if (allItems[i].itemData.Type == ItemType.Equipment) {
                if (itemsBySlot.TryGetValue(allItems[i].itemData.validSlots[0], out List<ItemDefinition> slottedItems) == true) {
                    itemsBySlot[allItems[i].itemData.validSlots[0]].AddUnique(allItems[i]);
                }
                else {
                    itemsBySlot.Add(allItems[i].itemData.validSlots[0], new List<ItemDefinition> { allItems[i] });
                }
            }

            if (allItems[i].itemData.Type == ItemType.Skill) {
                
                List<AbilityTag> tags = GetItemSkillTags(allItems[i]);

                if(tags != null && tags.Count > 0) {
                    foreach (AbilityTag tag in tags) {
                        itemsByTag[tag].AddUnique(allItems[i]);
                    }
                }
            }

            if (allItems[i].itemData.abilityTags != null && allItems[i].itemData.abilityTags.Count > 0) {
                List<AbilityTag> tags = allItems[i].itemData.abilityTags;
                foreach (AbilityTag tag in tags) {
                    itemsByTag[tag].AddUnique(allItems[i]);
                }
            }

            if (itemsByName.ContainsKey(allItems[i].itemData.itemName) == false) {
                itemsByName.Add(allItems[i].itemData.itemName, allItems[i]);
            }

        }

        statBoosters.Clear();
        CreateBaseStatBoosters();
        CreateStatAffixItems();
    }


    private void CreateStatAffixItems() {

        //Debug.Log("Creating base Stat Affixes: " + itemStatAffixes.Count);

        foreach (ItemStatAffixData affixData in itemStatAffixes) {
           
            ItemData statAffixItem = new ItemData(affixData.stat, affixData.baseValue);
            affixData.baseAffixItem = statAffixItem;
            itemAffixes.Add(affixData.stat, affixData);
            //Debug.Log("Adding: " +  affixData.stat);
        }
    }


    public List<StatName> GetRelavantStatsBySlot(ItemSlot slot) {
        List<StatName> results = new List<StatName>();

        foreach (var item in itemAffixes) {
            if (item.Value.validSlots.Contains(slot)) {
                results.Add(item.Value.stat);
            }
        }


        return results;
    }

    public List<ItemStatAffixData> GetAffixesBySlot(ItemSlot slot) {
        List < ItemStatAffixData> results = new List<ItemStatAffixData >();
        foreach (var item in itemAffixes) {
            if (item.Value.validSlots.Contains(slot)) {
                results.Add(item.Value);
            }
        }

        return results;
    }

    private void CreateBaseStatBoosters() {
        StatName[] allStats = System.Enum.GetValues(typeof(StatName)) as StatName[];

        //List<ItemData> desiredStats = new List<ItemData>();

        foreach (StatName stat in allStats) {
            ItemData data = stat switch {
                StatName.Health => new ItemData(stat, 2f),
                StatName.GlobalMoveSpeedModifier => new ItemData(stat, 0.1f),
                StatName.ShotCount => new ItemData(stat, 0.2f),
                //StatName.Accuracy => throw new System.NotImplementedException(),
                //StatName.DashSpeed => throw new System.NotImplementedException(),
                //StatName.DashDuration => throw new System.NotImplementedException(),
                //StatName.EffectInterval => throw new System.NotImplementedException(),
                StatName.CooldownReduction => new ItemData(stat, 0.05f),
                StatName.GlobalDamageModifier => new ItemData(stat, 0.1f),
                StatName.GlobalEffectDurationModifier => new ItemData(stat, 0.2f),
                StatName.MeleeDamageModifier => new ItemData(stat, 0.15f),
                StatName.OverloadChance => new ItemData(stat, 0.05f),
                StatName.OverloadDamageModifier => new ItemData(stat, 0.2f),
                StatName.ProjectilePierceCount => new ItemData(stat, 0.2f),
                StatName.GlobalEffectIntervalModifier => new ItemData(stat, -0.1f),
                StatName.DashCooldown => new ItemData(stat, 0.1f),
                StatName.ProjectileChainCount => new ItemData(stat, 0.2f),
                StatName.ProjectileSplitCount => new ItemData(stat, 0.2f),
                StatName.ProjectileSplitQuantity => new ItemData(stat, 1f),
                StatName.GlobalEffectSizeModifier => new ItemData(stat, 0.2f),
                //StatName.GlobalEffectRangeModifier => throw new System.NotImplementedException(),
                StatName.GlobalProjectileSizeModifier => new ItemData(stat, 0.2f),
                StatName.Essence => new ItemData(stat, 2f),
                StatName.EssenceRegenerationRate => new ItemData(stat, 1f),
                StatName.EssenceRegenerationValue => new ItemData(stat, 0.1f),
                //StatName.OverloadRecieveChance => new ItemData(stat, 0.1f),
                StatName.CastSpeedModifier => new ItemData(stat, 0.1f),
                StatName.MaxMinionCount => new ItemData(stat, 0.2f),
                StatName.MinionDamageModifier => new ItemData(stat, 0.15f),
                StatName.FireDamageModifier => new ItemData(stat, 0.15f),
                StatName.WaterDamageModifier => new ItemData(stat, 0.15f),
                StatName.AirDamageModifier => new ItemData(stat, 0.15f),
                StatName.ForceDamageModifier => new ItemData(stat, 0.15f),
                StatName.PoisonDamageModifier => new ItemData(stat, 0.15f),
                StatName.ProjectileLifetime => new ItemData(stat, 0.1f),
                _ => null,
            };

            if(data != null) {
                statBoosters.Add(stat, data);
            }
        }



    }



    [System.Serializable]
    public class StatBooserData {
        public StatName stat;
        public ItemData statModItem;
    }

    [System.Serializable]
    public class ItemStatAffixData {
        public StatName stat;
        public float baseValue;
        public float tierIncrament;
        [Header("Valid Slots")]
        public List<ItemSlot> validSlots = new List<ItemSlot>();
        [HideInInspector]
        public ItemData baseAffixItem;

        public float GetTierValue(int tier) {
            
            //if(stat == StatName.EssenceRegenerationRate) {
            //    Debug.LogWarning("Essence Regen Rate: " + baseValue + (tierIncrament * (tier - 1)));
            //}
            
            return baseValue + (tierIncrament * (tier -1));
        }
    }



    [System.Serializable]
    public class ItemIconData {
        public ItemType type;
        public Sprite icon;
    }
}
