using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LL.Events;

public class PotionEntry : MonoBehaviour {
    public Image potionImage;

    public Sprite fullSprite;
    public Sprite emptySprite;


    public void Fill() {
        potionImage.sprite = fullSprite;
    }

    public void Empty() {
        potionImage.sprite = emptySprite;
    }




}
