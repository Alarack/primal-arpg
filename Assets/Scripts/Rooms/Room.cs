using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Room {
    public enum RoomType {
        StartRoom,
        EliminationCombat,
        ItemShop,
        SkillShop,
        RecoveryRoom,
        SurvivalCombat,
        BossRoom,
        SecretRoom,
        TreasureRoom,
        MiniBossRoom,
        EventRoom
    }

    public RoomData Data { get; protected set; }

    public string roomBiome;

    public List<RoomReward> rewards = new List<RoomReward>();

    public abstract RoomType Type { get; }

    public Room() {

    }

    public Room(RoomData data) {
        Data = data;
    }

    public abstract void StartRoom();

    public virtual void EndRoom() {
        RoomManager.OnRoomEnded(this);
        RoomManager.SpawnRoomPortals();
    }

    public virtual void OnAllEnemiesKilled() {

    }

    public virtual void SpawnRewards(string displayText, bool multiReward = false, bool shopMode = false) {

        if(rewards.Count < 1) {
            EndRoom();
            return;
        }

        RoomManager.CreateRewards(GetAllItemRewards(), displayText, multiReward, shopMode);
    }


    protected List<ItemDefinition> GetAllItemRewards() {
        List<ItemDefinition> results = new List<ItemDefinition>();
        
        for (int i = 0; i < rewards.Count; i++) {
            results.AddRange(rewards[i].items);
        }

        return results;
    }

    protected void GenerateRewards(int count, ItemType type, AbilityTag tag = AbilityTag.None, ItemSlot slot = ItemSlot.None) {


        List<ItemDefinition> results = new List<ItemDefinition>();

        for (int i = 0; i < count; i++) {
            ItemDefinition item = ItemSpawner.Instance.lootDatabase.GetItem(type, results, tag, slot);

            if (item != null) {
                results.Add(item);
                //Debug.Log(item.itemData.itemName + " has been added");
            }
        }

        RoomReward reward = new RoomReward();
        reward.itemCategory = type;
        reward.items = results;

        SetRewardDescriptons(reward, type, tag, slot);

        rewards.Add(reward);

    }

    private void SetRewardDescriptons(RoomReward reward, ItemType type, AbilityTag tag, ItemSlot slot) {

        string result = type switch {
            ItemType.None => throw new System.NotImplementedException(),
            ItemType.Equipment when slot != ItemSlot.None => slot.ToString().Replace("1", ""), //SetEquipmentRewardDescriptions(reward, slot),
            ItemType.Equipment when slot == ItemSlot.None => "Random Equipment",
            ItemType.Rune => "Skill Rune",
            ItemType.Currency => "Gold",
            ItemType.Skill when tag != AbilityTag.None => tag + " Skill", //SetSkillRewardDescription(reward, tag),
            ItemType.Skill when tag == AbilityTag.None => "Random Skill",
            ItemType.ClassSelection => "",
            _ => throw new System.NotImplementedException(),
        };

        reward.rewardDescription = result;
    }

    private string SetSkillRewardDescription(RoomReward reward, AbilityTag tag) {

        return tag + " Skill";

    }

    private string SetEquipmentRewardDescriptions(RoomReward reward, ItemSlot slot) {

        return slot.ToString().Replace("1", "");
    }




    [System.Serializable]
    public class RoomReward {
        
        public enum RewardCategory {
            ClassSkill,
            ElementalSkill,
            PassiveSkill,
            Weapon,
            Equipment,
            SkillRune,
            Currency,
            ClassSelection
        }

        public RewardCategory rewardCategory;
        public string rewardDescription;
        public ItemType itemCategory;
        
        public List<ItemDefinition> items = new List<ItemDefinition>();
    }

}


public class StartingRoom : Room {
    public override RoomType Type => RoomType.StartRoom;

    public StartingRoom() {

        GenerateRewards(3, ItemType.ClassSelection);

    }

    public override void StartRoom() {
        

        SpawnRewards("Choose a Class");
    }

}

public class EliminitionCombatRoom : Room {


    public override RoomType Type => RoomType.EliminationCombat;

    public List<EntityManager.Wave> waves = new List<EntityManager.Wave>();
    private int waveIndex;

    public EliminitionCombatRoom(ItemType rewardType, AbilityTag rewardTag, ItemSlot rewardSlot) : base() {
        //waves = EntityManager.GenerateWaves()

        //Debug.Log("Creating a combat room");

        GenerateRewards(3, rewardType, rewardTag, rewardSlot);

        waves = EntityManager.GenerateWaves(3, RoomManager.CurrentBiome, RoomManager.CurrentDifficulty, RoomManager.CurrentDifficulty / 5, RoomManager.CurrentDifficulty);
    }

    public override void StartRoom() {
        //Debug.LogWarning("Wave Starting: " + (waveIndex + 1));

        SpawnWave();
    }

    public override void EndRoom() {
        base.EndRoom();

        
    }

    public override void OnAllEnemiesKilled() {
        base.OnAllEnemiesKilled();

        SpawnWave();
    }


    public void SpawnWave() {

        if (waves.Count < 1) {
            Debug.LogError("No waves in Room");
            return;
        }

        if (waveIndex >= waves.Count) {
            //Debug.LogWarning("All waves Complete");
            SpawnRewards("Choose a Reward");
            return;
        }

        new Task(waves[waveIndex].SpawnWaveOnDelay());

        waveIndex++;
    }

}

public class BossRoom : Room {
    public override RoomType Type =>RoomType.BossRoom;

    EntityManager.Wave bossWave;

    public BossRoom(ItemType rewardType, AbilityTag rewardTag, ItemSlot rewardSlot) : base() {
        
        GenerateRewards(2, rewardType, rewardTag, rewardSlot);
        
        bossWave = EntityManager.GenerateBossWave(RoomManager.CurrentBiome);
    
    }

    public override void StartRoom() {
        new Task(bossWave.SpawnWaveOnDelay());
    }


    public override void OnAllEnemiesKilled() {
        base.OnAllEnemiesKilled();

        SpawnRewards("Congratulations!", true);
    }
}

public class ShopRoom : Room {
    public override RoomType Type => RoomType.ItemShop;

    public ShopRoom(ItemType rewardType, AbilityTag rewardTag, ItemSlot rewardSlot) : base() {
        GenerateRewards(5, rewardType, rewardTag, rewardSlot);
    }


    public override void StartRoom() {
        SpawnRewards("Shop!", true, true);

        List<Vector2> portalLocations = TargetHelper.GetUpperCenterRow(2);

        RoomManager.SpawnRoomPortals(2, portalLocations);
    }

    public override void EndRoom() {
        RoomManager.OnRoomEnded(this);
    }
}



