using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

public class ItemSpawner : Singleton<ItemSpawner> {

    public List<ItemDefinition> classSelectionItems = new List<ItemDefinition>();

    public List<ItemDefinition> testItems = new List<ItemDefinition>();

    public ItemPickup pickupPrefab;
    public ItemPickup coinPickupPrefab;
    public ItemPickup unstableAetheriumPickup;
    public ItemPickup aetheriumIngotPickup;
    public ItemPickup expPickupPrefab;
    public ItemPickup heathOrbPickup;
    public ItemPickup essenceOrbPickup;
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
            Dev_SpawnResources();
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

        if (target.subtypes.Contains(Entity.EntitySubtype.Minion) && target.subtypes.Contains(Entity.EntitySubtype.Elite) == false)
            return;

        if (target.ownerType == OwnerConstraintType.Enemy && killer.ownerType == OwnerConstraintType.Friendly) {
            int threat = (int)NPCDataManager.GetThreatLevel(target.EntityName);

            SpawnCoins(threat, target.transform.position, threat, threat * 3);

            float expValue = threat / 1.5f;
            //Debug.Log("EXP from: " +  target.EntityName + " : "  + expValue);

            if (expValue > 0f) {
                SpawnEXP(threat, target.transform.position, 1f, Mathf.Min(1f, expValue));
            }

            float unstableAetheriumRoll = Random.Range(0f, 1f);

            if (RoomManager.CurrentRoom != null) {
                if (RoomManager.CurrentRoom.Type != Room.RoomType.BossRoom && unstableAetheriumRoll >= 0.99f)
                    SpawnCoins(Random.Range(1, 3), target.transform.position, 1f, 1f, CurrencyType.UnstableAetherium);

                if (RoomManager.CurrentRoom.Type == Room.RoomType.BossRoom && target.subtypes.Contains(Entity.EntitySubtype.Boss) == true)
                    SpawnCoins(Random.Range(5, 10), target.transform.position, 3f, 3f, CurrencyType.UnstableAetherium);
            }

            if (target.subtypes.Contains(Entity.EntitySubtype.Elite)) {
                SpawnCoins(Random.Range(2, 5), target.transform.position, 1f, 3f, CurrencyType.AethriumIngot);
            }

            if (target.subtypes.Contains(Entity.EntitySubtype.Boss)) {
                SpawnCoins(Random.Range(4, 7), target.transform.position, 1f, 3f, CurrencyType.AethriumIngot);
            }


            CheckForResourceOrbSpawns(target);

        }
    }

    private void CheckForResourceOrbSpawns(Entity target) {
        Entity player = EntityManager.ActivePlayer;

        if (player == null)
            return;

        float healthOrbRoll = Random.Range(0f, 1f);
        if (healthOrbRoll < player.Stats[StatName.HealthOrbChance]) {
            SpawnResourceOrb(1, target.transform.position, StatName.Health, ItemType.HealthOrb);
        }

        float essenceOrbRoll = Random.Range(0f, 1f);
        if (essenceOrbRoll < player.Stats[StatName.EssenceOrbChance]) {
            SpawnResourceOrb(1, target.transform.position, StatName.Essence, ItemType.EssenceOrb);
        }
    }

    #endregion

    public static void SpawnItem(ItemDefinition item, Vector2 location, bool autoPickup = false) {
        SpawnItem(item.itemData, location, autoPickup);
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
        ItemPickup pickUp = Instantiate(Instance.pickupPrefab, location, Quaternion.identity);
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


    public static void SpawnCoins(int count, Vector2 location, float valueMin = 1f, float valueMax = 1f, CurrencyType currencyType = CurrencyType.CrystalizedAetherium) {

        ItemPickup prefab = currencyType switch {
            CurrencyType.CrystalizedAetherium => Instance.coinPickupPrefab,
            CurrencyType.AethriumIngot => Instance.aetheriumIngotPickup,
            CurrencyType.UnstableAetherium => Instance.unstableAetheriumPickup,
            _ => null,
        };

        for (int i = 0; i < count; i++) {
            ItemData coinData = new ItemData();

            int valueRange = Random.Range((int)valueMin, (int)(valueMax + 1));

            coinData.itemValue = valueRange;
            coinData.itemName = currencyType.ToString();
            coinData.Type = ItemType.Currency;
            coinData.pickupOnCollision = true;

            ItemPickup pickup = Instantiate(prefab, location, Quaternion.identity);
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

    //public static void SpawnHealthOrbs(int count, Vector2 location) {

    //    if (EntityManager.ActivePlayer == null)
    //        return;

    //    float maxHealth = EntityManager.ActivePlayer.Stats.GetStatRangeMaxValue(StatName.Health);

    //    for (int i = 0; i < count; i++) {
    //        ItemData orbItemData = new ItemData();

    //        float valueRange = Random.Range(maxHealth * 0.1f, maxHealth * 0.25f);

    //        orbItemData.itemValue = valueRange;
    //        orbItemData.itemName = "HealthOrb";
    //        orbItemData.Type = ItemType.HealthOrb;
    //        orbItemData.pickupOnCollision = true;

    //        ItemPickup pickup = Instantiate(Instance.heathOrbPickup, location, Quaternion.identity);
    //        pickup.Setup(orbItemData);
    //    }
    //}

    public static void SpawnResourceOrb(int count, Vector2 location, StatName stat, ItemType orbType) {
        if (EntityManager.ActivePlayer == null)
            return;

        float maxResource = EntityManager.ActivePlayer.Stats.GetStatRangeMaxValue(stat);

        StatName orbValueStat = stat switch {
            StatName.Essence => StatName.EssenceOrbValue,
            StatName.Health => StatName.HealthOrbValue,
            _ => StatName.Vitality,

        };

        ItemPickup orbPrefab = orbType switch {
            ItemType.HealthOrb => Instance.heathOrbPickup,
            ItemType.EssenceOrb => Instance.essenceOrbPickup,
            _ => null,
        };

        float orbValue = EntityManager.ActivePlayer.Stats[orbValueStat];

        for (int i = 0; i < count; i++) {
            ItemData orbItemData = new ItemData();

            orbItemData.itemValue = orbValue * maxResource;
            orbItemData.itemName = orbType.ToString();
            orbItemData.Type = orbType;
            orbItemData.pickupOnCollision = true;

            ItemPickup pickup = Instantiate(orbPrefab, location, Quaternion.identity);
            pickup.Setup(orbItemData);
        }

    }


    private static void FilterExistingStats(ref List<StatName> usedStats, Item currentItem, ItemAffixSlotEntry selectedSlot) {
        if (selectedSlot == null) {
            foreach (var affix in currentItem.Affixes.Keys) {
                usedStats.Add(affix.affixStatTarget);
            }
        }
        else {
            List<ItemAffixSlotEntry> otherSlots = PanelManager.GetPanel<InventoryPanel>().GetOtherSlots(selectedSlot);

            for (int i = 0; i < otherSlots.Count; i++) {
                if (otherSlots[i].AffixData != null) {
                    usedStats.Add(otherSlots[i].AffixData.affixStatTarget);
                }
            }
        }
    }

    public static List<ItemData> CreateItemAffixSet(int count, ItemSlot itemSlot, Item currentItem, ItemAffixSlotEntry selectedSlot = null) {
        List<StatName> usedStats = new List<StatName>();

        FilterExistingStats(ref usedStats, currentItem, selectedSlot);
        Dictionary<LootDatabase.ItemStatAffixData, int> affixDict = new Dictionary<LootDatabase.ItemStatAffixData, int>();

        //List<LootDatabase.ItemStatAffixData> affixList = new List<LootDatabase.ItemStatAffixData>();
        List<StatName> allStats = Instance.lootDatabase.GetRelavantStatsBySlot(itemSlot); //Instance.lootDatabase.itemAffixes.Keys.ToList();
        Instance.FilterStats(allStats, ref usedStats);
        allStats.Shuffle();

        for (int i = 0; i < allStats.Count; i++) {
            if (usedStats.Contains(allStats[i]))
                continue;


            int potentialTier = Instance.RollAffixTier(1);
            ItemData existingAffix = currentItem.GetAffixByStat(allStats[i]);

            if (existingAffix != null && existingAffix.tier > potentialTier) {
                usedStats.Add(allStats[i]);
                continue;
            }

            affixDict.Add(Instance.lootDatabase.itemAffixes[allStats[i]], potentialTier);

            //affixList.Add(Instance.lootDatabase.itemAffixes[allStats[i]]);
            usedStats.Add(allStats[i]);


            if (affixDict.Count >= count) {
                break;
            }

        }

        List<ItemData> results = new List<ItemData>();

        foreach (var potentialAffix in affixDict) {
            ItemData affixData = Instance.CreateItemAffix(potentialAffix.Key, potentialAffix.Value);
            results.Add(affixData);
        }


        //for (int i = 0; i < affixList.Count; i++) {
        //    results.Add(Instance.RollAffixTier(affixList[i], 1));
        //}


        if (results.Count < 1) {
            Debug.LogError("No Affixes found for: " + itemSlot);
        }


        return results;
    }

    private List<StatName> GetRelevantStatsBySlot(ItemSlot slot) {
        List<StatName> results = new List<StatName>();



        return results;
    }


    public ItemData UpgradeItemAffixTier(ItemData affixData) {
        if (affixData.tier < 5) {
            return new ItemData(affixData, affixData.tier + 1);
        }
        else {
            Debug.LogError("Max tier for: " + affixData.GetAffixTooltip());
            return null;
        }
    }


    private int RollAffixTier(int currentTier) {
        float roll = Random.Range(0f, 1f);

        float chance = currentTier switch {
            1 => 0.3f,
            2 => 0.3f,
            3 => 0.3f,
            4 => 0.3f,
            5 => 0.3f,
            _ => 0.2f,
        };

        if (currentTier < 5) {
            if (roll < chance) {
                return RollAffixTier(currentTier + 1);
            }
            else {
                return currentTier;
            }
        }

        return currentTier;

    }


    private ItemData CreateItemAffix(LootDatabase.ItemStatAffixData data, int tier) {
        return new ItemData(data.stat, data.GetTierValue(tier), tier);
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

        if (currentTier < 5) {
            if (roll < chance) {
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


        for (int i = 0; i < allStats.Count; i++) {
            if (usedStats.Contains(allStats[i]))
                continue;

            results.Add(Instance.lootDatabase.statBoosters[allStats[i]]);
            usedStats.Add(allStats[i]);


            if (results.Count >= count) {
                break;
            }

        }

        return results;
    }

    public bool IsStatRelevant(StatName stat) {

        bool result = stat switch {
            StatName.Health => true,
            StatName.GlobalMoveSpeedModifier => true,
            StatName.ShotCount => true,
            StatName.CooldownReduction => true,
            StatName.GlobalDamageModifier => true,
            //StatName.GlobalEffectDurationModifier => true,
            StatName.MeleeDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Melee),
            StatName.OverloadChance => true,
            StatName.OverloadDamageModifier => true,
            StatName.ProjectilePierceCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            //StatName.GlobalEffectIntervalModifier => true,
            StatName.DashCooldown => false,
            StatName.ProjectileChainCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.ProjectileSplitCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.ProjectileSplitQuantity => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.GlobalEffectSizeModifier => true,
            StatName.GlobalProjectileSizeModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.Essence => true,
            StatName.EssenceRegenerationRate => true,
            StatName.EssenceRegenerationValue => false,
            StatName.HealthRegenerationRate => false,
            StatName.HealthRegenerationValue => true,
            //StatName.OverloadRecieveChance => new ItemData(stat, 0.1f),
            StatName.MaxMinionCount => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Summoning),
            StatName.MinionDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Summoning),
            StatName.FireDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Fire),
            StatName.WaterDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Water),
            StatName.AirDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Air),
            StatName.ForceDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Force),
            StatName.PoisonDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Poison),
            //StatName.ProjectileLifetime => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),
            StatName.ArcaneDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Arcane),
            StatName.TimeDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Time),
            StatName.VoidDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Void),
            StatName.SpatialDamageModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Space),
            //StatName.GlobalStatusDurationModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Affliction),
            //StatName.GlobalStatusIntervalModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Affliction),
            StatName.GlobalComboDurationModifier => true,
            StatName.GlobalComboIntervalModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.DamageOverTime),
            StatName.CastingMoveSpeedModifier => true,
            StatName.CastSpeedModifier => false,
            StatName.GlobalProjectileLifetimeModifier => EntityManager.ActivePlayer.HasAbilityOfTag(AbilityTag.Projectile),

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


    public void Dev_SpawnResources() {
        SpawnEXP(25, EntityManager.ActivePlayer.transform.position);
        SpawnCoins(25, EntityManager.ActivePlayer.transform.position, 10f, 10f);
        SpawnCoins(5, EntityManager.ActivePlayer.transform.position, 10f, 10f, CurrencyType.UnstableAetherium);
        SpawnCoins(5, EntityManager.ActivePlayer.transform.position, 10f, 10f, CurrencyType.AethriumIngot);
    }

}
