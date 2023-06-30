using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : Singleton<ItemSpawner>
{

    public ItemDefinition testItem;
    public ItemPickup pickupPrefab;


    private void Start() {



        ItemPickup testPickup = Instantiate(pickupPrefab, Vector2.zero, Quaternion.identity);
        testPickup.Setup(testItem.itemData);
    }

}
