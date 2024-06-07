using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Def", menuName = "Items/Item Def")]
public class ItemDefinition : ScriptableObject
{
    public bool devItem;
    public bool startingItem;
    public ItemData itemData;

    private Item displayItem;

    public Item GetItemForDisplay(Entity owner) {

        if(displayItem == null) {
            displayItem = itemData.GetDisplayItem();
            
        }

        return displayItem;
    }

}
