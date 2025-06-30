using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadoutSelectionEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {


    public Image itemImage;


    public GameObject selectionHighlight;


    private Item displayItem;
    private Ability displayAbility;

    public ItemDefinition ItemDef { get; private set; }
    public bool IsSelected { get { return selectionHighlight.activeSelf == true; } }


    private LoadoutEntry parentEntry;


    public void Setup(ItemDefinition item, LoadoutEntry parentEntry) {
        this.parentEntry = parentEntry;
        ItemDef = item;
        displayItem = item.itemData.GetDisplayItem();

        if (displayItem.Data.Type == ItemType.Skill) {
            displayAbility = displayItem.Data.learnableAbilities[0].FetchAbilityForDisplay(EntityManager.ActivePlayer);
        }

        itemImage.sprite = displayItem.Data.itemIcon;
    }


    public void Select() {
        selectionHighlight.SetActive(true);

        SaveLoadUtility.SaveData.SaveLoadoutSelection(parentEntry.loadoutType, ItemDef.itemData.itemName);
    }

    public void Deselect() {
        selectionHighlight.SetActive(false);
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

    public void OnPointerClick(PointerEventData eventData) {

        if (eventData.button == PointerEventData.InputButton.Left) {
            parentEntry.OnItemSelected(this);
        }

        if (eventData.button ==  PointerEventData.InputButton.Right && parentEntry.loadoutType == LoadoutEntry.LoadoutEntryType.Item) {
            if(IsSelected == true)
                parentEntry.Unselect(this);
        }
        
    }
}
