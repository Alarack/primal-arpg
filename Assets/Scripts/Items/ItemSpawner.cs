using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : Singleton<ItemSpawner>
{


    public List<ItemDefinition> testItems = new List<ItemDefinition>();

    public ItemPickup pickupPrefab;

    public static Vector2 defaultSpawnLocation = Vector2.zero;

    private void Start() {

        for (int i = 0; i < Instance.testItems.Count; i++) {
            ItemPickup testPickup = Instantiate(pickupPrefab, Vector2.zero, Quaternion.identity);
            testPickup.Setup(Instance.testItems[i].itemData);
        }

      
    }


    public static void SpawnItem(ItemDefinition item, Vector2 location) {
        ItemPickup testPickup = Instantiate(Instance.pickupPrefab,location, Quaternion.identity);
        testPickup.Setup(item.itemData);
    }

    public static void SpawnItem(Item item, Vector2 location) {
        ItemPickup pickUp = Instantiate(Instance.pickupPrefab, location, Quaternion.identity) ;
        pickUp.Setup(item);
    }

    public static void SpawnItem(ItemData itemData, Vector2 location) {
        Item newItem = null;


        if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
            newItem = new ItemWeapon(itemData, null);
        }
        else {
            newItem = new Item(itemData, null);

        }


        SpawnItem(newItem, location);
    }

}
