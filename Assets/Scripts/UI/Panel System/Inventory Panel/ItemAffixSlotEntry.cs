using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Text;

public class ItemAffixSlotEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {



    public TextMeshProUGUI affixText;
    public Image affixBG;
    public Image borderImage;



    private Item currentItem;
    private InventoryPanel inventoryPanel;

    private Color baseBorderColor;
    public ItemData AffixData { get; private set; }

    public void Setup(InventoryPanel inventoryPanel, Item item, ItemData affixData) {
        this.inventoryPanel = inventoryPanel;
        this.currentItem = item;
        baseBorderColor = borderImage.color;
        this.AffixData = affixData;

        SetupDisplay();
    }

    public void UpdateAffix(ItemData affixData) {
        this.AffixData = affixData;
        SetupDisplay();
    }


    public void Select() {
        borderImage.color = Color.white;
    }

    public void Deselect() {
        borderImage.color = baseBorderColor;
    }

    private void SetupDisplay() {
        if (AffixData == null) {
            affixText.text = "Empty";
            return;
        }

        affixText.text = AffixData.GetShortTooltip();
    }


    #region UI CALLBACKS

    public void OnPointerClick(PointerEventData eventData) {
        inventoryPanel.OnAffixSlotSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {

    }

    public void OnPointerExit(PointerEventData eventData) {

    }

    #endregion
}
