using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LL.Events;

public class InventoryBaseEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerClickHandler {

    public Image itemImage;
    public Image bgImage;
    public Image emptyImage;

    public ItemSlot slot = ItemSlot.Inventory;
    public Item MyItem { get; protected set; }

    protected int baseOrder;
    protected Canvas canvas;

    public static InventoryBaseEntry DraggedInventoryItem { get; set; }



    protected virtual void Awake() {
        canvas = GetComponent<Canvas>();
        
        if(slot == ItemSlot.ForgeSlot) {
            return;
        }
        
        baseOrder = canvas.sortingOrder;
        canvas.overrideSorting = false;
    }

    protected virtual void OnEnable() {
        EventManager.RegisterListener(GameEvent.ItemEquipped, OnItemEquipped);
        EventManager.RegisterListener(GameEvent.ItemUnequipped, OnItemUnequipped);
    }

    protected virtual void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }

    protected virtual void SetupDisplay() {
        if (MyItem != null) {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = MyItem.Data.itemIcon;

            if (emptyImage != null)
                emptyImage.gameObject.SetActive(false);
        }
        else if (emptyImage != null) {
            emptyImage.gameObject.SetActive(true);
            itemImage.gameObject.SetActive(false);
        }
        else {
            itemImage.gameObject.SetActive(false);
        }

    }

    #region EVENTS

    protected virtual void OnItemEquipped(EventData data) {
        Item item = data.GetItem("Item");

        //if (item.CurrentSlot == slot) {
        //    Add(item);
        //}

        if (MyItem != null && item == MyItem && (slot == ItemSlot.Inventory)) {
            Remove();
        }

        //if (MyItem != null && slot != ItemSlot.Inventory) {
        //    parentPanel.CheckForDupeEquips(this);
        //}

    }

    protected virtual void OnItemUnequipped(EventData data) {
        Item item = data.GetItem("Item");

        if (slot == item.CurrentSlot && MyItem != null && item == MyItem) {
            Remove();
        }
    }

    #endregion  

    public void Add(Item item) {
        MyItem = item;
        SetupDisplay();
    }

    public void Remove() {

        MyItem = null;
        SetupDisplay();
    }


    #region UI CALLBACKS

    public void ResetSortOrder() {
        //canvas.sortingOrder = baseOrder;
        UIHelper.ResetCanvasLayer(canvas, baseOrder);
    }

    public virtual void OnBeginDrag(PointerEventData eventData) {
        DraggedInventoryItem = this;
        UIHelper.SetCanvasLayerOnTop(canvas);
    }

    public virtual void OnDrag(PointerEventData eventData) {
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DraggedInventoryItem.itemImage.gameObject.transform.position = targetPos;
    }

    public virtual void OnDrop(PointerEventData eventData) {

    }

    public virtual void OnEndDrag(PointerEventData eventData) {
        DraggedInventoryItem.ResetSortOrder();
        DraggedInventoryItem.itemImage.gameObject.transform.localPosition = Vector3.zero;
        DraggedInventoryItem = null;
        //parentPanel.HideAllHighlights();
        //parentPanel.dropZone.SetActive(false);
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {

        if (MyItem != null) {
            TooltipManager.Show(MyItem.GetTooltip(), TextHelper.ColorizeText( MyItem.Data.itemName, ColorDataManager.Instance["Item Header"]));

        }

    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    public virtual void OnPointerClick(PointerEventData eventData) {
        //if (eventData.button == PointerEventData.InputButton.Right) {
        //    if (MyItem == null)
        //        return;

            //if (MyItem.Equipped == true) {
            //    //parentPanel.AddToFirstEmptySlot(MyItem);
            //    EntityManager.ActivePlayer.Inventory.UnEquipItem(MyItem);
            //}
            //else {
            //    EntityManager.ActivePlayer.Inventory.EquipItem(MyItem);
            //}
        //}
    }

    #endregion

}
