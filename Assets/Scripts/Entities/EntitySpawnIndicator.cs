using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawnIndicator : MonoBehaviour
{

    public float spawnTime;
    public float spawnAudioVolume = 1f;
    public List<AudioClip> spawnInSFX = new List<AudioClip>();
    
    private Entity entityToSpawn;


    public void Setup(Entity prefabToSpawn, GameObject spawnVFX, float timeToSpawn = 1f) {
        entityToSpawn = prefabToSpawn;
        spawnTime = timeToSpawn;
        GameObject activeSpawnVFX = Instantiate(spawnVFX, transform);
        activeSpawnVFX.transform.localPosition = Vector3.zero;
        AudioManager.PlayRandomClip(spawnInSFX, transform.position, spawnAudioVolume);

        new Task(SpawnEntity());
    }

    private IEnumerator SpawnEntity() {
        yield return new WaitForSeconds(spawnTime);
        Instantiate(entityToSpawn, transform.position, transform.rotation);
        Destroy(gameObject);
    }

}
