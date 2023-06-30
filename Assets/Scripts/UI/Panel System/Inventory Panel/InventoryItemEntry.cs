using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LL.Events;

public class InventoryItemEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerClickHandler {



    public Image itemImage;
    public Image bgImage;
    public Image emptyImage;

    public ItemSlot slot = ItemSlot.Inventory;

    public Item MyItem { get; private set; }


    private int baseOrder;
    private InventoryPanel parentPanel;
    private Canvas canvas;

    public static InventoryItemEntry DraggedInventoryItem { get; set; }


    private void Awake() {
        canvas = GetComponent<Canvas>();
        baseOrder = canvas.sortingOrder;
    }

    private void OnEnable() {
        EventManager.RegisterListener(GameEvent.ItemEquipped, OnItemEquipped);
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }

    private void OnItemEquipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item.CurrentSlot == slot) {
            Add(item);
        }

        if (slot == ItemSlot.Inventory) {
            Remove();
        }

    }

    public void Setup(Item item, InventoryPanel parentPanel) {
        this.parentPanel = parentPanel;
        this.MyItem = item;

        SetupDisplay();
    }

    public void Add(Item item) {
        MyItem = item;
        SetupDisplay();
    }

    public void Remove() {
        MyItem = null;
        SetupDisplay();
    }

    public void ShowHighlight() {

    }

    public void HideHighlight() {

    }

    private void SetupDisplay() {
        if (MyItem != null) {
            itemImage.sprite = MyItem.Data.itemIcon;

            if (emptyImage != null)
                emptyImage.gameObject.SetActive(false);
        }
        else if (emptyImage != null) {
            emptyImage.gameObject.SetActive(true);
        }

    }

    #region UI CALLBACKS

    public void OnBeginDrag(PointerEventData eventData) {
        DraggedInventoryItem = this;
        parentPanel.HighlightValidSLots();
    }

    public void OnDrag(PointerEventData eventData) {
        DraggedInventoryItem.itemImage.gameObject.transform.localPosition = Input.mousePosition;
    }

    public void OnDrop(PointerEventData eventData) {
        if (slot == ItemSlot.Inventory) {
            DraggedInventoryItem.itemImage.gameObject.transform.localPosition = Vector3.zero;
        }
        else {
            if (DraggedInventoryItem.MyItem.Data.validSlots.Contains(slot)) {
                EntityManager.ActivePlayer.Inventory.EquipItem(DraggedInventoryItem.MyItem);
            }
        }



    }

    public void OnEndDrag(PointerEventData eventData) {
        DraggedInventoryItem.itemImage.gameObject.transform.localPosition = Vector3.zero;
        DraggedInventoryItem = null;
        parentPanel.HideAllHighlights();
    }

    public void OnPointerEnter(PointerEventData eventData) {

    }

    public void OnPointerExit(PointerEventData eventData) {

    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            if (MyItem == null)
                return;

            if (MyItem.Equipped == true) {
                EntityManager.ActivePlayer.Inventory.UnEquipItem(MyItem);
            }
            else {
                EntityManager.ActivePlayer.Inventory.EquipItem(MyItem);
            }
        }
    }

    #endregion
}
