using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionPanel : BasePanel
{

    [SerializeField] private CanvasGroup dimmerFader;
    [SerializeField] private float dissolveTime = 0.75f;
    [SerializeField] private Image transitionImage;

    private int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");


    private Action onTransitionComplete;

    protected override void Awake() {
        base.Awake();
        transitionImage.material = new Material(transitionImage.material);
    }


    public void Setup(Action onTransitionComplete) {
        this.onTransitionComplete = onTransitionComplete;

        BeginTransition();
    }


    private void BeginTransition() {
        Tween transitionTween = transitionImage.material.DOFloat(0f, _dissolveAmount, dissolveTime);
        transitionTween.onComplete += OnMidTransition;
        dimmerFader.DOFade(0.8f, dissolveTime);
    }

    public void OnMidTransition() {
        //EndTransition();
        //onTransitionComplete?.Invoke();
        new Task(HoldTime());
    }

    private IEnumerator HoldTime() {
        yield return new WaitForSeconds(0.3f);

        EndTransition();
        onTransitionComplete?.Invoke();
    }

    private void EndTransition() {
        Tween transitionTween = transitionImage.material.DOFloat(1.1f, _dissolveAmount, dissolveTime * 0.75f);
        transitionTween.onComplete += OnEndTransition;
        dimmerFader.DOFade(0f, dissolveTime * 0.75f);


    }

    private void OnEndTransition() {

        Close();
    }

}
