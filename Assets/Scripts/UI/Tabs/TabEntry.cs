using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

public class TabEntry : MonoBehaviour, IPointerClickHandler
{

    public GameObject selectedHighlight;
    public GameObject showableArea;

    public UnityEvent onSelect;

    public bool IsSelected { get; set; }

    private TabManager tabManager;

    public void Setup(TabManager manager) {
        tabManager = manager;
    }


    public void Select() {
        if (showableArea != null)
            showableArea.SetActive(true);
        
        if (selectedHighlight != null)
            selectedHighlight.SetActive(true);

        onSelect?.Invoke();
    }

    public void Deselect() {
        if(showableArea != null)
            showableArea.SetActive(false);

        if(selectedHighlight != null)
            selectedHighlight.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) {
        tabManager.OnTabSelected(this);
    }
}
