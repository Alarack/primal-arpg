using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillRuneEntry : InventoryBaseEntry
{


    private RunesPanel parentPanel;





    public void Setup(Item item, RunesPanel parentPanel, ItemSlot slot) {
        this.parentPanel = parentPanel;
        this.MyItem = item;
        this.slot = slot;

        SetupDisplay();
    }




    #region UI CALLBACKS

    public override void OnDrop(PointerEventData eventData) {

        Item draggedItem = DraggedInventoryItem.MyItem;

        if(draggedItem == null) {
            Debug.LogError("Dragged item is null when dropping a rune");
            return;
        }


        if (slot == ItemSlot.Inventory) {

            if (draggedItem.Equipped == true) {

                if (MyItem == null) {
                    Add(draggedItem);
                    EntityManager.ActivePlayer.Inventory.UnEquipRune(draggedItem, parentPanel.CurrentAbility);
                    parentPanel.CurrentAbility.equippedRunes.Remove(draggedItem);
                    //Debug.LogWarning("a rune: " + draggedItem.Data.itemName + " was dragged into an empty inventory slot");

                }
                else {
                    Debug.LogWarning("Dragged an equipped item onto a non-null inventory item");
                }

            }
            else {
                if (MyItem == null) {
                    DraggedInventoryItem.Remove();
                    Add(draggedItem);
                }
                else {
                    Item swappingItem = draggedItem;

                    DraggedInventoryItem.Add(MyItem);
                    Add(swappingItem);
                }
            }

        }
        else {
            if (draggedItem.Data.validSlots.Contains(slot)) {

                InventoryBaseEntry replacement = null;
                Item replacedItem = null;

                if (draggedItem.Equipped == true) {
                    return;
                    //EntityManager.ActivePlayer.Inventory.UnEquipRune(draggedItem);
                }


                if (MyItem != null) {
                    //EntityManager.ActivePlayer.Inventory.EquipRune(draggedItem);
                    //Add(draggedItem);

                 

                    replacement = DraggedInventoryItem;
                    replacedItem = MyItem;

                    EntityManager.ActivePlayer.Inventory.UnEquipRune(MyItem, parentPanel.CurrentAbility);
                    parentPanel.CurrentAbility.equippedRunes.Remove(MyItem);
                    Remove();
                    TooltipManager.Hide();
                    //DraggedInventoryItem.Add(MyItem);
                }

                
                EntityManager.ActivePlayer.Inventory.EquipRune(draggedItem, parentPanel.CurrentAbility);
                Add(draggedItem);
                parentPanel.CurrentAbility.equippedRunes.Add(draggedItem);

                if (replacement != null) {
                    replacement.Add(replacedItem);
                }

            }
        }

    }

    #endregion

}
