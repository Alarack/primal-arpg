using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class TweenHelper : MonoBehaviour
{

    public enum TweenPreset {
        Breathing,
        Rotating,
    }

    public TweenPreset preset;

    public float endScale;
    public float scaleDuration;
    public float endRotatation;
    public float rotationDuration;

    private Vector3 initialScale;



    private void Awake() {
        initialScale = transform.localScale;
    }

    public void Start() {

        Action tweenMethod = preset switch {
            TweenPreset.Breathing => Breathe,
            TweenPreset.Rotating => Rotate,
            _ => null,
        };

        if(tweenMethod == null ) {
            Debug.LogError("Null tween preset in Tween Helper. You probably forgot to add it to the switch");
            return;
        }

        tweenMethod?.Invoke();
    }

    private void OnDisable() {
        transform.DOKill();
    }

    public void Breathe() {
        transform.DOScale(endScale, scaleDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void Rotate() {
        transform.DOLocalRotate(new Vector3(0f, 0f, endRotatation), rotationDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

}