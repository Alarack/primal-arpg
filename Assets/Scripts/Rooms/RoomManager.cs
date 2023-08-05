using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomManager : Singleton<RoomManager> {

    public RewardPedestal pedestalTemplate;
    public Transform pedestalHolderLeft;
    public Transform pedistalHolderRight;

    public List<ItemDefinition> testRewardItems = new List<ItemDefinition>();

    public static Room CurrentRoom { get; private set; }

    public static string CurrentBiome { get; private set; }

    public static float CurrentDifficulty { get; private set; } = 5f;

    private int currentRoomIndex;
    private List<Room> roomList = new List<Room>();

    public static bool MultiReward { get; private set; }
    private List<RewardPedestal> currentRewards = new List<RewardPedestal>();

    private List<RoomPortalDisplay> currentPortals = new List<RoomPortalDisplay>();

    private void Awake() {
        CurrentBiome = "Grasslands";
    }

    public static void EnterRoom(Room room) {
        room.StartRoom();

        Instance.currentRoomIndex++;
    }

    public void OnPortalEntered(Room room) {
        CurrentRoom = room;
        EnterRoom(room);
    }

    public void OnRoomEnded(Room room) {

    }

    public static void AdjustDifficulty(float difficulty) {
        CurrentDifficulty += difficulty;
    }

    public static void SpawnRoomPortals() {
        Debug.Log("Choose and spawn X Rooms");
    }



    public static Room GenerateRoom(float difficultyMod = 0f) {


        Room room = Instance.currentRoomIndex switch {
            0 => CreateRoom(Room.RoomType.StartRoom, difficultyMod),
            5 => CreateRoom(Room.RoomType.MiniBossRoom, difficultyMod),
            10 => CreateRoom(Room.RoomType.BossRoom, difficultyMod),

            _ => CreateRandomRoom(difficultyMod),
        };




        return room;
    }


    public static Room CreateRandomRoom(float difficultyMod = 0f) {

        Room.RoomType[] allTypes = System.Enum.GetValues(typeof(Room.RoomType)) as Room.RoomType[];

        List<Room.RoomType> excludedTypes = new List<Room.RoomType> {
            Room.RoomType.StartRoom,
            Room.RoomType.BossRoom,
            Room.RoomType.MiniBossRoom,
            Room.RoomType.SecretRoom
        };

        List<Room.RoomType> validTypes = new List<Room.RoomType>();

        for (int i = 0; i < allTypes.Length; i++) {
            if (excludedTypes.Contains(allTypes[i])) {
                continue;
            }

            validTypes.Add(allTypes[i]);
        }

        int randomIndex = Random.Range(0, validTypes.Count);

        return CreateRoom(validTypes[randomIndex], difficultyMod);

    }

    public static Room CreateRoom(Room.RoomType roomType, float difficultyModifier) {

        Room result = roomType switch {
            Room.RoomType.StartRoom => new StartingRoom(),
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



    public static void OnRoomSelected(Room room) {
        Debug.Log("Room Selected: " + room.Data.type);

        Instance.OnPortalEntered(room);

        for (int i = 0; i < Instance.currentPortals.Count; i++) {
            Destroy(Instance.currentPortals[i].gameObject);
        }

        Instance.currentPortals.Clear();





    }



    #region REWARDS

    public static List<Room.RoomReward> CreateRoomRewards(Room room, Room.RoomReward.RewardCategory category, int rewardCount = 3, bool allowDupes = false) {
        List<Room.RoomReward> results = new List<Room.RoomReward>();

        List<ItemDefinition> SelectedRewards = new List<ItemDefinition>();


        for (int i = 0; i < rewardCount; i++) {

            ItemDefinition possibleReward = category switch {
                Room.RoomReward.RewardCategory.ClassSkill => throw new System.NotImplementedException(),
                Room.RoomReward.RewardCategory.ElementalSkill => throw new System.NotImplementedException(),
                Room.RoomReward.RewardCategory.PassiveSkill => throw new System.NotImplementedException(),
                Room.RoomReward.RewardCategory.Weapon => throw new System.NotImplementedException(),
                Room.RoomReward.RewardCategory.Equipment => throw new System.NotImplementedException(),
                Room.RoomReward.RewardCategory.SkillRune => throw new System.NotImplementedException(),
                Room.RoomReward.RewardCategory.Currency => throw new System.NotImplementedException(),
                Room.RoomReward.RewardCategory.ClassSelection => throw new System.NotImplementedException(),
                _ => null,
            };

        }

        //for (int i = 0; i < rewardCount; i++) {
        //    Room.RoomReward reward = room.Type switch {
        //        Room.RoomType.StartRoom => throw new System.NotImplementedException(),
        //        Room.RoomType.EliminationCombat => throw new System.NotImplementedException(),
        //        Room.RoomType.ItemShop => throw new System.NotImplementedException(),
        //        Room.RoomType.SkillShop => throw new System.NotImplementedException(),
        //        Room.RoomType.RecoveryRoom => throw new System.NotImplementedException(),
        //        Room.RoomType.SurvivalCombat => throw new System.NotImplementedException(),
        //        Room.RoomType.BossRoom => throw new System.NotImplementedException(),
        //        Room.RoomType.SecretRoom => throw new System.NotImplementedException(),
        //        Room.RoomType.TreasureRoom => throw new System.NotImplementedException(),
        //        Room.RoomType.MiniBossRoom => throw new System.NotImplementedException(),
        //        Room.RoomType.EventRoom => throw new System.NotImplementedException(),
        //        _ => null,
        //    };
        //}

        return results;
    }

    public static void CreateRewards(List<ItemDefinition> rewardItems, bool multiReward = false) {
        MultiReward = multiReward;

        for (int i = 0; i < rewardItems.Count; i++) {
            Vector2 targetPos = Vector2.Lerp(Instance.pedestalHolderLeft.position, Instance.pedistalHolderRight.position, (i + 0.5f) / rewardItems.Count);

            RewardPedestal pedestal = Instantiate(Instance.pedestalTemplate, targetPos, Quaternion.identity);
            pedestal.transform.SetParent(Instance.transform, false);
            pedestal.Setup(rewardItems[i]);
            Instance.currentRewards.Add(pedestal);

        }

    }


    public static void OnRewardSelected(RewardPedestal reward) {

        if (MultiReward == false) {

            reward.DispenseReward();

            for (int i = 0; i < Instance.currentRewards.Count; i++) {
                if (Instance.currentRewards[i] != reward) {
                    Destroy(Instance.currentRewards[i].gameObject);
                }
            }

            Destroy(reward.gameObject);

            Instance.currentRewards.Clear();
            CurrentRoom.EndRoom();
        }
        else {
            reward.DispenseReward();
            Instance.currentRewards.Remove(reward);
            Destroy(reward.gameObject);

            if (Instance.currentRewards.Count == 0) {
                CurrentRoom.EndRoom();
            }
        }


    }


    #endregion

}
