using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Text;

public class ItemAffixSlotEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {



    public TextMeshProUGUI affixText;
    //public Image affixBG;
    public Image borderImage;
    public Image affixIconImage;
    public Image rarityGemImage;
    public GameObject rarityHolder;
    public GameObject selectorArrow;
    public ParticleSystem upgradeVFX;


    private Item currentItem;
    private InventoryPanel inventoryPanel;

    private Color baseBorderColor;

    public ItemData AffixData { get; private set; }

    private void Awake() {
        rarityHolder.SetActive(false);
    }

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

    public void UpgradeAffix() {
        if (AffixData == null)
            return;
        
        AffixData.tier++;
        SetupDisplay();
    }


    public void Select() {
        borderImage.color = Color.white;
        if(selectorArrow != null)
            selectorArrow.SetActive(true);
    }

    public void Deselect() {
        borderImage.color = baseBorderColor;
        if (selectorArrow != null)
            selectorArrow.SetActive(false);
    }

    private void SetupDisplay() {
        if (AffixData == null) {
            if(affixText != null)
                affixText.text = "Empty";
            if (affixIconImage != null)
                affixIconImage.gameObject.SetActive(false);

            rarityHolder.SetActive(false);
            return;
        }

        if(affixText != null)
            affixText.text = AffixData.GetShortTooltip();

        rarityHolder.SetActive(true);

        rarityGemImage.sprite = AffixData.GetAffixRarityGem();

        if(affixIconImage != null) {
            Sprite icon = AffixData.GetAffixIcon();

            if(icon != null) {
                affixIconImage.gameObject.SetActive(true);
                affixIconImage.sprite = icon;
                //affixIconImage.color = AffixData.GetTierColor(AffixData.tier);
            }
            else {
                affixIconImage.gameObject.SetActive(false);
            }
        }
    }


    #region UI CALLBACKS

    public void OnPointerClick(PointerEventData eventData) {
        inventoryPanel.OnAffixSlotSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if(AffixData != null)
            TooltipManager.Show(AffixData.GetAffixTooltip(), AffixData.GetShortTooltip());
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    #endregion
}
