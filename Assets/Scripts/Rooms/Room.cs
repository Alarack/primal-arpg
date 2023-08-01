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

    public void StartRoom() {

    }

    public void EndRoom() {

    }


    [System.Serializable]
    public class RoomReward {
        public List<ItemDefinition> items = new List<ItemDefinition>();
    }

}



