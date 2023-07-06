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
    private Color defaultBgColor;


    public static InventoryItemEntry DraggedInventoryItem { get; set; }


    private void Awake() {
        canvas = GetComponent<Canvas>();
        baseOrder = canvas.sortingOrder;
        canvas.overrideSorting = false;
        defaultBgColor = bgImage.color;
    }

    private void OnEnable() {
        EventManager.RegisterListener(GameEvent.ItemEquipped, OnItemEquipped);
        EventManager.RegisterListener(GameEvent.ItemDropped, OnItemDropped);
        EventManager.RegisterListener(GameEvent.ItemUnequipped, OnItemUnequipped);
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }

    private void OnItemEquipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item.CurrentSlot == slot) {
            Add(item);
        }

        if (MyItem != null && item == MyItem && slot == ItemSlot.Inventory ) {
            Remove();
        }

        if(MyItem != null && slot != ItemSlot.Inventory) {
            parentPanel.CheckForDupeEquips(this);
        }

    }

    private void OnItemUnequipped(EventData data) {
        Item item = data.GetItem("Item");

        if(slot == item.CurrentSlot && MyItem != null && item == MyItem) {
            Remove();
        }

        //if(EntityManager.ActivePlayer.Inventory.ItemOwned(item) == true) {
        //    parentPanel.AddToFirstEmptySlot(item);
        //}
    }

    private void OnItemDropped(EventData data) {
        Item item = data.GetItem("Item");

        Debug.Log("Inventory Entry sees a dropped item: " + item.Data.itemName);

        if (MyItem != null && item == MyItem) {
            Debug.Log("Item Matches: " + MyItem.Data.itemName);

            Remove();

            ItemSpawner.SpawnItem(item, Vector2.zero);
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
        bgImage.color = Color.yellow;
    }

    public void HideHighlight() {
        bgImage.color = defaultBgColor;
    }

    private void SetupDisplay() {
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

    #region UI CALLBACKS

    public void ResetSortOrder() {
        //canvas.sortingOrder = baseOrder;
        UIHelper.ResetCanvasLayer(canvas, baseOrder);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        DraggedInventoryItem = this;

        //DraggedInventoryItem.canvas.sortingOrder = 100;

        UIHelper.SetCanvasLayerOnTop(canvas);

        parentPanel.HighlightValidSLots();
        parentPanel.dropZone.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DraggedInventoryItem.itemImage.gameObject.transform.position = targetPos;
    }

    public void OnDrop(PointerEventData eventData) {

        Item draggedItem = DraggedInventoryItem.MyItem;
        
        if (slot == ItemSlot.Inventory) {
            
            if(draggedItem.Equipped == true) {
                
                if(MyItem == null) {
                    Add(draggedItem);
                    EntityManager.ActivePlayer.Inventory.UnEquipItem(draggedItem);
                    
                }
                else {
                    Debug.LogWarning("Dragged an equipped item onto a non-null inventory item");
                }

            }
            else {

                if(MyItem == null) {
                    DraggedInventoryItem.Remove();
                    Add(draggedItem);
                }
                else {
                    //DraggedInventoryItem.Remove();

                    Item swappingItem = draggedItem;

                    DraggedInventoryItem.Add(MyItem);
                    Add(swappingItem);
                }

                
                //DraggedInventoryItem.itemImage.gameObject.transform.localPosition = Vector3.zero;

            }

        }
        else {
            if (draggedItem.Data.validSlots.Contains(slot)) {
                EntityManager.ActivePlayer.Inventory.EquipItemToSlot(draggedItem, slot);
            }
        }



    }

    public void OnEndDrag(PointerEventData eventData) {
        DraggedInventoryItem.ResetSortOrder();
        DraggedInventoryItem.itemImage.gameObject.transform.localPosition = Vector3.zero;
        DraggedInventoryItem = null;
        parentPanel.HideAllHighlights();
        parentPanel.dropZone.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {

        if(MyItem != null) {
            TooltipManager.Show(MyItem.GetTooltip(), MyItem.Data.itemName);

        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            if (MyItem == null)
                return;

            if (MyItem.Equipped == true) {
                //parentPanel.AddToFirstEmptySlot(MyItem);
                EntityManager.ActivePlayer.Inventory.UnEquipItem(MyItem);
            }
            else {
                EntityManager.ActivePlayer.Inventory.EquipItem(MyItem);
            }
        }
    }

    #endregion
}
