using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : Singleton<EntityManager> {

    [Header("Test Variable")]
    //public Entity testEnemyPrefab;
    public EntityPlayer playerPrefab;
    public Transform playerSpawnPoint;
    //public Transform[] spawnPoints;
    //public int enemiesPerWave = 3;

    [Header("Default Wave Variables")]
    public EntitySpawnIndicator spawnIndicator;
    public GameObject spawnVFX;
    public GameObject bossSpawnVFX;
    public int count;
    public float vfxDelay;
    public float bossVFXDelay;

    public List<Wave> waves = new List<Wave>();
    public List<Wave> genearatedWaves = new List<Wave>();
    public bool infiniteMode;
    private int waveIndex;

    private Task enemiesClearedCheck;

    public static EntityPlayer ActivePlayer { get { return GetPlayer(); } }

    public static Dictionary<Entity.EntityType, List<Entity>> ActiveEntities { get; private set; } = new Dictionary<Entity.EntityType, List<Entity>>();


    private void Start() {
        //SpawnWave();

        //genearatedWaves = GenerateWaves(3, "Grasslands", 5, 1, 5);
    }


    private void Update() {
#if UNITY_EDITOR

        //if (Input.GetKeyDown(KeyCode.P)) {
        //    //SpawnWave();

        //    SpawnGeneratedWave();
        //}


        if (Input.GetKeyDown(KeyCode.Keypad0)) {
            //SpawnWave();

            KillEnemies();
        }

#endif
    }


    public static void RegisterEntity(Entity target) {
        if (ActiveEntities.ContainsKey(target.entityType) == true) {
            ActiveEntities[target.entityType].Add(target);
        }
        else {

            List<Entity> newEntityList = new List<Entity> { target };

            ActiveEntities.Add(target.entityType, newEntityList);
        }
    }

    public static void RemoveEntity(Entity target) {
        if (ActiveEntities.TryGetValue(target.entityType, out List<Entity> results) == true) {
            if(results.RemoveIfContains(target) == false) {
                return;
            }
        }

        if(target.ownerType == OwnerConstraintType.Friendly) {
            return;
        }

        if (Instance.enemiesClearedCheck == null || Instance.enemiesClearedCheck.Running == false) {
            Instance.enemiesClearedCheck = new Task(Instance.DelayedCheckForEnemies());
        }

    }

    private IEnumerator DelayedCheckForEnemies() {
        yield return new WaitForSeconds(0.5f);

        if (ActiveEntities.ContainsKey(Entity.EntityType.Enemy) == false)
            yield break;

        if (ActiveEntities[Entity.EntityType.Enemy].Count == 0) {

            if (RoomManager.CurrentRoom != null) {
                RoomManager.CurrentRoom.OnAllEnemiesKilled();

            }
            else {
                enemiesClearedCheck = null;
            }

        }

        //enemiesClearedCheck = null;
    }
    public static EntityPlayer GetPlayer() {
        if (ActiveEntities.TryGetValue(Entity.EntityType.Player, out List<Entity> results) == true) {

            for (int i = 0; i < results.Count; i++) {
                if (results[i] is EntityPlayer) {
                    return results[i] as EntityPlayer;
                }
            }

            //return results[0] as EntityPlayer;
        }

        return null;
    }

    public static void SpawnWave() {

        if (Instance.waves.Count < 1) {
            Debug.LogError("No waves in entity manager");
            return;
        }

        if (Instance.waveIndex >= Instance.waves.Count) {
            if (Instance.infiniteMode == true)
                Instance.waveIndex = 0;
            else {
                Debug.LogWarning("Waves Finished");
                return;
            }
        }

        new Task(Instance.waves[Instance.waveIndex].SpawnWaveOnDelay());

        Instance.waveIndex++;
    }

    public static void KillEnemies() {
        if (ActiveEntities.ContainsKey(Entity.EntityType.Enemy) == false)
            return;

        for (int i = 0; i < ActiveEntities[Entity.EntityType.Enemy].Count; i++) {
            ActiveEntities[Entity.EntityType.Enemy][i].ForceDie(ActivePlayer);
        }
    }

    public static void SpawnGeneratedWave() {

        if (Instance.genearatedWaves.Count < 1) {
            Debug.LogError("No waves in entity manager");
            return;
        }

        if (Instance.waveIndex >= Instance.genearatedWaves.Count) {
            if (Instance.infiniteMode == true)
                Instance.waveIndex = 0;
            else {
                Debug.LogWarning("Waves Finished");
                return;
            }
        }

        new Task(Instance.genearatedWaves[Instance.waveIndex].SpawnWaveOnDelay());

        Instance.waveIndex++;
    }


    public static void GameOver() {

        //StatAdjustmentManager.ResetStat(ActivePlayer, StatName.Health);
        //StatAdjustmentManager.ResetStat(ActivePlayer, StatName.Essence);


        //ActivePlayer.Stats.Refresh(StatName.Health);
        //ActivePlayer.Stats.Refresh(StatName.Essence);
        ActivePlayer.Stats.RemoveAllMaxValueModifiersFromSource(StatName.Health, ActivePlayer);
        ActivePlayer.Stats.RemoveAllMaxValueModifiersFromSource(StatName.Essence, ActivePlayer);


        ActivePlayer.Stats.Refresh(StatName.Experience);
        ActivePlayer.Stats.SetStatValue(StatName.StatReroll, 3f, ActivePlayer);
        ActivePlayer.Stats.HardResetStatRange(StatName.HeathPotions, ActivePlayer, 1f);
        ActivePlayer.IsDead = false;
        ActivePlayer.ActiveChannelingAbility = null;
        ActivePlayer.ActivelyCastingAbility = null;
        ActivePlayer.RemoveAllStatuses();
        ActivePlayer.AbilityManager.ResetAbilities();
        ActivePlayer.ResetLevel();
        MasteryManager.Instance.LoadSavedMasteries();
       
        PanelManager.GetPanel<InventoryPanel>().ResetForge();
        PanelManager.GetPanel<HUDPanel>().ClearStatusUI();
        PanelManager.ClosePanel<LevelUpPanel>();
        PanelManager.ClosePanel<InventoryPanel>();
        PanelManager.ClosePanel<SkillsPanel>();
        PanelManager.ClosePanel<RunesPanel>();
        PanelManager.ClosePanel<MasteryPanel>();
        RoomManager.Instance.CleanUpRewardPedestals();
        RoomManager.InCombat = false;


        ClearRemainingEnemies();
        ClearRemainingProjectiles();
        ClearRemainingPickups();

        RoomManager.ClearRooms();
        RoomManager.SetDifficulty(5f);
    }


    public void CreatePlayer() {

        ClearRemainingEnemies();
        ClearRemainingPickups();
        RoomManager.Instance.CleanUpRewardPedestals();

        if (ActivePlayer != null) {

            ActivePlayer.transform.position = Vector3.zero;
            ActivePlayer.gameObject.SetActive(true);
            ActivePlayer.Stats.Refresh(StatName.Health);
            PanelManager.OpenPanel<CharacterSelectPanel>();
            PanelManager.ClosePanel<MainMenuPanel>();
            
            return;

        }

        Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);

        //OpenDefaultPanelsWithoutDelay();
        new Task(OpenDefaultPanels());
    }

    public static void ClearRemainingEnemies() {
        if (ActiveEntities.TryGetValue(Entity.EntityType.Enemy, out List<Entity> enemies)) {
            for (int i = 0; i < enemies.Count; i++) {
                enemies[i].EndGameCleanUp();
            }

            enemies.Clear();
        }
    }

    private static void ClearRemainingProjectiles() {
        Projectile[] leftoverProjectiles = GameObject.FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        for (int i = 0; i < leftoverProjectiles.Length; i++) {
            Debug.Log("Purging a Projectile: " + leftoverProjectiles[i].EntityName);
            leftoverProjectiles[i].EndGameCleanUp();
        }
    }

    private static void ClearRemainingPickups() {
        ItemPickup[] allPickups = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
        for(int i = 0;i < allPickups.Length; i++) {
            Destroy(allPickups[i]);
        }
    }

    private IEnumerator OpenDefaultPanels() {
        yield return new WaitForEndOfFrame();

        PanelManager.ClosePanel<MainMenuPanel>();
       
        PanelManager.GetPanel<InventoryPanel>();
        PanelManager.GetPanel<SkillsPanel>();
        PanelManager.OpenPanel<CharacterSelectPanel>();
        PanelManager.GetPanel<HotbarPanel>();
        PanelManager.GetPanel<HUDPanel>();
        

        //RoomManager.CreateRewards(RoomManager.Instance.testRewardItems);

        //TestingProcGenThings();
    }

    public static Wave GenerateBossWave(string biome) {
        Wave bossWave = new Wave();
        NPC bossPrefab = NPCDataManager.GetBoss(biome);

        WaveEntry waveEntry = new WaveEntry();
        waveEntry.spawnVFX = Instance.bossSpawnVFX;
        waveEntry.spawnIndicator = Instance.spawnIndicator;
        waveEntry.npcPrefab = bossPrefab;
        waveEntry.vfxDelay = Instance.bossVFXDelay;
        waveEntry.count = 1;

        bossWave.entries.Add(waveEntry);

        return bossWave;

    }

    public static List<Wave> GenerateWaves(int waveCount, string biome, float totalThreat, float minSingleThreat = 1f, float maxSingleThreat = 100f) {
        List<Wave> results = new List<Wave>();

        for (int i = 0; i < waveCount; i++) {

            float waveIncrement = 1 + (i / 2f);

            List<NPC> waveMobs = NPCDataManager.GetSpawnList(biome, totalThreat * waveIncrement, minSingleThreat, maxSingleThreat);

            if(waveMobs.Count == 0) {
                Debug.LogError("No enemies found when forming a wave. Check difficulty");
            }

            Wave newWave = new Wave();
            newWave.spanwDelay = 0.1f;

            for (int j = 0; j < waveMobs.Count; j++) {
                WaveEntry waveEntry = new WaveEntry();
                waveEntry.spawnVFX = Instance.spawnVFX;
                waveEntry.spawnIndicator = Instance.spawnIndicator;
                waveEntry.npcPrefab = waveMobs[j];
                waveEntry.vfxDelay = Instance.vfxDelay;
                waveEntry.count = 1;

                newWave.entries.Add(waveEntry);
            }

            results.Add(newWave);
        }

        return results;
    }


    [System.Serializable]
    public class Wave {

        public float spanwDelay = 0.1f;
        public List<WaveEntry> entries = new List<WaveEntry>();

        public IEnumerator SpawnWaveOnDelay() {
            WaitForSeconds waiter = new WaitForSeconds(spanwDelay);

            //Debug.LogWarning("Spawning a wave: " + entries.Count + " entries found");
            yield return new WaitForSeconds(0.3f);

            foreach (var entry in entries) {
                for (int i = 0; i < entry.count; i++) {
                    Vector3 randomSpawnPoint = TargetUtilities.GetViewportToWorldPoint(0.1f, 0.5f, 0.9f, 0.9f);

                    EntitySpawnIndicator spawnIndicator = GameObject.Instantiate(entry.spawnIndicator, randomSpawnPoint, Quaternion.identity);
                    spawnIndicator.Setup(entry.npcPrefab, entry.spawnVFX, entry.vfxDelay);
                    yield return waiter;
                    //NPC spawn = GameObject.Instantiate(entry.npcPrefab, randomSpawnPoint, Quaternion.identity);

                }
            }
        }


    }

    [System.Serializable]
    public class WaveEntry {
        public NPC npcPrefab;
        public EntitySpawnIndicator spawnIndicator;
        public GameObject spawnVFX;
        public int count;
        public float vfxDelay;
        //public float weight;
    }


}



