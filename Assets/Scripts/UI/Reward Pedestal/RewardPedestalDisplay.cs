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


    private RewardPedestal pedestal;
    private Item displayItem;
    private Ability displayAbility;

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

        if(pedestal.enforceCost == true) {
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
                TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance["Burnt Orange"]));
                break;
            case ItemType.Rune:
                TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, Color.cyan));
                break;
            case ItemType.Currency:
                break;
            case ItemType.Skill:
                TooltipManager.Show(displayAbility.GetTooltip(), displayAbility.Data.abilityName);
                break;

            case ItemType.ClassSelection:
                TooltipManager.Show(displayItem.Data.itemDescription, TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance[displayItem.Data.itemName]));

                break;
            default:
                break;
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    public void OnPointerClick(PointerEventData eventData) {
        
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
