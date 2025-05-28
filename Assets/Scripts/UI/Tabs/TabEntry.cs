using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;
using DG.Tweening;
using Michsky.MUIP;

public class TabEntry : MonoBehaviour, IPointerClickHandler
{

    //public GameObject selectedHighlight;
    public GameObject showableArea;
    private CanvasGroup showableAreaFader;
    public UIGradient selectedHighlightGradient;


    public UnityEvent onSelect;

    public bool IsSelected { get; set; }

    private TabManager tabManager;


    private void Awake() {
        showableAreaFader = showableArea.GetComponent<CanvasGroup>();
        selectedHighlightGradient.Offset = -0.6f;
    }

    public void Setup(TabManager manager) {
        tabManager = manager;
    }


    public void Select() {
        IsSelected = true;
        if (showableArea != null)
            FadeIn();
        
        //if (selectedHighlight != null)
        //    selectedHighlight.SetActive(true);

        onSelect?.Invoke();
    }

    public void Deselect() {
        if (IsSelected == false)
            return;
        
        IsSelected = false;
        if(showableArea != null)
            FadeOut();

        //if(selectedHighlight != null)
        //    selectedHighlight.SetActive(false);
    }

    private void FadeIn() {
        tabManager.transitionInProgress = true;
        showableArea.SetActive(true);
        showableAreaFader.alpha = 0f;
        Tween fadeIn = showableAreaFader.DOFade(1f, 0.3f).SetUpdate(true);
        fadeIn.onComplete += OnFadeInComplete;

        
        if (selectedHighlightGradient != null) {
            selectedHighlightGradient.Offset = -0.6f;
            DOTween.To(() => selectedHighlightGradient.Offset, x => selectedHighlightGradient.Offset = x, 0f, 0.3f);
        }

    }

    private void FadeOut() {
        Tween fadeOut = showableAreaFader.DOFade(0f, 0.3f).SetUpdate(true);
        fadeOut.onComplete += OnFadeOutComplete;

        if (selectedHighlightGradient != null)
            DOTween.To(() => selectedHighlightGradient.Offset, x => selectedHighlightGradient.Offset = x, -0.6f, 0.3f).SetUpdate(true);
    }

    private void OnFadeOutComplete() {
        showableArea.SetActive(false);
    }

    private void OnFadeInComplete() {
        tabManager.transitionInProgress = false;
    }


    public void OnClick() {
        if (IsSelected == true)
            return;

        tabManager.OnTabSelected(this);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (IsSelected == true)
            return;

        tabManager.OnTabSelected(this);
    }
}
