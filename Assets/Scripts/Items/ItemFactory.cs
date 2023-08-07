using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemFactory 
{


    public static Item CreateItem(ItemData itemData, Entity owner) {

        Item result;
        if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
            result = new ItemWeapon(itemData, owner, true);
        }
        else {
            result = new Item(itemData, owner, true);

        }


        return result;

    }

    public static Item CreateItem(ItemDefinition itemDefinition, Entity owner) {
        return CreateItem(itemDefinition.itemData, owner);
    }





}
