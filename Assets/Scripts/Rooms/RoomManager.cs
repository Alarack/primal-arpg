using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{


    public Room CurrentRoom { get; private set; }

    private int currentRoomIndex;
    private List<Room> roomList = new List<Room>();




    public static void EnterRoom(Room room) {
        room.StartRoom();
    }

    public static void SpawnRoomPortals() {
        Debug.Log("Choose and spawn X Rooms");
    }

    public static Room CreateRoom(Room.RoomType roomType, float difficultyModifier) {

        Room result = roomType switch {
            Room.RoomType.StartRoom => throw new System.NotImplementedException(),
            Room.RoomType.EliminationCombat => throw new System.NotImplementedException(),
            Room.RoomType.ItemShop => throw new System.NotImplementedException(),
            Room.RoomType.SkillShop => throw new System.NotImplementedException(),
            Room.RoomType.RecoveryRoom => throw new System.NotImplementedException(),
            Room.RoomType.SurvivalCombat => throw new System.NotImplementedException(),
            Room.RoomType.BossRoom => throw new System.NotImplementedException(),
            Room.RoomType.SecretRoom => throw new System.NotImplementedException(),
            Room.RoomType.TreasureRoom => throw new System.NotImplementedException(),
            _ => throw new System.NotImplementedException(),
        };



        return result;
    }

}
