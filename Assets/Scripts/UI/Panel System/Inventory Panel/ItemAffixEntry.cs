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
    public Image circleBorder;
    public Image innerCircle;

    [Header("VFX")]
    public CanvasGroup fader;
    public GameObject shimmer;
    public GameObject highlightShimmer;
    private Vector3 highlightShimmerStartPos;
    public ParticleSystem selectionEffect;
    public CanvasGroup flashFader;

    private Item currentItem;
    private InventoryPanel inventoryPanel;

    private ItemData affixData;

    private Color baseCircleBorderColor;
    private Color baseCircleFillColor;
    public Color highlightedCircleBorderColor;
    public Color highlightedCircleFillColor;


    private Task unRotateTask;

    public void Setup(InventoryPanel inventoryPanel, Item item, ItemData affixData) {
        this.inventoryPanel = inventoryPanel;
        this.currentItem = item;
        this.affixData = affixData;

        SetupDisplay();
        baseCircleBorderColor = circleBorder.color;
        baseCircleFillColor = innerCircle.color;
        fader.alpha = 0f;
        shimmer.transform.DOLocalMove(new Vector2(311f, 0f), 0.75f);
        highlightShimmerStartPos = highlightShimmer.transform.localPosition;
        fader.DOFade(1f, 0.3f);

    }

    private void OnDisable() {
        if(unRotateTask != null) 
            unRotateTask.Stop();
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
        //highlightShimmer.transform.localPosition = highlightShimmerStartPos;
        //highlightShimmer.transform.DOLocalMove(new Vector2(311f, 0f), 0.75f);
        circleBorder.color = highlightedCircleBorderColor;
        innerCircle.color = highlightedCircleFillColor;
        statIcon.transform.DORotate(new Vector3(0f, 180f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutSine);
    }

    public void OnPointerExit(PointerEventData eventData) {
        innerCircle.color = baseCircleFillColor;
        circleBorder.color = baseCircleBorderColor;
        unRotateTask = new Task(DelayedRotate());
    }

    private IEnumerator DelayedRotate() {
        yield return new WaitForSeconds(0.2f);
        statIcon.transform.DORotate(new Vector3(0f, 360, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutSine);

    }

    #endregion
}
