using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room {
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
    }

    public RoomData Data { get; protected set; }

    public string roomBiome;

    public List<RoomReward> rewards = new List<RoomReward>();

    public Room() {

    }

    public Room(RoomData data) {
        Data = data;
    }

    public virtual void StartRoom() {

    }

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


    [System.Serializable]
    public class RoomReward {
        public List<ItemDefinition> items = new List<ItemDefinition>();
    }

}

public class EliminitionCombatRoom : Room {


    public RoomType type => RoomType.EliminationCombat;

    public List<EntityManager.Wave> waves = new List<EntityManager.Wave>();
    private int waveIndex;

    public EliminitionCombatRoom() : base() {
        //waves = EntityManager.GenerateWaves()
        waves = EntityManager.GenerateWaves(3, RoomManager.CurrentBiome, RoomManager.CurrentDifficulty, RoomManager.CurrentDifficulty / 5, RoomManager.CurrentDifficulty);
    }

    public override void StartRoom() {
        base.StartRoom();

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



