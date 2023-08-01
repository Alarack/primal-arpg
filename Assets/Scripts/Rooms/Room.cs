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


    [System.Serializable]
    public class RoomReward {
        public List<ItemDefinition> items = new List<ItemDefinition>();
    }

}

public class EliminitionCombatRoom : Room {

    public List<EntityManager.Wave> waves = new List<EntityManager.Wave>();
    private int waveIndex;



    public override void StartRoom() {
        base.StartRoom();

        Debug.LogWarning("Wave Starting: " + waveIndex + 1);

        new Task(SpawnWave());
    }

    public override void EndRoom() {
        base.EndRoom();

        Debug.Log("Create Rewards for this combat elimination room");
    }


    public IEnumerator SpawnWave() {

        if (waves.Count < 1) {
            Debug.LogError("No waves in entity manager");
            yield break;
        }

        if (waveIndex >= waves.Count) {
            Debug.LogWarning("All waves Complete");
            EndRoom();
            yield break;
        }

        new Task(waves[waveIndex].SpawnWaveOnDelay());

        waveIndex++;
    }

}



