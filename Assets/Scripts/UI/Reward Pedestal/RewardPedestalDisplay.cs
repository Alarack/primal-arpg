using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class RewardPedestalDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {


    public Image rewardImage;

    [Header("Costs")]
    public TextMeshProUGUI costTextField;
    public GameObject costArea;

    [Header("Anim Presets")]
    public GameObject goldAnim;
    public GameObject skillPointAnim;


    private RewardPedestal pedestal;
    private Item displayItem;
    private Ability displayAbility;

    private bool chosen;

    private void Awake() {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void Setup(RewardPedestal pedestal) {
        this.pedestal = pedestal;

        SetupDisplay();
    }


    private void SetupDisplay() {
        displayItem = pedestal.rewardItem.GetDisplayItem();
        rewardImage.sprite = displayItem.Data.itemIcon;

        if(displayItem.Data.Type == ItemType.Skill) {
            displayAbility = displayItem.Data.learnableAbilities[0].FetchAbilityForDisplay(EntityManager.ActivePlayer);
        }

        if(displayItem.Data.Type == ItemType.Currency) {
            goldAnim.SetActive(true);
            rewardImage.gameObject.SetActive(false);
        }

        if (displayItem.Data.Type == ItemType.SkillPoint) {
            skillPointAnim.SetActive(true);
            rewardImage.gameObject.SetActive(false);
        }


        if (pedestal.enforceCost == true) {
            costArea.SetActive(true);
            costTextField.text = pedestal.rewardItem.itemValue.ToString();
        }
        else {
            costArea.SetActive(false);
        }
    }


    #region UI CALLBACKS

    public void OnPointerEnter(PointerEventData eventData) {

        switch (displayItem.Data.Type) {
            case ItemType.None:
                break;
            case ItemType.Equipment:
                TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance["Item Header"]));
                break;
            case ItemType.Rune:
                TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, Color.cyan));
                break;
            case ItemType.Currency:
                TooltipManager.Show("A cluster of Aetherium Crystals");
                break;
            case ItemType.Skill:
                TooltipManager.Show(displayAbility.GetTooltip(), displayAbility.Data.abilityName);
                break;

            case ItemType.ClassSelection:
                TooltipManager.Show(displayItem.Data.itemDescription, TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance[displayItem.Data.itemName]));

                break;

            case ItemType.SkillPoint:
                TooltipManager.Show("Skill Point");
                break;
            default:
                break;
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    public void OnPointerClick(PointerEventData eventData) {

        if (chosen == true)
            return;

        chosen = true;

        if(pedestal.enforceCost == false) {
            RoomManager.OnRewardSelected(pedestal);
            TooltipManager.Hide();
        }
        else {
            if(EntityManager.ActivePlayer.Inventory.TryBuyItem(displayItem)  == true) {
                RoomManager.OnRewardSelected(pedestal);
                TooltipManager.Hide();
            }
        }
        
      
    }

    #endregion
}
