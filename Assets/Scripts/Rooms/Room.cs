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
        RoomManager.SpawnRoomPortals();
    }

    public virtual void SpawnRewards() {

        if(rewards.Count < 1) {
            EndRoom();
            return;
        }

        RoomManager.CreateRewards(GetAllItemRewards());
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
                Debug.Log(item.itemData.itemName + " has been added");
            }
        }

        RoomReward reward = new RoomReward();
        reward.itemCategory = type;
        reward.items = results;

        rewards.Add(reward);

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

        public RewardCategory category;
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
        

        SpawnRewards();
    }

}

public class EliminitionCombatRoom : Room {


    public override RoomType Type => RoomType.EliminationCombat;

    public List<EntityManager.Wave> waves = new List<EntityManager.Wave>();
    private int waveIndex;

    public EliminitionCombatRoom() : base() {
        //waves = EntityManager.GenerateWaves()
        waves = EntityManager.GenerateWaves(3, RoomManager.CurrentBiome, RoomManager.CurrentDifficulty, RoomManager.CurrentDifficulty / 5, RoomManager.CurrentDifficulty);
    }

    public override void StartRoom() {
        Debug.LogWarning("Wave Starting: " + waveIndex + 1);

        SpawnWave();
    }

    public override void EndRoom() {
        base.EndRoom();

        
    }


    public void SpawnWave() {

        if (waves.Count < 1) {
            Debug.LogError("No waves in entity manager");
            return;
        }

        if (waveIndex >= waves.Count) {
            Debug.LogWarning("All waves Complete");
            SpawnRewards();
            return;
        }

        new Task(waves[waveIndex].SpawnWaveOnDelay());

        waveIndex++;
    }

}



