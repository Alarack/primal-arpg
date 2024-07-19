using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RuneChoiceEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {


    public Image runeImage;
    public Image selectionImage;
    public GameObject shimmer;
    public CanvasGroup imagefader;
    public Item RuneItem { get; private set; }
    public bool IsSelected { get; private set; }

    private RuneGroupEntry parentGroup;
    private RunesPanel runesPanel;

    private Vector2 shimmerStartPos;

    public void Setup(Item runeItem, RuneGroupEntry parentGroup, RunesPanel runesPanel) {
        this.parentGroup = parentGroup;
        this.runesPanel = runesPanel;
        RuneItem = runeItem;
        shimmerStartPos = shimmer.transform.localPosition;

        SetupDisplay();
    }

    private void SetupDisplay() {
        runeImage.sprite = RuneItem.Data.itemIcon;
    }

    public void Select() {
        selectionImage.gameObject.SetActive(true);
        IsSelected = true;
        TooltipManager.Hide();
        shimmer.transform.localPosition = shimmerStartPos;
        shimmer.transform.DOLocalMove(new Vector2(64f, -64f), 1f);
        imagefader.DOFade(1f, 1f);

        if(RuneItem.Equipped == false) {
            EntityManager.ActivePlayer.Inventory.EquipRune(RuneItem, runesPanel.CurrentAbility);
            //runesPanel.CurrentAbility.equippedRunes.Add(RuneItem);
        }

    }

    public void Deselect() {
        selectionImage.gameObject.SetActive(false);
        IsSelected = false;
        imagefader.DOFade(0.5f, 1f);

        if(RuneItem.Equipped == true) {
            EntityManager.ActivePlayer.Inventory.UnEquipRune(RuneItem, runesPanel.CurrentAbility);
            //runesPanel.CurrentAbility.equippedRunes.Remove(RuneItem);

            //Debug.Log("Unequipping: " + RuneItem.Data.itemName);
        }

    }


    #region UI CALLBACKS

    public void OnPointerClick(PointerEventData eventData) {
       
        if(eventData.button == PointerEventData.InputButton.Left) {
            parentGroup.OnChoiceSelected(this);
        }

        if(eventData.button == PointerEventData.InputButton.Right) {
            Deselect();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Show(RuneItem.GetTooltip(), TextHelper.ColorizeText(RuneItem.Data.itemName, ColorDataManager.Instance["Burnt Orange"]));

    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    #endregion


}
