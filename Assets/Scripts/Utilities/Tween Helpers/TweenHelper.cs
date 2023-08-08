using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenHelper : MonoBehaviour
{

    public enum TweenPreset {
        Breathing,
        Rotating,
    }

    public TweenPreset preset;

    public float endScale;
    public float scaleDuration;

    private Vector3 initialScale;


    private void Awake() {
        initialScale = transform.localScale;
    }

    public void Start() {
        Breathe();
    }

    private void OnDisable() {
        transform.DOKill();
    }

    public void Breathe() {
        transform.DOScale(endScale, scaleDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

}
