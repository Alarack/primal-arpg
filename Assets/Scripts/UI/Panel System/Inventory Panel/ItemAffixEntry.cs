using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Text;

public class ItemAffixEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public TextMeshProUGUI affixText;
    public Image affixBG;
    public Image borderImage;



    private Item currentItem;
    private InventoryPanel inventoryPanel;

    private ItemData affixData;


    public void Setup(InventoryPanel inventoryPanel, Item item, ItemData affixData) {
        this.inventoryPanel = inventoryPanel;
        this.currentItem = item;
        this.affixData = affixData;

        SetupDisplay();
    }



    private void SetupDisplay() {
        StringBuilder builder = new StringBuilder();

        //for (int i = 0; i < affixData.statModifierData.Count; i++) {
        //    StatModifierData affixMod = affixData.statModifierData[i];
            


        //    builder.Append(TextHelper.ColorizeText( "Tier - ", affixData.GetTierColor(affixData.tier))).Append(affixData.GetTier())
        //         .Append(" ")
        //         .Append(affixMod.targetStat.ToString().SplitCamelCase())
        //         .Append(": ")
        //         .Append(TextHelper.FormatStat(affixMod.targetStat, affixMod.value))
                
        //         .AppendLine();
        //}

        affixText.text = affixData.GetAffixTooltip();
    }


    #region UI CALLBACKS

    public void OnPointerClick(PointerEventData eventData) {
        inventoryPanel.OnAffixSelected(affixData);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        
    }

    public void OnPointerExit(PointerEventData eventData) {
        
    }

    #endregion
}