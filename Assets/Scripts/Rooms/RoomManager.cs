using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LL.Events;

public class RoomManager : Singleton<RoomManager> {

    public RewardPedestal pedestalTemplate;
    public Transform pedestalHolderLeft;
    public Transform pedistalHolderRight;

    public RoomPortalDisplay roomPortalTemplate;

    public static Room CurrentRoom { get; private set; }

    public static string CurrentBiome { get; private set; }

    public static float CurrentDifficulty { get; private set; } = 5f;

    private int currentRoomIndex;
    //private List<Room> roomList = new List<Room>();

    public static bool MultiReward { get; private set; }
    private List<RewardPedestal> currentRewards = new List<RewardPedestal>();

    private List<RoomPortalDisplay> currentPortals = new List<RoomPortalDisplay>();

    private Task rewardSpawnTask;
    private Task createPortalsTask;
    private void Awake() {
        CurrentBiome = "Grasslands";
    }

    public static void EnterRoom(Room room) {
        Instance.currentRoomIndex++;
        Debug.Log("Starting a room: " + room.Type.ToString());
        room.StartRoom();
    }

    private void OnEnable() {
        EventManager.RegisterListener(GameEvent.LevelUpAbilitySelected, OnLevelAbilitySelected);
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }

    private void Update() {

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.R) && CurrentRoom != null &&  CurrentRoom.Type == Room.RoomType.ItemShop ) {
            ShopRoom shop = CurrentRoom as ShopRoom;
            shop.RerollShop();
        }

#endif
    }

    public void OnPortalEntered(Room room) {
        CurrentRoom = room;
        EnterRoom(room);
    }

    public static void OnRoomEnded(Room room) {
        
        if(room.Type == Room.RoomType.EliminationCombat || 
            room.Type == Room.RoomType.BossRoom ||
            room.Type == Room.RoomType.SurvivalCombat) {
            
            AdjustDifficulty(1f);
        }

    }

    private void OnLevelAbilitySelected(EventData data) {
        //if(CurrentRoom != null) {
        //    CurrentRoom.RegenerateReward();
        //}
    }

    public static void CheckLevelUp() {
        //if(EntityManager.ActivePlayer.levelsStored > 0) {
        //    PanelManager.OpenPanel<LevelUpPanel>();
        //}
    }

    public static void AdjustDifficulty(float difficulty) {
        CurrentDifficulty += difficulty;
    }

    public static void SetDifficulty(float difficulty) {
        CurrentDifficulty = difficulty;
    }

    public static void ClearRooms() {
        Instance.CleanUpRoomPortals();
        Instance.CleanUpRewardPedestals();
        Instance.currentRoomIndex = 0;
    }

    public static void SpawnRoomPortals(int portalCount = 2, List<Vector2> portalLocations = null) {
        //Debug.Log("Choose and spawn X Rooms");

        //Debug.Log("Room Index: " + Instance.currentRoomIndex);

        //PanelManager.OpenPanel<TextDisplayPanel>().Setup("Choose a Room");
        Room.RoomType roomType = Instance.GetRoomType();

        //List<ItemType> rewardTypes = new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint };
        List<ItemType> rewardTypes = roomType switch {
            Room.RoomType.EliminationCombat => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
            Room.RoomType.ItemShop => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.SkillPoint },
            Room.RoomType.SkillShop => new List<ItemType> { ItemType.Skill, ItemType.SkillPoint },
            Room.RoomType.RecoveryRoom => throw new System.NotImplementedException(),
            Room.RoomType.SurvivalCombat => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
            Room.RoomType.BossRoom => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
            Room.RoomType.SecretRoom => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
            Room.RoomType.TreasureRoom => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
            Room.RoomType.MiniBossRoom => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
            Room.RoomType.EventRoom => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
            _ => new List<ItemType> { ItemType.Skill, ItemType.Equipment, ItemType.Currency, ItemType.SkillPoint },
        };

        if(EntityManager.ActivePlayer.Stats.GetStatRangeRatio(StatName.Health) <= 0.25f 
            || EntityManager.ActivePlayer.Stats.GetStatRangeRatio(StatName.HeathPotions) < 1f) {
            rewardTypes.Add(ItemType.HealthPotion);
        }

        rewardTypes.Shuffle();

        List<ItemType> chosenTypes = new List<ItemType>();


        if (portalCount > rewardTypes.Count) {
            Debug.LogError("More portals than reward types, can't be distinct");
        }

        for (int i = 0; i < portalCount; i++) {

            if (i >= rewardTypes.Count) {
                Debug.LogWarning("Reset count since i is creater than possible reward types");
                i = 0;

                if (chosenTypes.Count > 0)
                    break;
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
            Room room = CreateRoom(Instance.GetRoomType(), type);
            //Debug.Log("Creating a room.Reward: " + room.rewards[0].rewardDescription);
            choices.Add(room);
        }


        //if (Instance.currentRoomIndex <= 5) {
        //    //Debug.Log("Current Room: " + Instance.currentRoomIndex);
        //    if (chosenTypes.Contains(ItemType.Skill) == false) {
        //        //Debug.LogWarning("Adding a skill room since there wasn't one");
        //        choices[0] = CreateRoom(Instance.GetRoomType(), ItemType.Skill);
        //    }
        //}


        EntityManager.ActivePlayer.DeactivateBigVacum();
        CreateRoomPortals(choices, portalLocations);

    }

    private Room.RoomType GetRoomType() {
        if (Instance.currentRoomIndex % 2 == 0 && Instance.currentRoomIndex != 0)
            return Room.RoomType.ItemShop;

        if (Instance.currentRoomIndex % 5 == 0 && Instance.currentRoomIndex != 0)
            return Room.RoomType.BossRoom;

        return Room.RoomType.EliminationCombat;
    }

    public static void CreateRoomPortals(List<Room> rooms, List<Vector2> portalLocations = null) {
        Instance.createPortalsTask = new Task(Instance.CreateRoomPortalsOnDelay(rooms, portalLocations));
    }

    private IEnumerator CreateRoomPortalsOnDelay(List<Room> rooms, List<Vector2> portalLocations = null) {
        WaitForSeconds waiter = new WaitForSeconds(0.2f);

        if(portalLocations == null) {
            portalLocations = new List<Vector2>() { Instance.pedestalHolderLeft.position, Instance.pedistalHolderRight.position };
        }


        for (int i = 0; i < rooms.Count; i++) {
            Vector2 targetPos = Vector2.Lerp(portalLocations[0], portalLocations[ portalLocations.Count -1], (i + 0.5f) / rooms.Count);

            RoomPortalDisplay portal = Instantiate(Instance.roomPortalTemplate, targetPos, Quaternion.identity);
            portal.Setup(rooms[i]);

            Instance.currentPortals.Add(portal);
            yield return waiter;
        }

        createPortalsTask = null;
    }



    //public static Room GenerateRoom(ItemType rewardType = ItemType.None, AbilityTag rewardTag = AbilityTag.None, ItemSlot rewardSlot = ItemSlot.None, float difficultyMod = 0f) {


    //    Room room = Instance.currentRoomIndex switch {
    //        0 => CreateRoom(Room.RoomType.StartRoom, rewardType, rewardTag, rewardSlot, difficultyMod),
    //        5 => CreateRoom(Room.RoomType.MiniBossRoom, rewardType, rewardTag, rewardSlot, difficultyMod),
    //        10 => CreateRoom(Room.RoomType.BossRoom, rewardType, rewardTag, rewardSlot, difficultyMod),

    //        _ => CreateRandomRoom(rewardType, rewardTag, rewardSlot, difficultyMod),
    //    };

    //    return room;
    //}

    //public static Room CreateRandomRoom(ItemType rewardType = ItemType.None, AbilityTag rewardTag = AbilityTag.None, ItemSlot rewardSlot = ItemSlot.None, float difficultyMod = 0f) {

    //    Room.RoomType[] allTypes = System.Enum.GetValues(typeof(Room.RoomType)) as Room.RoomType[];

    //    List<Room.RoomType> excludedTypes = new List<Room.RoomType> {
    //        Room.RoomType.StartRoom,
    //        Room.RoomType.BossRoom,
    //        Room.RoomType.MiniBossRoom,
    //        Room.RoomType.SecretRoom
    //    };

    //    List<Room.RoomType> validTypes = new List<Room.RoomType>();

    //    for (int i = 0; i < allTypes.Length; i++) {
    //        if (excludedTypes.Contains(allTypes[i])) {
    //            continue;
    //        }

    //        validTypes.Add(allTypes[i]);
    //    }

    //    int randomIndex = Random.Range(0, validTypes.Count);

    //    return CreateRoom(validTypes[randomIndex], rewardType, rewardTag, rewardSlot, difficultyMod);

    //}

    public static Room CreateRoom(Room.RoomType roomType, ItemType rewardType = ItemType.None, AbilityTag rewardTag = AbilityTag.None, ItemSlot rewardSlot = ItemSlot.None, float difficultyModifier = 0f) {

        Room result = roomType switch {
            Room.RoomType.StartRoom => new StartingRoom(),
            Room.RoomType.EliminationCombat => new EliminitionCombatRoom(rewardType, rewardTag, rewardSlot),
            Room.RoomType.ItemShop => new ShopRoom(rewardType, rewardTag, rewardSlot),
            Room.RoomType.SkillShop => throw new System.NotImplementedException(),
            Room.RoomType.RecoveryRoom => throw new System.NotImplementedException(),
            Room.RoomType.SurvivalCombat => throw new System.NotImplementedException(),
            Room.RoomType.BossRoom => new BossRoom(rewardType, rewardTag, rewardSlot),
            Room.RoomType.SecretRoom => throw new System.NotImplementedException(),
            Room.RoomType.TreasureRoom => throw new System.NotImplementedException(),
            _ => throw new System.NotImplementedException(),
        };



        return result;
    }

    public static void OnRoomSelected(Room room) {
        //Debug.Log("Room Selected: " + room.Type);
        if (Instance.createPortalsTask != null && Instance.createPortalsTask.Running == true) {
            Debug.LogWarning("Portal creation task is running");
            
            return;
        }


        Instance.OnPortalEntered(room);

        Instance.CleanUpRoomPortals();
        Instance.CleanUpRewardPedestals();

        //for (int i = 0; i < Instance.currentPortals.Count; i++) {
        //    Destroy(Instance.currentPortals[i].gameObject);
        //}

        //Instance.currentPortals.Clear();

        //PanelManager.ClosePanel<TextDisplayPanel>();
    }

    private void CleanUpRoomPortals() {
        for (int i = 0; i < currentPortals.Count; i++) {
            Destroy(currentPortals[i].gameObject);
        }

        currentPortals.Clear();
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


    public static void RerollRewards(List<ItemDefinition> rewardItems, string displayText, bool multiReward = false, bool shopMode = false) {
        Instance.CleanUpRewardPedestals();
        CreateRewards(rewardItems, displayText, multiReward, shopMode);
    }

    public static void CreateRewards(List<ItemDefinition> rewardItems, string displayText, bool multiReward = false, bool shopMode = false) {
        //CheckLevelUp();
        
        MultiReward = multiReward;

        //PanelManager.OpenPanel<TextDisplayPanel>().Setup(displayText);
        EntityManager.ActivePlayer.ActivateBigVacum();

        if (rewardItems.Count == 0) {
            Debug.LogWarning("No rewards. Sad face");
            CurrentRoom.EndRoom();
            return;
        }


        Instance.rewardSpawnTask = new Task(Instance.SpawnRewardsOnDelay(rewardItems, shopMode));

        if(CurrentRoom != null && (
            CurrentRoom.Type == Room.RoomType.EliminationCombat 
            || CurrentRoom.Type == Room.RoomType.SurvivalCombat 
            || CurrentRoom.Type == Room.RoomType.BossRoom)) {

            if (PlayerPrefs.GetInt("HealOnRoomEnd") == 1) {
                ItemSpawner.SpawnHealthOrbs(2, Vector2.zero);
            }

        }





    }

    private IEnumerator SpawnRewardsOnDelay(List<ItemDefinition> rewardItems, bool shopMode = false) {
        WaitForSeconds waiter = new WaitForSeconds(0.2f);

        yield return new WaitForSeconds(0.75f);

        for (int i = 0; i < rewardItems.Count; i++) {
            Vector2 targetPos = Vector2.Lerp(Instance.pedestalHolderLeft.position, Instance.pedistalHolderRight.position, (i + 0.5f) / rewardItems.Count);

            RewardPedestal pedestal = Instantiate(Instance.pedestalTemplate, targetPos, Quaternion.identity);
            pedestal.transform.SetParent(Instance.transform, false);
            pedestal.Setup(rewardItems[i].itemData, shopMode);
            Instance.currentRewards.Add(pedestal);
            yield return waiter;
        }


        rewardSpawnTask = null;
    }



    public static void OnRewardSelected(RewardPedestal reward) {

        if (Instance.rewardSpawnTask != null && Instance.rewardSpawnTask.Running == true) {
            return;
        }


        //Debug.Log("Selected Reward: " + reward.rewardItem.itemName);

        if (MultiReward == false) {

            reward.DispenseReward();
            //PanelManager.ClosePanel<TextDisplayPanel>();

            Instance.CleanUpRewardPedestals();

            //for (int i = 0; i < Instance.currentRewards.Count; i++) {
            //    if (Instance.currentRewards[i] != reward) {
            //        Destroy(Instance.currentRewards[i].gameObject);
            //    }
            //}

            //Destroy(reward.gameObject);

            //Instance.currentRewards.Clear();
            CurrentRoom.EndRoom();
        }
        else {
            reward.DispenseReward();
            Instance.currentRewards.Remove(reward);
            Destroy(reward.gameObject);

            if (Instance.currentRewards.Count == 0) {
                //PanelManager.ClosePanel<TextDisplayPanel>();
                CurrentRoom.EndRoom();

            }
        }


    }

    public void CleanUpRewardPedestals() {
        for (int i = 0; i < currentRewards.Count; i++) {
            Destroy(currentRewards[i].gameObject);
        }

        currentRewards.Clear();
    }


    public static List<string> GetSKillRewardNames() {
        List<string> results = new List<string>();
        
        if (CurrentRoom == null)
            return results;

        if(CurrentRoom.rewards == null || CurrentRoom.rewards.Count == 0) 
            return results;

        for (int i = 0; i < CurrentRoom.rewards.Count; i++) {
            if (CurrentRoom.rewards[i].itemCategory == ItemType.Skill) {
                Room.RoomReward reward = CurrentRoom.rewards[i];

                for (int j = 0; j < reward.items.Count; j++) {
                    results.Add(reward.items[j].itemData.itemName);
                    //Debug.Log("A skill: " + reward.items[j].itemData.itemName + " is in the rewards selection");
                }


            }
        }

        return results;
    }

    #endregion

}
