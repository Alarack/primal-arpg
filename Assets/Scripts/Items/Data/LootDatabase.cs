using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Loot Database", fileName = "Loot Database")]
public class LootDatabase : ScriptableObject {

    public Dictionary<ItemType, List<ItemDefinition>> itemDict = new Dictionary<ItemType, List<ItemDefinition>>();

    public Dictionary<ItemSlot, List<ItemDefinition>> itemsBySlot = new Dictionary<ItemSlot, List<ItemDefinition>>();

    public Dictionary<AbilityTag, List<ItemDefinition>> itemsByTag = new Dictionary<AbilityTag, List<ItemDefinition>>();

    public ItemDefinition[] allItems;


    private AbilityTag[] allTags; 




    private ItemDefinition GetRandomEquipment<TKey>(Dictionary<TKey, List<ItemDefinition>> dict, TKey key, List<ItemDefinition> exclusions) where TKey : struct {
        List<ItemDefinition> allPossibleItems = dict[key];
        List<ItemDefinition> filteredList = new List<ItemDefinition>();

        allPossibleItems.Shuffle();

        for (int i = 0; i < allPossibleItems.Count; i++) {
            if (exclusions.Contains(allPossibleItems[i])) {
                //Debug.Log(allPossibleItems[i].itemData.itemName + " is a dupe");
                continue;
            }

            if (GameManager.AllowDuiplicatSkills == false ) {
                if(allPossibleItems[i].itemData.Type == ItemType.Skill) {

                    Debug.LogWarning("Checking if " + allPossibleItems[i].itemData.itemName + " is a skill");

                    if(allItems[i].itemData.learnableAbilities.Count > 0) {
                        if (EntityManager.ActivePlayer.AbilityManager.HasAbility(allItems[i].itemData.learnableAbilities[0]) == true)
                            continue;
                    }

                  
                }
            }

            filteredList.Add(allPossibleItems[i]);
        }

        if (filteredList.Count == 0)
            return null;

        int randomIndex = Random.Range(0, filteredList.Count);
        return filteredList[randomIndex];

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
                itemDict[allItems[i].itemData.Type].Add(allItems[i]);
            }
            else {
                itemDict.Add(allItems[i].itemData.Type, new List<ItemDefinition> { allItems[i] });
            }

            if (allItems[i].itemData.Type == ItemType.Equipment) {
                if (itemsBySlot.TryGetValue(allItems[i].itemData.validSlots[0], out List<ItemDefinition> slottedItems) == true) {
                    itemsBySlot[allItems[i].itemData.validSlots[0]].Add(allItems[i]);
                }
                else {
                    itemsBySlot.Add(allItems[i].itemData.validSlots[0], new List<ItemDefinition> { allItems[i] });
                }
            }

            if (allItems[i].itemData.Type == ItemType.Skill) {
                
                List<AbilityTag> tags = GetItemSkillTags(allItems[i]);

                if(tags != null && tags.Count > 0) {
                    foreach (AbilityTag tag in tags) {
                        itemsByTag[tag].Add(allItems[i]);
                    }
                }
            }

        }
    }


    [System.Serializable]
    public class LootDataEntry {
        public ItemType Type;
    }

}
