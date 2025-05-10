using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPedestal : MonoBehaviour
{

    public ItemData rewardItem;

    public RewardPedestalDisplay display;
    public GameObject blueAura;
    public GameObject shopAura;
    public Mask passiveMask;
    public Image maskImage;
    public Image skillFrameImage;
    public Sprite passiveFrameSprite;
    public Sprite activeFrameSprite;
    public GameObject skillPointVFX;
    public AudioClip skillPointAudio;
    public GameObject collectionVFX;

    public bool enforceCost;

    public void Setup(ItemData rewardItem, bool enforceCost = false) {
        this.rewardItem = rewardItem;
        this.enforceCost = enforceCost;
        display.Setup(this);

        blueAura.gameObject.SetActive(enforceCost == false);
        shopAura.gameObject.SetActive(enforceCost == true);
        passiveMask.enabled = false;
        
        if(rewardItem.Type == ItemType.Skill) {
            if(rewardItem.learnableAbilities.Count > 0) {
                skillFrameImage.enabled = true;

                if (rewardItem.learnableAbilities[0].AbilityData.category == AbilityCategory.PassiveSkill) {
                    maskImage.enabled = true;
                    passiveMask.enabled = true;
                    skillFrameImage.sprite = passiveFrameSprite;
                }
                else {
                    skillFrameImage.sprite = activeFrameSprite;
                }
            }
        }
        else {
            maskImage.enabled = false;
            passiveMask.enabled = false;
            skillFrameImage.enabled = false;
        }

    }

    public void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag != "Player")
            return;
    }



    public void DispenseReward() {

        switch (rewardItem.Type) {
            case ItemType.None:
                break;
         
            case ItemType.Currency:
                ItemSpawner.SpawnCoins((int)(RoomManager.CurrentDifficulty * 15f), transform.position, 1f, 1f);
                break;
            case ItemType.SkillPoint:
                StatAdjustmentManager.AdjustSkillPoints(EntityManager.ActivePlayer, 1f);
                //Debug.LogWarning("TODO: Show a fanfare effect here for when skill points are awarded");
                VFXUtility.SpawnVFX(skillPointVFX, EntityManager.ActivePlayer.transform.position, Quaternion.identity, null, 3f);
                AudioManager.PlaySoundClip(skillPointAudio, Vector2.zero, 1f);
                break;

            case ItemType.HealthPotion:
                StatAdjustmentManager.AdjustHealthPotions(EntityManager.ActivePlayer, 1f);
                break;
            default:
                ItemSpawner.SpawnItem(rewardItem, transform.position, true);
                VFXUtility.SpawnVFX(collectionVFX, transform.position, Quaternion.identity, null, 2f);
                break;
        }


    }

}
