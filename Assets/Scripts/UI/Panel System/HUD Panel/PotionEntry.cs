using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LL.Events;

public class PotionEntry : MonoBehaviour {
    public Image potionImage;

    public Sprite fullSprite;
    public Sprite emptySprite;

    public ParticleSystem fillVFX;
    public ParticleSystem emptyVFX;


    public void Fill() {
        potionImage.sprite = fullSprite;
        fillVFX.Play();
    }

    public void Empty() {
        potionImage.sprite = emptySprite;
        emptyVFX.Play();
    }




}
