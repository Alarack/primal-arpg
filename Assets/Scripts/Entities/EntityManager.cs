using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : Singleton<EntityManager> {

    [Header("Test Variable")]
    public Entity testEnemyPrefab;
    public Entity playerPrefab;
    public Transform[] spawnPoints;
    public int enemiesPerWave = 3;

    public static EntityPlayer ActivePlayer { get { return GetPlayer(); } }

    public static Dictionary<Entity.EntityType, List<Entity>> ActiveEntities { get; private set; } = new Dictionary<Entity.EntityType, List<Entity>>(); 

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

        if (ActiveEntities[Entity.EntityType.Enemy].Count == 0) {
            Instance.SpawnWave();
        }
    }

    public static EntityPlayer GetPlayer() {
        if(ActiveEntities.TryGetValue(Entity.EntityType.Projectile, out List<Entity> results) == true) {
            return results[0] as EntityPlayer;
        }

        return null;
    }
    //public void CreatePlayer() {
    //    //Entity newPlayer = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity);

    //    PlayerController player = FindObjectOfType<PlayerController>(true);

    //    player.gameObject.SetActive(true);

    //    player.transform.position = Vector2.zero;

    //    player.Stats.Refresh(StatName.Health);
    //}

    public void CreateEnemy(Vector2 spawnLocation) {
        Entity newEnemy = Instantiate(testEnemyPrefab, spawnLocation, Quaternion.identity);

    }

    public void SpawnWave() {

        if(spawnPoints.Length < 1) {
            Debug.LogWarning("No spawn points in entity manager. Cannot spawn wave");
            return;
        }

        for (int i = 0; i < enemiesPerWave; i++) {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform targetPoint = spawnPoints[randomIndex];

            CreateEnemy(targetPoint.position);

        }

    }


}



