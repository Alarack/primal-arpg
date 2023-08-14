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
    public int count;
    public float vfxDelay;

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
        if (Input.GetKeyDown(KeyCode.P)) {
            //SpawnWave();

            SpawnGeneratedWave();
        }
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
            results.Remove(target);
        }

        //if (ActiveEntities[Entity.EntityType.Enemy].Count == 0) {
        if (Instance.enemiesClearedCheck == null) {
            Instance.enemiesClearedCheck = new Task(Instance.DelayedCheckForEnemies());
        }
        else if(Instance.enemiesClearedCheck.Running == false) {
            Instance.enemiesClearedCheck = new Task(Instance.DelayedCheckForEnemies());
        }


       

        //}
        //if (ActiveEntities[Entity.EntityType.Enemy].Count == 0) {
        //    //SpawnWave();
        //    //SpawnGeneratedWave();
        //    //Debug.LogWarning("Spawning Next Wave");

        //    if(RoomManager.CurrentRoom != null) {
        //       RoomManager.CurrentRoom.OnAllEnemiesKilled();

        //    }



        //}
    }

    private IEnumerator DelayedCheckForEnemies() {
        yield return new WaitForSeconds(0.5f);

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



    public void CreatePlayer() {

        if (ActivePlayer == null)
            Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);


        new Task(OpenDefaultPanels());
    }

    private IEnumerator OpenDefaultPanels() {
        yield return new WaitForEndOfFrame();

        PanelManager.ClosePanel<InventoryPanel>();
        PanelManager.ClosePanel<SkillsPanel>();
        PanelManager.OpenPanel<HotbarPanel>();
        PanelManager.OpenPanel<HUDPanel>();

        //RoomManager.CreateRewards(RoomManager.Instance.testRewardItems);

        TestingProcGenThings();
    }


    private void TestingProcGenThings() {

        Room startingRoom = RoomManager.CreateRoom(Room.RoomType.StartRoom, 0f);
        RoomManager.Instance.OnPortalEntered(startingRoom);


        //List<ItemDefinition> results = new List<ItemDefinition>();

        //for (int i = 0; i < 5; i++) {
        //    ItemDefinition item = ItemSpawner.Instance.lootDatabase.GetItem(ItemType.ClassSelection, results);

        //    if(item != null) {
        //        results.Add(item);
        //        Debug.Log(item.itemData.itemName + " has been added");
        //    }

        //}



    }

    //public void CreateEnemy(Vector2 spawnLocation) {
    //    Entity newEnemy = Instantiate(testEnemyPrefab, spawnLocation, Quaternion.identity);

    //}

    //public void SpawnWave() {

    //    if (spawnPoints.Length < 1) {
    //        Debug.LogWarning("No spawn points in entity manager. Cannot spawn wave");
    //        return;
    //    }

    //    for (int i = 0; i < enemiesPerWave; i++) {
    //        int randomIndex = Random.Range(0, spawnPoints.Length);
    //        Transform targetPoint = spawnPoints[randomIndex];

    //        CreateEnemy(targetPoint.position);

    //    }

    //}


    public static List<Wave> GenerateWaves(int waveCount, string biome, float totalThreat, float minSingleThreat = 1f, float maxSingleThreat = 100f) {
        List<Wave> results = new List<Wave>();

        for (int i = 0; i < waveCount; i++) {

            float waveIncrement = 1 + (i / 2f);

            List<NPC> waveMobs = NPCDataManager.GetSpawnList(biome, totalThreat * waveIncrement, minSingleThreat, maxSingleThreat);

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



