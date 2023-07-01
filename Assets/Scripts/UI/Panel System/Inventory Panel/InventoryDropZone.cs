using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler {
    
    
    
    
    
    
    public void OnDrop(PointerEventData eventData) {

        if(InventoryItemEntry.DraggedInventoryItem != null) {
           

            Item draggedItem = InventoryItemEntry.DraggedInventoryItem.MyItem;

            Debug.Log("Dropping: " + draggedItem.Data.itemName);

            //if (draggedItem.Equipped == true) {
            //    Debug.Log("Unequipping: " + draggedItem.Data.itemName);
            //    EntityManager.ActivePlayer.Inventory.UnEquipItem(draggedItem);
            //}

            EntityManager.ActivePlayer.Inventory.Remove(InventoryItemEntry.DraggedInventoryItem.MyItem);
        }
    }
}
