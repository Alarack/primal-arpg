using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemPickupNamePlate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public Image backgroundImage;

    public Color defaultColor;
    public Color highlightColor;


    private ItemPickup parent;
    private CanvasGroup canvasGroup;
    private bool fading;

    private Task fadeTask;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    private void OnDisable() {
        EntityManager.ActivePlayer.CanAttack = true;
    }

    public void Setup(ItemPickup parent) {
        this.parent = parent;
    }


    public void Show() {
        if(fading == false) {
            fadeTask = new Task(Fade(1f));
        }

        EntityManager.ActivePlayer.CanAttack = false;
    }

    public void Hide() {
        if (fading == false) {
            fadeTask = new Task(Fade(0f));
        }

        EntityManager.ActivePlayer.CanAttack = true;
    }


    private IEnumerator Fade(float targetValue) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame(); 

        fading = true;

        while(canvasGroup.alpha != targetValue) {
            float desiredAlpha = Mathf.MoveTowards(canvasGroup.alpha, targetValue, Time.deltaTime * 3f);
            canvasGroup.alpha = desiredAlpha;
            yield return waiter;
        }

        fading = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(fadeTask != null) 
            fadeTask.Stop();
        
        parent.Collect();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        backgroundImage.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        backgroundImage.color = defaultColor;
    }
}
