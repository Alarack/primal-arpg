using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemFactory 
{


    public static Item CreateItem(ItemData itemData, Entity owner, bool unstable = false) {

        Item result;
        if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
            result = new ItemWeapon(itemData, owner, true, unstable);
        }
        else {
            result = new Item(itemData, owner, true, unstable);

        }


        return result;

    }

    public static Item CreateItem(ItemDefinition itemDefinition, Entity owner) {
        return CreateItem(itemDefinition.itemData, owner);
    }





}
