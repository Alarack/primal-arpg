using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPedestal : MonoBehaviour
{

    public ItemData rewardItem;

    public RewardPedestalDisplay display;



    public void Setup(ItemData rewardItem) {
        this.rewardItem = rewardItem;
        display.Setup(this);
    }

    public void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag != "Player")
            return;
    }



    public void DispenseReward() {
        ItemSpawner.SpawnItem(rewardItem, transform.position, true);
    }

}
