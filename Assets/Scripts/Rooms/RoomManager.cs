using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomManager : Singleton<RoomManager> {

    public RewardPedestal pedestalTemplate;
    public Transform pedestalHolderLeft;
    public Transform pedistalHolderRight;

    public RoomPortalDisplay roomPortalTemplate;

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

    public static void SpawnRoomPortals(int portalCount = 2) {
        //Debug.Log("Choose and spawn X Rooms");


        PanelManager.OpenPanel<TextDisplayPanel>().Setup("Choose a Room");


        List<ItemType> rewardTypes = new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Rune};

        rewardTypes.Shuffle();

        List<ItemType> chosenTypes = new List<ItemType>();

        if(Instance.currentRoomIndex <= 6) {
            chosenTypes.Add(ItemType.Skill);
        }

        if(portalCount > rewardTypes.Count) {
            Debug.LogError("More portals than reward types, can't be distinct");
        }

        for (int i = 0; i < portalCount; i++) {
            
            if(i >= rewardTypes.Count) {
                Debug.LogWarning("Resent count since i is creater than possible reward types");
                i = 0;
            }

            if (chosenTypes.Count >= rewardTypes.Count) {
                Debug.LogWarning("Clearning choosen types");
                chosenTypes.Clear();
            }

            if (chosenTypes.Contains(rewardTypes[i])) {
                continue;
            }

            chosenTypes.Add(rewardTypes[i]);
        }

        List<Room> choices = new List<Room>();
        foreach (ItemType type in chosenTypes) {
            Room room = CreateRoom(Room.RoomType.EliminationCombat, type);
            //Debug.Log("Creating a room.Reward: " + room.rewards[0].rewardDescription);
            choices.Add(room);
        }



        CreateRoomPortals(choices);

    }



    public static Room GenerateRoom(ItemType rewardType = ItemType.None, AbilityTag rewardTag = AbilityTag.None, ItemSlot rewardSlot = ItemSlot.None, float difficultyMod = 0f) {


        Room room = Instance.currentRoomIndex switch {
            0 => CreateRoom(Room.RoomType.StartRoom, rewardType, rewardTag, rewardSlot, difficultyMod),
            5 => CreateRoom(Room.RoomType.MiniBossRoom, rewardType, rewardTag, rewardSlot, difficultyMod),
            10 => CreateRoom(Room.RoomType.BossRoom, rewardType, rewardTag, rewardSlot, difficultyMod),

            _ => CreateRandomRoom(rewardType, rewardTag, rewardSlot, difficultyMod),
        };

        return room;
    }

    public static Room CreateRandomRoom(ItemType rewardType = ItemType.None, AbilityTag rewardTag = AbilityTag.None, ItemSlot rewardSlot = ItemSlot.None, float difficultyMod = 0f) {

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

        return CreateRoom(validTypes[randomIndex], rewardType, rewardTag, rewardSlot, difficultyMod);

    }

    public static Room CreateRoom(Room.RoomType roomType, ItemType rewardType = ItemType.None, AbilityTag rewardTag = AbilityTag.None, ItemSlot rewardSlot = ItemSlot.None, float difficultyModifier = 0f) {

        Room result = roomType switch {
            Room.RoomType.StartRoom => new StartingRoom(),
            Room.RoomType.EliminationCombat => new EliminitionCombatRoom(rewardType, rewardTag, rewardSlot),
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
        Debug.Log("Room Selected: " + room.Type);

        Instance.OnPortalEntered(room);

        for (int i = 0; i < Instance.currentPortals.Count; i++) {
            Destroy(Instance.currentPortals[i].gameObject);
        }

        Instance.currentPortals.Clear();

        PanelManager.ClosePanel<TextDisplayPanel>();
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

    public static void CreateRewards(List<ItemDefinition> rewardItems, string displayText, bool multiReward = false) {
        MultiReward = multiReward;

        PanelManager.OpenPanel<TextDisplayPanel>().Setup(displayText);


        for (int i = 0; i < rewardItems.Count; i++) {
            Vector2 targetPos = Vector2.Lerp(Instance.pedestalHolderLeft.position, Instance.pedistalHolderRight.position, (i + 0.5f) / rewardItems.Count);

            RewardPedestal pedestal = Instantiate(Instance.pedestalTemplate, targetPos, Quaternion.identity);
            pedestal.transform.SetParent(Instance.transform, false);
            pedestal.Setup(rewardItems[i]);
            Instance.currentRewards.Add(pedestal);

        }

        if(rewardItems.Count == 0) {
            Debug.LogWarning("No rewards. Sad face");
            CurrentRoom.EndRoom();
        }

    }

    public static void CreateRoomPortals(List<Room> rooms) {
        for (int i = 0; i < rooms.Count; i++) {
            Vector2 targetPos = Vector2.Lerp(Instance.pedestalHolderLeft.position, Instance.pedistalHolderRight.position, (i + 0.5f) / rooms.Count);

            RoomPortalDisplay portal = Instantiate(Instance.roomPortalTemplate, targetPos, Quaternion.identity);
            portal.Setup(rooms[i]);

            Instance.currentPortals.Add(portal);

        }
    }


    public static void OnRewardSelected(RewardPedestal reward) {

        if (MultiReward == false) {

            reward.DispenseReward();
            PanelManager.ClosePanel<TextDisplayPanel>();

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
                PanelManager.ClosePanel<TextDisplayPanel>();
                CurrentRoom.EndRoom();
                
            }
        }

        
    }


    #endregion

}
