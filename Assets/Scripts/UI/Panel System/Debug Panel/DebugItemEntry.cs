using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DebugItemEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Image itemImage;
    public TextMeshProUGUI itemNameText;


    public ItemDefinition Item { get; private set; }


    private Item displayItem;
    private Ability displayAbility;

    public void Setup(ItemDefinition item) {
        Item = item;
        displayItem = item.itemData.GetDisplayItem();

        if (displayItem.Data.Type == ItemType.Skill) {
            displayAbility = displayItem.Data.learnableAbilities[0].FetchAbilityForDisplay(EntityManager.ActivePlayer);
        }

        itemImage.sprite = displayItem.Data.itemIcon;
        itemNameText.text = Item.itemData.itemName;
    }


    public void OnPointerClick(PointerEventData eventData) {
        ItemSpawner.SpawnItem(Item, EntityManager.ActivePlayer.transform.position, true);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        switch (displayItem.Data.Type) {
            case ItemType.Equipment:
                TooltipManager.Show(displayItem.GetTooltip(), TextHelper.ColorizeText(displayItem.Data.itemName, ColorDataManager.Instance["Item Header"]));
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
