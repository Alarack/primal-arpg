using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Def", menuName = "Items/Item Def")]
public class ItemDefinition : ScriptableObject
{

    public ItemData itemData;

    private Item displayItem;

    public Item GetItemForDisplay(Entity owner) {


        displayItem = ItemFactory.CreateItem(itemData);

        //if(displayItem == null) {
        //    if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
        //        displayItem = new ItemWeapon(itemData, owner);
        //    }
        //    else {
        //        displayItem = new Item(itemData, owner);
        //    }
        //}

        return displayItem;
    }

}
