using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{


    public static Room CurrentRoom { get; private set; }

    public static string CurrentBiome { get; private set; }

    public static float CurrentDifficulty { get; private set; } = 5f;

    private int currentRoomIndex;
    private List<Room> roomList = new List<Room>();

    public static bool MultiReward { get; private set; }
    private List<RewardPedestal> currentRewards = new List<RewardPedestal>();

    private void Awake() {
        CurrentBiome = "Grasslands";
    }

    public static void EnterRoom(Room room) {
        room.StartRoom();
    }

    public void OnPortalEntered(Room room) {
        CurrentRoom = room;
        EnterRoom(room);
    }

    public static void AdjustDifficulty(float difficulty) {
        CurrentDifficulty += difficulty;
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



    #region REWARDS
    public static void OnRewardSelected(RewardPedestal reward) {
        
        if(MultiReward == false) {

            reward.DispenseReward();
            
            for (int i = 0; i < Instance.currentRewards.Count; i++) {
                if (Instance.currentRewards[i] != reward) {
                    Destroy(Instance.currentRewards[i].gameObject);
                }
            }

            Destroy(reward.gameObject);

            Instance.currentRewards.Clear();
        }
        else {
            reward.DispenseReward();
            Instance.currentRewards.Remove(reward);
            Destroy(reward.gameObject);
        }
        
        
    }


    #endregion

}
