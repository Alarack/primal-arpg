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
    public ItemPickup expPickupPrefab;
    public ItemPickup heathOrbPickup;
    public LootDatabase lootDatabase;
    public ItemAffixDatabase itemAffixDatabase;

    public static Vector2 defaultSpawnLocation = Vector2.zero;


    [RuntimeInitializeOnLoadMethod]
    private static void InitStatic() {
        defaultSpawnLocation = Vector2.zero;
    }

    private void Start() {

        for (int i = 0; i < Instance.testItems.Count; i++) {
            ItemPickup testPickup = Instantiate(pickupPrefab, Vector2.zero, Quaternion.identity);
            testPickup.Setup(Instance.testItems[i].itemData);
        }


        lootDatabase.InitDict();

    }

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E)) {
            SpawnEXP(25, EntityManager.ActivePlayer.transform.position);
            SpawnCoins(25, EntityManager.ActivePlayer.transform.position, 5f, 10f);
        }
#endif
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
            int threat = (int)NPCDataManager.GetThreatLevel(target.EntityName);

            SpawnCoins(threat, target.transform.position, threat, threat * 3);

            float expValue = threat / 1.5f;
            //Debug.Log("EXP from: " +  target.EntityName + " : "  + expValue);

            if(expValue > 0f) {
                SpawnEXP(threat, target.transform.position, 1f, Mathf.Min(1f, expValue));
            }


            Entity player = EntityManager.ActivePlayer;

            if(player != null && player.Stats.GetStatRangeRatio(StatName.Health) < 1f) {
                float roll = Random.Range(0f, 1f);
                if(roll < 0.15f) {
                    SpawnHealthOrbs(1, target.transform.position);
                }
            }


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

    public static void SpawnEXP(int count, Vector2 location, float valueMin = 1f, float valueMax = 1f) {
        for (int i = 0; i < count; i++) {
            ItemData expDataItem = new ItemData();

            int valueRange = Random.Range((int)valueMin, (int)(valueMax + 1));

            expDataItem.itemValue = valueRange;
            expDataItem.itemName = "EXP";
            expDataItem.Type = ItemType.Experience;
            expDataItem.pickupOnCollision = true;

            ItemPickup pickup = Instantiate(Instance.expPickupPrefab, location, Quaternion.identity);
            pickup.Setup(expDataItem);
        }
    }

    public static void SpawnHealthOrbs(int count, Vector2 location) {

        if (EntityManager.ActivePlayer == null)
            return;

        float maxHealth = EntityManager.ActivePlayer.Stats.GetStatRangeMaxValue(StatName.Health);
        
        for (int i = 0; i < count; i++) {
            ItemData expDataItem = new ItemData();

            float valueRange = Random.Range(maxHealth * 0.1f, maxHealth * 0.25f);

            expDataItem.itemValue = valueRange;
            expDataItem.itemName = "EXP";
            expDataItem.Type = ItemType.HealthOrb;
            expDataItem.pickupOnCollision = true;

            ItemPickup pickup = Instantiate(Instance.heathOrbPickup, location, Quaternion.identity);
            pickup.Setup(expDataItem);
        }
    }


    public static void CreateStatBooster(StatName stat) {

    }


    public static List<ItemData> CreateItemAffixSet(int count) {
        List<StatName> usedStats = new List<StatName>();
        //List<ItemData> baseAffixItems = new List<ItemData>();

        List<LootDatabase.ItemStatAffixData> affixList = new List<LootDatabase.ItemStatAffixData>();
        List<StatName> allStats = Instance.lootDatabase.itemAffixes.Keys.ToList();
        Instance.FilterStats(allStats, ref usedStats);
        allStats.Shuffle();

        for (int i = 0; i < allStats.Count; i++) {
            if (usedStats.Contains(allStats[i]))
                continue;

            //baseAffixItems.Add(Instance.lootDatabase.itemAffixes[allStats[i]].baseAffixItem);
            affixList.Add(Instance.lootDatabase.itemAffixes[allStats[i]]);
            usedStats.Add(allStats[i]);


            if (affixList.Count >= count) {
                break;
            }

        }

        List<ItemData> results = new List<ItemData>();

        for (int i = 0; i < affixList.Count; i++) {
            results.Add(Instance.RollAffixTier(affixList[i], 1));
        }



        return results;
    }

    private ItemData RollAffixTier(LootDatabase.ItemStatAffixData data, int currentTier) {
        float roll = Random.Range(0f, 1f);

        float chance = currentTier switch {
            1 => 0.3f,
            2 => 0.3f,
            3 => 0.3f,
            4 => 0.3f,
            5 => 0.3f,
            _ => 0.2f,
        };

        if(currentTier < 5) {
            if(roll < chance) {
                return RollAffixTier(data, currentTier + 1);
            }
            else {
                return new ItemData(data.stat, data.GetTierValue(currentTier), currentTier);
            }
        }

        return new ItemData(data.stat, data.GetTierValue(currentTier), currentTier);
    }

    public static List<ItemData> CreateStatBoosterSet(int count) {
        List<StatName> usedStats = new List<StatName>();

        List<ItemData> results = new List<ItemData>();

        List<StatName> allStats = Instance.lootDatabase.statBoosters.Keys.ToList();

        Instance.FilterStats(allStats, ref usedStats);


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




    private bool IsStatRelevant(StatName stat) {

        bool result = stat switch {
            StatName.Health => true,
            StatName.GlobalMoveSpeedModifier => true,
            StatName.ShotCount => true,
            StatName.CooldownReduction => true,
            StatName.GlobalDamageModifier => true,
            StatName.GlobalEffectDurationModifier => true,
            StatName.MeleeDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Melee),
            StatName.OverloadChance => true,
            StatName.OverloadDamageModifier => true,
            StatName.ProjectilePierceCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.GlobalEffectIntervalModifier => true,
            StatName.DashCooldown => true,
            StatName.ProjectileChainCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.ProjectileSplitCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.ProjectileSplitQuantity => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.GlobalEffectSizeModifier => true,
            StatName.GlobalProjectileSizeModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.Essence => true,
            StatName.EssenceRegenerationRate => true,
            StatName.EssenceRegenerationValue => true,
            //StatName.OverloadRecieveChance => new ItemData(stat, 0.1f),
            StatName.CastSpeedModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.CastTime),
            StatName.MaxMinionCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Summoning),
            StatName.MinionDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Summoning),
            StatName.FireDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Fire),
            StatName.WaterDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Water),
            StatName.AirDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Air),
            StatName.ForceDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Force),
            StatName.PoisonDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Poison),
            StatName.ProjectileLifetime => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.ArcaneDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Arcane),
            StatName.TimeDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Time),
            StatName.VoidDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Void),
            StatName.SpatialDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Space),
            _ => false,
        };

        //Debug.Log(stat + " is relevant: " + result);

        return result;
    }

    private void FilterStats(List<StatName> allStats, ref List<StatName> usedStats) {

        for (int i = 0; i < allStats.Count; i++) {
            if (IsStatRelevant(allStats[i]) == false) {
                usedStats.Add(allStats[i]);
            }
        }
    }


}
