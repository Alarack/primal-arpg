using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LL.Events;
using System.Text;


public class InventoryItemEntry : InventoryBaseEntry {


    private InventoryPanel parentPanel;
    private Color defaultBgColor;

    protected override void Awake() {
        base.Awake();
        defaultBgColor = bgImage.color;
    }

    protected override void OnEnable() {
        base.OnEnable();
        //EventManager.RegisterListener(GameEvent.ItemEquipped, OnItemEquipped);
        EventManager.RegisterListener(GameEvent.ItemDropped, OnItemDropped);
        //EventManager.RegisterListener(GameEvent.ItemUnequipped, OnItemUnequipped);
    }

    protected override void OnItemEquipped(EventData data) {
        Item item = data.GetItem("Item");


        //Debug.Log("A Slot: " + slot + " has detected an equip for: " + item.Data.itemName + ".  Current Slot: " + item.CurrentSlot);

        if (item.CurrentSlot == slot) {
            Add(item);
        }

        if (MyItem != null && item == MyItem && slot == ItemSlot.Inventory ) {
            Remove();
        }

        if(MyItem != null && slot != ItemSlot.Inventory && slot != ItemSlot.ForgeSlot) {
            parentPanel.CheckForDupeEquips(this);
        }
    }

    protected override void OnItemUnequipped(EventData data) {
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
        bool drop = data.GetBool("Drop");

        Debug.Log("Inventory Entry sees a dropped item: " + item.Data.itemName);

        if (MyItem != null && item == MyItem) {
            Debug.Log("Item Matches: " + MyItem.Data.itemName);

            Remove();

            if(drop == true)
                ItemSpawner.SpawnItem(item, Vector2.zero);
        }
    }

    public void Setup(Item item, InventoryPanel parentPanel) {
        this.parentPanel = parentPanel;
        this.MyItem = item;

        SetupDisplay();
    }

    public void ShowHighlight() {
        bgImage.color = Color.yellow;
    }

    public void HideHighlight() {
        bgImage.color = defaultBgColor;
    }

    #region UI CALLBACKS

    public override void OnBeginDrag(PointerEventData eventData) {
        if (slot == ItemSlot.ForgeSlot)
            return;
        
        DraggedInventoryItem = this;

        //DraggedInventoryItem.canvas.sortingOrder = 100;

        UIHelper.SetCanvasLayerOnTop(canvas);

        parentPanel.HighlightValidSLots();
        parentPanel.dropZone.SetActive(true);
    }

    public override void OnDrag(PointerEventData eventData) {
        if (slot == ItemSlot.ForgeSlot)
            return;

        base.OnDrag(eventData);
    }

    public override void OnDrop(PointerEventData eventData) {

        if (DraggedInventoryItem == null)
            return;

        Item draggedItem = DraggedInventoryItem.MyItem;

        if(draggedItem == null) 
            return;
        
        if(slot == ItemSlot.ForgeSlot) {
            Debug.Log("Forging: " + draggedItem.Data.itemName);
            Add(draggedItem);
            parentPanel.SetupItemAffixSlots();
            return;
        }


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
        else if(slot != ItemSlot.ForgeSlot) {
            if (draggedItem.Data.validSlots.Contains(slot)) {
                EntityManager.ActivePlayer.Inventory.EquipItemToSlot(draggedItem, slot);
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData) {
        if (slot == ItemSlot.ForgeSlot)
            return;

        base.OnEndDrag(eventData);
        parentPanel.HideAllHighlights();
        parentPanel.dropZone.SetActive(false);
    }

    public override void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            if (MyItem == null)
                return;

            if (slot == ItemSlot.ForgeSlot) {
                Remove();
                return;
            }


            if (MyItem.Equipped == true) {
                //parentPanel.AddToFirstEmptySlot(MyItem);
                EntityManager.ActivePlayer.Inventory.UnEquipItem(MyItem);
            }
            else {
                EntityManager.ActivePlayer.Inventory.EquipItem(MyItem);
            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);

        //if(MyItem == null && slot == ItemSlot.ForgeSlot) {
        //    //TooltipManager.Show("Drag an item into the Forget Slot to begin. Then pick an empty slot and click Forge. You can forge any number of times in the same slot to re-roll for a desired affix. Click on a desired Affix to attach it to the current Item. You can reforge Affixes even after you've chosen.", "Forging");
        //    ShowForgeInfo();
        //}

    }


    

    #endregion
}
