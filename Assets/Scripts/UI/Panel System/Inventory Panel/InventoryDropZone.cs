using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler {
    
    
    
    
    
    
    public void OnDrop(PointerEventData eventData) {
        if(InventoryItemEntry.DraggedInventoryItem != null) {
            EntityManager.ActivePlayer.Inventory.Remove(InventoryItemEntry.DraggedInventoryItem.MyItem);
        }
    }
}
