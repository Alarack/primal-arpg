using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemFactory 
{


    public static Item CreateItem(ItemData itemData) {

        Item result;
        if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
            result = new ItemWeapon(itemData, null);
        }
        else {
            result = new Item(itemData, null);

        }


        return result;

    }

    public static Item CreateItem(ItemDefinition itemDefinition) {
        return CreateItem(itemDefinition.itemData);
    }





}
