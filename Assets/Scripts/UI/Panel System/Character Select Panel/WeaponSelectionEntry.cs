using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSelectionEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {


    public Image weaponIcon;
    public GameObject selectionFrame;

    private Item displayItem; 
    public ItemDefinition MyItem { get; private set; }
    private CharacterChoiceEntry parentCharacter;

    public void Setup(ItemDefinition itemDef, CharacterChoiceEntry parentCharacter) {
        this.MyItem = itemDef;
        this.parentCharacter = parentCharacter;
        displayItem = itemDef.itemData.GetDisplayItem();
        weaponIcon.sprite = itemDef.itemData.itemIcon;

        //Debug.Log("Setting up display for: " + itemDef.itemData.itemName);
    }


    public void Select() {
        selectionFrame.SetActive(true);
    }

    public void Deselect() {
        selectionFrame?.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) {
        parentCharacter.OnWeaponSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Show(displayItem.GetTooltip(), displayItem.Data.itemName);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

}
