using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Text;
using DG.Tweening;

public class ItemAffixEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public TextMeshProUGUI affixText;
    public Image affixBG;
    public Image borderImage;
    public Image statIcon;

    [Header("VFX")]
    public CanvasGroup fader;
    public GameObject shimmer;
    public ParticleSystem selectionEffect;
    public CanvasGroup flashFader;

    private Item currentItem;
    private InventoryPanel inventoryPanel;

    private ItemData affixData;


    public void Setup(InventoryPanel inventoryPanel, Item item, ItemData affixData) {
        this.inventoryPanel = inventoryPanel;
        this.currentItem = item;
        this.affixData = affixData;

        SetupDisplay();

        fader.alpha = 0f;
        shimmer.transform.DOLocalMove(new Vector2(311f, 0f), 0.75f);
        fader.DOFade(1f, 0.3f);
    }

    public void ShowSelectionEffects() {
        selectionEffect.Play();
        flashFader.DOFade(0.8f, 0.35f);
    }

    private void SetupDisplay() {

        affixText.text = affixData.GetAffixTooltip();

        Sprite statIcon = affixData.GetAffixIcon();

        if(statIcon != null) {
            this.statIcon.gameObject.SetActive(true);
            Color tierColor = affixData.GetTierColor(affixData.tier);
            this.statIcon.sprite = statIcon;
            this.statIcon.color = tierColor;
        }
        else {
            this.statIcon.gameObject.SetActive(false);
        }
    }


    #region UI CALLBACKS

    public void OnPointerClick(PointerEventData eventData) {
        inventoryPanel.OnAffixSelected(affixData, this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        
    }

    public void OnPointerExit(PointerEventData eventData) {
        
    }

    #endregion
}
