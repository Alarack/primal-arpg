using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemPickupTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public TextMeshProUGUI itemNameText;

    private ItemPickupNamePlate namePlate;

    private Canvas canvas;

    public ItemPickup ItemPickup { get; private set; }

    private void Awake() {
        canvas = GetComponentInParent<Canvas>();
        canvas.worldCamera = Camera.main;
    }

    public void Setup(ItemPickup pickup) {
        this.ItemPickup = pickup;
        itemNameText.text = pickup.Item.Data.itemName;

        namePlate = GetComponentInChildren<ItemPickupNamePlate>();
        namePlate.Setup(pickup);
    }

    
    public void OnPointerEnter(PointerEventData eventData) {

        if(ItemPickup.IsGrounde == false) 
            return;

        namePlate.Show();
    }

    public void OnPointerExit(PointerEventData eventData) {
        namePlate.Hide();
    }
}
