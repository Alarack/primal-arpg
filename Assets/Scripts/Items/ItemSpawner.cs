using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using System.Linq;

public class ItemSpawner : Singleton<ItemSpawner>
{

    public List<ItemDefinition> classSelectionItems = new List<ItemDefinition>();

    public List<ItemDefinition> testItems = new List<ItemDefinition>();

    public ItemPickup pickupPrefab;
    public ItemPickup coinPickupPrefab;
    public LootDatabase lootDatabase;

    public static Vector2 defaultSpawnLocation = Vector2.zero;

    private void Start() {

        for (int i = 0; i < Instance.testItems.Count; i++) {
            ItemPickup testPickup = Instantiate(pickupPrefab, Vector2.zero, Quaternion.identity);
            testPickup.Setup(Instance.testItems[i].itemData);
        }


        lootDatabase.InitDict();

    }

    private void OnEnable() {
        EventManager.RegisterListener(GameEvent.UnitDied, OnUnitDied);
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }


    #region EVENTS

    public void OnUnitDied(EventData data) {
        Entity target = data.GetEntity("Victim");
        Entity killer = data.GetEntity("Killer");

        if (target.ownerType == OwnerConstraintType.Enemy && killer.ownerType == OwnerConstraintType.Friendly) {
            int threat = (int)NPCDataManager.GetThreatLevel(target.entityName);

            SpawnCoins(threat, target.transform.position, threat, threat * 3); 
        }
    }

    #endregion

    public static void SpawnItem(ItemDefinition item, Vector2 location, bool autoPickup = false) {
        
        SpawnItem(item.itemData, location, autoPickup);

        //if(autoPickup == false) {
        //    ItemPickup testPickup = Instantiate(Instance.pickupPrefab, location, Quaternion.identity);
        //    testPickup.Setup(item.itemData);
        //}
        //else {
        //    EntityManager.ActivePlayer.Inventory.Add(ItemFactory.CreateItem(item, EntityManager.ActivePlayer));
        //}

    }

    public static void SpawnItem(ItemData item, Vector2 location, bool autoPickup = false) {
        if (autoPickup == false) {
            ItemPickup testPickup = Instantiate(Instance.pickupPrefab, location, Quaternion.identity);
            testPickup.Setup(item);
        }
        else {
            EntityManager.ActivePlayer.Inventory.Add(ItemFactory.CreateItem(item, EntityManager.ActivePlayer));
        }
    }

    public static void SpawnItem(Item item, Vector2 location) {
        ItemPickup pickUp = Instantiate(Instance.pickupPrefab, location, Quaternion.identity) ;
        pickUp.Setup(item);
    }

    public static void SpawnItem(ItemData itemData, Vector2 location, Entity owner) {
        Item newItem = ItemFactory.CreateItem(itemData, owner);


        //if (itemData.validSlots.Contains(ItemSlot.Weapon)) {
        //    newItem = new ItemWeapon(itemData, null);
        //}
        //else {
        //    newItem = new Item(itemData, null);

        //}


        SpawnItem(newItem, location);
    }

    public static void SpawnCoins(int count, Vector2 location, float valueMin = 1f, float valueMax = 1f) {

        for (int i = 0; i < count; i++) {
            ItemData coinData = new ItemData();

            int valueRange = Random.Range((int)valueMin, (int)(valueMax + 1));
            
            coinData.itemValue = valueRange;
            coinData.itemName = "Coin";
            coinData.Type = ItemType.Currency;
            coinData.pickupOnCollision = true;

            ItemPickup pickup = Instantiate(Instance.coinPickupPrefab, location, Quaternion.identity);
            pickup.Setup(coinData);
        }
    }


    public static void CreateStatBooster(StatName stat) {

    }

    public static List<ItemData> CreateStatBoosterSet(int count) {
        List<StatName> usedStats = new List<StatName>();

        List<ItemData> results = new List<ItemData>();

        List<StatName> allStats = Instance.lootDatabase.statBoosters.Keys.ToList();

        allStats.Shuffle();


        for(int i = 0; i < allStats.Count; i++) {
            if (usedStats.Contains(allStats[i]))
                continue;

            results.Add(Instance.lootDatabase.statBoosters[allStats[i]]);
            usedStats.Add(allStats[i]);


            if(results.Count >= count) {
                break;
            }

        }

        return results;
    }


}
