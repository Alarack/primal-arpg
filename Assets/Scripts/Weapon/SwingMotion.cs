using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwingMotion : MonoBehaviour
{
    public Transform ownerTransform;
    public Transform pivotPoint;
    //public Transform weaponTransform;
    public float swingTime;
    public Ease weaponSwingEase;
    public Collider2D swingWeaponCollider;

    public GameObject swingVFX;

    private Tween swingTween;
    private bool swingLeft;


    private void Start() {
        swingTween = pivotPoint.DOLocalRotate(new Vector3(0f, 0f, 240f), swingTime, RotateMode.LocalAxisAdd);
        swingTween.SetEase(weaponSwingEase);
        swingTween.SetAutoKill(false);
        swingTween.onComplete += OnSwingComplete;
        swingTween.onRewind += OnSwingComplete;
        swingTween.Pause();
    }


    private void Update() {
        if(Input.GetMouseButtonDown(0)) {
            Swing();
        
        }
    }

    private void Swing() {
        if (swingTween.IsPlaying() == true)
            return;

        //swingWeaponCollider.enabled = true;
        //swingParticles.Play();

        if (swingLeft == false)
            SwingRight();
        else
            SwingLeft();

        swingLeft = !swingLeft;
    }

    private void SwingRight() {
        StartCoroutine(OnSwingHalfWay());

        GameObject activeVFX = Instantiate(swingVFX, pivotPoint.position, ownerTransform.rotation);
        activeVFX.SetActive(true);
        
        swingTween.PlayForward();

    }

    private void SwingLeft() {
        StartCoroutine(OnSwingHalfWay());

        GameObject activeVFX = Instantiate(swingVFX, pivotPoint.position, ownerTransform.rotation);
        activeVFX.SetActive(true);
 
        Quaternion flippedRot = Quaternion.Euler(-90f, 0f, 0f);
        activeVFX.transform.Find("Slash").localRotation = flippedRot;

        swingTween.PlayBackwards();
    }

    private IEnumerator OnSwingHalfWay() {
        WaitForSeconds waiter = new WaitForSeconds(swingTime / 2f);
        yield return waiter;

        HalfSwingCallback();
    }

    private void HalfSwingCallback() {
        //TestHitbox hit = Instantiate(meleeHitbox, meleeAnchor.transform.position, Quaternion.identity);
        //hit.SetOnHitCallback(meleeWeapon.OnHit);
    }

    private void OnSwingComplete() {
        swingWeaponCollider.enabled = false;
        //swingParticles.Stop();
    }

}
