using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StarterItemDisplayEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image itemImage;

    private Item displayItem;
    private Ability displayAbility;

    public ItemDefinition ItemDef { get; private set; }

    public void Setup(ItemDefinition item) {
        ItemDef = item;
        displayItem = item.itemData.GetDisplayItem();

        if (displayItem.Data.Type == ItemType.Skill) {
            displayAbility = displayItem.Data.learnableAbilities[0].FetchAbilityForDisplay(EntityManager.ActivePlayer);
        }

        itemImage.sprite = displayItem.Data.itemIcon;
    }


    public void OnPointerEnter(PointerEventData eventData) {

        switch (displayItem.Data.Type) {
            case ItemType.Equipment:
                TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance["Burnt Orange"]));
                break;
         
            case ItemType.Skill:
                TooltipManager.Show(displayAbility.GetTooltip(), displayAbility.Data.abilityName);
                break;
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }
}
