using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPedestal : MonoBehaviour
{

    public ItemDefinition rewardItem;



    public void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag != "Player")
            return;
    }



    public void DispenseReward() {
        ItemSpawner.SpawnItem(rewardItem, transform.position);
    }

}